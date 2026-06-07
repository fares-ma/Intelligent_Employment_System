using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Abstractions.TalentX;
using Services.Abstractions.TalentX.Models;
using Shared.Configuration;

namespace IES.api.Controllers;

/// <summary>
/// Receives async TalentX scoring callbacks. Primary path for applying MatchScore (no polling).
/// Placed in the main API project to guarantee route discovery on all hosting environments.
/// </summary>
[ApiController]
[Route("api/webhooks/talentx")]
[AllowAnonymous]
public class TalentXWebhookController : ControllerBase
{
    private const string IdempotencyHeaderName = "X-Idempotency-Key";
    private const string WebhookSecretHeaderName = "X-Webhook-Secret";

    private readonly ITalentXScoringService _scoringService;
    private readonly IValidator<TalentXWebhookPayload> _validator;
    private readonly TalentXSettings _settings;
    private readonly ILogger<TalentXWebhookController> _logger;

    public TalentXWebhookController(
        ITalentXScoringService scoringService,
        IValidator<TalentXWebhookPayload> validator,
        IOptions<TalentXSettings> options,
        ILogger<TalentXWebhookController> logger)
    {
        _scoringService = scoringService;
        _validator = validator;
        _settings = options.Value;
        _logger = logger;
    }

    [HttpPost("scoring")]
    public async Task<IActionResult> ReceiveScoringWebhook(
        [FromBody] TalentXWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        if (!ValidateWebhookSecret())
            return Unauthorized(new { message = "Invalid webhook secret." });

        var idempotencyKey = Request.Headers[IdempotencyHeaderName].FirstOrDefault()
            ?? payload.IdempotencyKey;

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { message = $"{IdempotencyHeaderName} header or idempotency_key body field is required." });

        var validation = await _validator.ValidateAsync(payload, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { message = "Invalid webhook payload.", errors = validation.Errors.Select(e => e.ErrorMessage) });

        try
        {
            var processed = await _scoringService.ProcessWebhookAsync(payload, idempotencyKey, cancellationToken);

            // Always 200 for duplicates so TalentX does not retry indefinitely.
            return Ok(new
            {
                success = true,
                duplicate = !processed,
                message = processed ? "Webhook processed." : "Duplicate webhook ignored (idempotent)."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TalentX webhook processing failed for key {Key}", idempotencyKey);
            throw;
        }
    }

    private bool ValidateWebhookSecret()
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            return true;

        var provided = Request.Headers[WebhookSecretHeaderName].FirstOrDefault();
        return string.Equals(provided, _settings.WebhookSecret, StringComparison.Ordinal);
    }
}
