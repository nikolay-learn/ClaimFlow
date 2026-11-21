using ClaimFlow.Api.Domain;

namespace ClaimFlow.Api.Tests.Domain;

public class ClaimTests
{
    private static Claim NewClaim() =>
        new()
        {
            PolicyNumber = "POL-123456",
            Description = "Water leak in the bathroom ceiling",
            ClaimedAmount = 900m,
            IncidentDate = new DateOnly(2026, 10, 1),
        };

    [Fact]
    public void New_claim_starts_as_submitted_without_a_note()
    {
        var claim = NewClaim();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Null(claim.ReviewerNote);
    }

    [Fact]
    public void Approve_moves_a_submitted_claim_to_approved()
    {
        var claim = NewClaim();

        claim.Approve("Plumber's invoice attached.");

        Assert.Equal(ClaimStatus.Approved, claim.Status);
        Assert.Equal("Plumber's invoice attached.", claim.ReviewerNote);
    }

    [Fact]
    public void Approving_a_rejected_claim_is_not_allowed()
    {
        var claim = NewClaim();

        claim.Reject("Not covered by a policy.");

        var error = Assert.Throws<ClaimRuleViolationException>(() => claim.Approve());

        Assert.Contains("Rejected", error.Message);
        Assert.Equal(ClaimStatus.Rejected, claim.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_requeres_a_reason(string reason)
    {
        var claim = NewClaim();

        Assert.Throws<ArgumentException>(() => claim.Reject(reason));
        Assert.Equal(ClaimStatus.Submitted, claim.Status);
    }

    [Fact]
    public void Paying_approved_claim_sets_status_Paid_and_updates_PaidAt()
    {
        var claim = NewClaim();
        claim.Approve("Looks ok");

        claim.MarkPaid();

        Assert.Equal(ClaimStatus.Paid, claim.Status);
        Assert.NotNull(claim.PaidAt);
    }

    [Fact]
    public void Paying_submitted_claim_throws_ClaimRuleViolationException()
    {
        var claim = NewClaim();

        Assert.Throws<ClaimRuleViolationException>(() => claim.MarkPaid());
    }
}
