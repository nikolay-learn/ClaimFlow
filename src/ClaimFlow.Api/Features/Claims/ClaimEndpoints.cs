using System.Threading.Tasks.Dataflow;
using ClaimFlow.Api.Data;
using ClaimFlow.Api.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClaimFlow.Api.Features.Claims;

public static class ClaimEndpoints
{
    public static IEndpointRouteBuilder MapClaimEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/claims").WithTags("Claims");

        group.MapGet("/", ListClaims);
        group.MapGet("/stats", GetStats);
        group.MapGet("/{id:guid}", GetClaim);
        group.MapPost("/", CreateClaim);
        group.MapPost("/{id:guid}/approve", ApproveClaim);
        group.MapPost("/{id:guid}/reject", RejectClaim);
        group.MapDelete("/{id:guid}", DeleteClaim);
        group.MapGet("/dashboard", Dashboard);

        return app;
    }

    private static async Task<Ok<List<ClaimResponse>>> ListClaims(
        ClaimStatus? status,
        IClaimRepository repository,
        CancellationToken cancellationToken
    )
    {
        var claims = await repository.ListAsync(status, cancellationToken);
        return TypedResults.Ok(claims.Select(ClaimResponse.From).ToList());
    }

    private static async Task<Ok<List<ClaimStatsRow>>> GetStats(
        IClaimRepository repository,
        CancellationToken cancellationToken
    )
    {
        var claims = await repository.ListAsync(cancellationToken: cancellationToken);
        return TypedResults.Ok(
            claims
                .GroupBy(c => c.Status)
                .Select(g => new ClaimStatsRow(g.Key, g.Count(), g.Sum(c => c.ClaimedAmount)))
                .OrderBy(c => c.Status)
                .ToList()
        );
    }

    private static async Task<Results<Ok<ClaimResponse>, NotFound>> GetClaim(
        Guid id,
        IClaimRepository repository,
        CancellationToken cancellationToken
    ) =>
        await repository.FindAsync(id, cancellationToken) is { } claim
            ? TypedResults.Ok(ClaimResponse.From(claim))
            : TypedResults.NotFound();

    private static async Task<Created<ClaimResponse>> CreateClaim(
        CreateClaimRequest request,
        IClaimRepository repository,
        CancellationToken cancellationToken = default
    )
    {
        var claim = new Claim
        {
            PolicyNumber = request.PolicyNumber,
            Description = request.Description,
            ClaimedAmount = request.ClaimedAmount,
            IncidentDate = request.IncidentDate,
        };

        await repository.AddAsync(claim, cancellationToken);
        return TypedResults.Created($"/claims/{claim.Id}", ClaimResponse.From(claim));
    }

    private static async Task<Results<Ok<ClaimResponse>, NotFound, Conflict<string>>> ApproveClaim(
        Guid id,
        ReviewClaimRequest request,
        IClaimRepository repository,
        CancellationToken cancellationToken
    )
    {
        if (await repository.FindAsync(id, cancellationToken) is not { } claim)
        {
            return TypedResults.NotFound();
        }

        claim.Approve(request.Note);
        return TypedResults.Ok(ClaimResponse.From(claim));
    }

    private static async Task<Results<Ok<ClaimResponse>, NotFound, Conflict<string>>> RejectClaim(
        Guid id,
        RejectClaimRequest request,
        IClaimRepository repository,
        CancellationToken cancellationToken
    )
    {
        if (await repository.FindAsync(id, cancellationToken) is not { } claim)
        {
            return TypedResults.NotFound();
        }

        claim.Reject(request.Reason);
        return TypedResults.Ok(ClaimResponse.From(claim));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteClaim(
        Guid id,
        IClaimRepository repository,
        CancellationToken cancellationToken
    ) =>
        await repository.RemoveAsync(id, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static async Task<Ok<DashboardResponse>> Dashboard(
        IClaimRepository repository,
        CancellationToken cancellationToken
    )
    {
        var submitted = repository.ListAsync(ClaimStatus.Submitted, cancellationToken);
        var inReview = repository.ListAsync(ClaimStatus.InReview, cancellationToken);

        await Task.WhenAll(submitted, inReview);

        return TypedResults.Ok(
            new DashboardResponse(submitted.Result.Count, inReview.Result.Count)
        );
    }
}

public sealed record ClaimStatsRow(ClaimStatus Status, int Count, decimal TotalClaimed);

public sealed record DashboardResponse(int Submitted, int InReview);
