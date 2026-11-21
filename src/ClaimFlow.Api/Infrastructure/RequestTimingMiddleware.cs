using System.Diagnostics;

namespace ClaimFlow.Api.Infrastructure;

public sealed class RequestTimingMiddleware(
    RequestDelegate next,
    ILogger<RequestTimingMiddleware> logger
)
{
    public const string CorrelationHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId =
            context.Request.Headers[CorrelationHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        long startedAt = Stopwatch.GetTimestamp();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);
            context.Response.Headers["X-Response-Time-Ms"] = elapsed.TotalMilliseconds.ToString(
                "0.0"
            );
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);
            logger.LogInformation(
                "{Method} {Path} responded {StatusCode} in {ElapsedMs:0.0} ms [{CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsed.TotalMilliseconds,
                correlationId
            );
        }
    }
}
