using ClaimFlow.Api.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace ClaimFlow.Api.Infrastructure;

public sealed class ClaimRuleExceptionHandler(IProblemDetailsService problemDetails)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ClaimRuleViolationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetails.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails =
                {
                    Title = "The claim can't do that right now",
                    Detail = exception.Message,
                    Status = StatusCodes.Status409Conflict,
                },
            }
        );
    }
}
