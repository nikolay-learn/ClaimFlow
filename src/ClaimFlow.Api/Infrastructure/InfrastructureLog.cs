namespace ClaimFlow.Api.Infrastructure;

public static partial class InfrastructureLog
{
    private static readonly Func<ILogger, string, IDisposable?> _correlationScope = LoggerMessage.DefineScope<string>(
        "CorrelationId:{CorrelationId}"
    );

    public static IDisposable? BeginCorrelationScope(this ILogger logger, string correlationId) => _correlationScope(logger, correlationId);

    [LoggerMessage(9001, LogLevel.Information, "{Method} {Path} responded {StatusCode} in {ElapsedMs:0.0} ms")]
    public static partial void RequestCompleted(this ILogger logger, string method, string path, int statusCode, double elapsedMs);

    [LoggerMessage(9002, LogLevel.Information, "ClaimsDb connection string found")]
    public static partial void ClaimdDbConfigured(this ILogger logger);
}
