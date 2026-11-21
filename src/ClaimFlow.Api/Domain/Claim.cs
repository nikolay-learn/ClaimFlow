namespace ClaimFlow.Api.Domain;

/// <summary>
/// An insurance claim: a customer asks to be paid for a loss covered by a policy.
/// The class protects its own rules: status can only change through methods.
/// </summary>
public sealed class Claim
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public required string PolicyNumber { get; init; }
    public required string Description { get; init; }
    public decimal ClaimedAmount { get; init; }
    public DateOnly IncidentDate { get; init; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Submitted;

    public string? ReviewerNote { get; private set; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; private set; }

    public void StartReview()
    {
        EnsureStatus(ClaimStatus.Submitted);
        Status = ClaimStatus.InReview;
    }

    public void Approve(string? note = null)
    {
        EnsureStatus(ClaimStatus.Submitted, ClaimStatus.InReview);
        Status = ClaimStatus.Approved;
        ReviewerNote = note;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A rejection should explain why", nameof(reason));
        }

        EnsureStatus(ClaimStatus.Submitted, ClaimStatus.InReview);
        Status = ClaimStatus.Rejected;
        ReviewerNote = reason;
    }

    public void MarkPaid()
    {
        EnsureStatus(ClaimStatus.Approved);
        Status = ClaimStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
    }

    private void EnsureStatus(params ClaimStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new ClaimRuleViolationException(
                $"Claim {Id} is {Status}; this action needs {string.Join(" or ", allowed)}"
            );
        }
    }
}
