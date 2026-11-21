using ClaimFlow.Api.Domain;

namespace ClaimFlow.Api.Data;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> ListAsync(
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default
    );
    Task<Claim?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
