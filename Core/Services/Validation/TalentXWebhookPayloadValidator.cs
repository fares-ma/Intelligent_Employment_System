using FluentValidation;
using Services.Abstractions.TalentX.Models;

namespace Services.Validation;

public class TalentXWebhookPayloadValidator : AbstractValidator<TalentXWebhookPayload>
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "completed",
        "failed",
        "processing"
    };

    public TalentXWebhookPayloadValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();

        RuleFor(x => x.JobId)
            .GreaterThan(0);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status must be completed, failed, or processing.");

        When(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.FinalScore)
                .NotNull()
                .InclusiveBetween(0, 100);
        });

        When(x => string.Equals(x.Status, "failed", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.ErrorMessage)
                .NotEmpty()
                .MaximumLength(2000);
        });
    }
}
