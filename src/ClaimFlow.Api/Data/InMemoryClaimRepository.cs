using System.Collections.Concurrent;
using ClaimFlow.Api.Domain;

namespace ClaimFlow.Api.Data;

public sealed class InMemoryClaimRepository : IClaimRepository
{
    private readonly ConcurrentDictionary<Guid, Claim> _claims = new();

    public InMemoryClaimRepository()
    {
        foreach (var claim in SeedData.Claims())
        {
            _claims[claim.Id] = claim;
        }
    }

    public Task<IReadOnlyList<Claim>> ListAsync(
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<Claim> result = _claims
            .Values.Where(c => status is null || c.Status == status)
            .OrderByDescending(c => c.IncidentDate)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Claim?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_claims.GetValueOrDefault(id));

    public Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        _claims[claim.Id] = claim;
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_claims.TryRemove(id, out _));
}
