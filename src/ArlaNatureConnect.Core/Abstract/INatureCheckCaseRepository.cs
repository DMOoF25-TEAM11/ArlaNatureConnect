using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

// Purpose: Defines persistence contracts for NatureCheckCase aggregates.
// Notes: Extends the generic repository with specific queries for active-case checks.
public interface INatureCheckCaseRepository : IRepository<NatureCheckCase>
{
    Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(CancellationToken cancellationToken = default);

    Task<bool> FarmHasActiveCaseAsync(Guid farmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all assigned nature check cases for a specific consultant.
    /// </summary>
    /// <param name="consultantId">The ID of the consultant.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A list of nature check cases with status "Assigned" for the specified consultant.</returns>
    Task<IReadOnlyList<NatureCheckCase>> GetAssignedCasesForConsultantAsync(Guid consultantId, CancellationToken cancellationToken = default);
}


