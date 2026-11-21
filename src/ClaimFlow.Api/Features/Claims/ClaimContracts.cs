using System.ComponentModel.DataAnnotations;
using System.Data;
using ClaimFlow.Api.Domain;
using ClaimFlow.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace ClaimFlow.Api.Features.Claims;

public sealed record CreateClaimRequest(
    [Required, RegularExpression(@"^POL-\d{6}$", ErrorMessage = "Policy number must look like POL-123456")]
        string PolicyNumber,
    [Required, StringLength(2000, MinimumLength = 10)] string Description,
    [Range(0.01, 100000)] decimal ClaimedAmount,
    DateOnly IncidentDate
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var rules = validationContext.GetRequiredService<IOptions<ClaimRulesOptions>>().Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (ClaimedAmount > rules.MaxClaimAmount)
        {
            yield return new ValidationResult(
                $"Claims above {rules.MaxClaimAmount:NO} go through the large-loss process.",
                [nameof(ClaimedAmount)]
            );
        }

        if (IncidentDate > today)
        {
            yield return new ValidationResult("Incident date can't be in the future.", [nameof(IncidentDate)]);
        }

        if (IncidentDate < today.AddYears(-rules.FilingWindowYears))
        {
            yield return new ValidationResult(
                $"Claim must be filled within {rules.FilingWindowYears} years of the incident.",
                [nameof(IncidentDate)]
            );
        }
    }
};

public sealed record ReviewClaimRequest(string? Note) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var rules = validationContext.GetRequiredService<IOptions<ClaimRulesOptions>>().Value;

        if (Note?.Length is not null && Note.Length > rules.MaxNotesLength)
        {
            yield return new ValidationResult(
                $"Notes is too long. Maximum: {rules.MaxNotesLength} charactes.",
                [nameof(Note)]
            );
        }
    }
};

public sealed record RejectClaimRequest([Required, StringLength(int.MaxValue, MinimumLength = 5)] string Reason)
    : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var rules = validationContext.GetRequiredService<IOptions<ClaimRulesOptions>>().Value;

        if (Reason.Length > rules.MaxNotesLength)
        {
            yield return new ValidationResult(
                $"Reason is too long. Max {rules.MaxNotesLength} characters.",
                [nameof(Reason)]
            );
        }
    }
};

public sealed record ClaimResponse(
    Guid Id,
    string PolicyNumber,
    string Description,
    decimal ClaimAmount,
    DateOnly IncidentDate,
    ClaimStatus Status,
    string? ReviewNote,
    DateTimeOffset CreatedAt
)
{
    public static ClaimResponse From(Claim claim) =>
        new(
            claim.Id,
            claim.PolicyNumber,
            claim.Description,
            claim.ClaimedAmount,
            claim.IncidentDate,
            claim.Status,
            claim.ReviewerNote,
            claim.CreatedAt
        );
}
