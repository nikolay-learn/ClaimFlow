namespace ClaimFlow.Api.Domain;

public sealed class ClaimRuleViolationException(string message) : Exception(message);
