using FluentValidation;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Services.Abstractions.TalentX;
using Services.TalentX;
using Services.Validation;
using Shared.Configuration;

namespace IES.api.Extensions;

/// <summary>
/// Registers TalentX typed HttpClient (with Polly resilience), orchestration services, and FluentValidation.
/// </summary>
public static class TalentXServiceRegistration
{
    public static IServiceCollection AddTalentXIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TalentXSettings>(configuration.GetSection(TalentXSettings.SectionName));

        var talentXSection = configuration.GetSection(TalentXSettings.SectionName);
        var baseUrl = talentXSection.GetValue<string>("BaseUrl") ?? "http://localhost:8008";
        var timeoutSeconds = talentXSection.GetValue("HttpTimeoutSeconds", 120);

        services.AddValidatorsFromAssemblyContaining<TalentXWebhookPayloadValidator>();

        services.AddScoped<IJobDescriptionBuilder, JobDescriptionBuilder>();
        services.AddScoped<ITalentXScoringService, TalentXScoringService>();

        // Typed HttpClient + Polly retry (via Microsoft.Extensions.Http.Resilience).
        services.AddHttpClient<ITalentXResumeScorerClient, TalentXResumeScorerClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        })
        .AddResilienceHandler("talentx-resilience", builder =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args =>
                {
                    var response = args.Outcome.Result;
                    if (response is null)
                        return ValueTask.FromResult(true);

                    return ValueTask.FromResult(
                        (int)response.StatusCode >= 500
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout);
                }
            });
        });

        return services;
    }
}
