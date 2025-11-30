using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

// Purpose: EF Core repository for NatureCheckCase entities.
// Notes: Adds queries for active cases while reusing base CRUD behavior.
public class NatureCheckCaseRepository : Repository<NatureCheckCase>, INatureCheckCaseRepository
{
    #region Commands
    public NatureCheckCaseRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(CancellationToken cancellationToken = default)
    {
        // EF Core will automatically convert enum values to strings based on HasConversion configuration
        NatureCheckCaseStatus[] activeStatuses =
        [
            NatureCheckCaseStatus.Assigned,
            NatureCheckCaseStatus.InProgress
        ];

        return await _context.NatureCheckCases
            .Where(c => activeStatuses.Contains(c.Status))
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> FarmHasActiveCaseAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        // EF Core will automatically convert enum values to strings based on HasConversion configuration
        return await _context.NatureCheckCases
            .AsNoTracking()
            .AnyAsync(c =>
                c.FarmId == farmId &&
                (c.Status == NatureCheckCaseStatus.Assigned ||
                 c.Status == NatureCheckCaseStatus.InProgress),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<NatureCheckCase>> GetAssignedCasesForConsultantAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        return await _context.NatureCheckCases
            .AsNoTracking()
            .Where(c => c.ConsultantId == consultantId && c.Status == NatureCheckCaseStatus.Assigned)
            .OrderByDescending(c => c.AssignedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion
}


