using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Services.Abstractions.TalentX;
using Services.Abstractions.TalentX.Models;
namespace Services.TalentX;

/// <summary>
/// Typed HttpClient for TalentX POST /api/v1/score and GET /api/v1/results.
/// Retry/circuit breaker are applied via HttpClientFactory resilience handler in Program.cs.
/// </summary>
public class TalentXResumeScorerClient : ITalentXResumeScorerClient
{
    public const string HttpClientName = "TalentXResumeScorer";

    private readonly HttpClient _httpClient;
    private readonly ILogger<TalentXResumeScorerClient> _logger;

    public TalentXResumeScorerClient(
        HttpClient httpClient,
        ILogger<TalentXResumeScorerClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TalentXScoreAcceptedResponse> InitiateScoringAsync(
        TalentXScoreRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.CandidateId.ToString()), "candidate_id");
        content.Add(new StringContent(request.JobId.ToString()), "job_id");
        content.Add(new StringContent(request.JobDescriptionJson), "job_description");

        var streamContent = new StreamContent(request.CvFileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(request.CvFileName));
        content.Add(streamContent, "cv_file", request.CvFileName);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/score")
        {
            Content = content
        };
        httpRequest.Headers.TryAddWithoutValidation("X-Idempotency-Key", idempotencyKey);

        _logger.LogInformation(
            "Initiating TalentX scoring for candidate {CandidateId}, job {JobId}, idempotency {Key}",
            request.CandidateId,
            request.JobId,
            idempotencyKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "TalentX score request failed with {StatusCode}: {Body}",
                response.StatusCode,
                body);
            throw new BadRequestException($"TalentX scoring request failed: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<TalentXScoreAcceptedResponse>(
            TalentXJsonSerializerOptions.WebhookDeserialize,
            cancellationToken);

        if (result is null || !result.Success)
            throw new BadRequestException("TalentX returned an invalid acceptance response.");

        return result;
    }

    public async Task<TalentXScoreResultResponse?> GetResultsAsync(
        int candidateId,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"/api/v1/results/{candidateId}/{jobId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TalentXScoreResultResponse>(
            TalentXJsonSerializerOptions.WebhookDeserialize,
            cancellationToken);
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TalentX health check failed");
            return false;
        }
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
