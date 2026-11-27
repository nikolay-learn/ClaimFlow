namespace ClaimFlow.Api.Features.Claims;

public static partial class ClaimLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Claim {ClaimId} submitted for policy {PolicyNumber}, amount {ClaimedAmount}"
    )]
    public static partial void ClaimSubmitted(this ILogger logger, Guid claimId, string policyNumber, decimal claimedAmount);

    [LoggerMessage(1002, LogLevel.Information, "Claim {ClaimId} approved")]
    public static partial void ClaimApproved(this ILogger logger, Guid claimId);

    [LoggerMessage(1003, LogLevel.Information, "Claim {ClaimId} rejected: {Reason}")]
    public static partial void ClaimRejected(this ILogger logger, Guid claimId, string reason);

    [LoggerMessage(1004, LogLevel.Debug, "Claim {ClaimId} not found")]
    public static partial void ClaimNotFound(this ILogger logger, Guid claimId);

    [LoggerMessage(1005, LogLevel.Warning, "Claim rule violated: {Details}")]
    public static partial void ClaimRuleViolated(this ILogger logger, string details);
}
