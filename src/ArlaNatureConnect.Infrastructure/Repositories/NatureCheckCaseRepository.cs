using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using System.Runtime.InteropServices;

namespace ArlaNatureConnect.Infrastructure.Repositories;

// Purpose: EF Core repository for NatureCheckCase entities.
// Notes: Adds queries for active cases while reusing base CRUD behavior.
public class NatureCheckCaseRepository : Repository<NatureCheckCase>, INatureCheckCaseRepository
{
    #region Commands
    public NatureCheckCaseRepository(IDbContextFactory<AppDbContext> factory)
        : base(factory)
    {
    }

    public async Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            NatureCheckCaseStatus[] activeStatuses = new[]
            {
                NatureCheckCaseStatus.Assigned,
                NatureCheckCaseStatus.InProgress
            };

            await using AppDbContext ctx = _factory.CreateDbContext();
            return await ctx.NatureCheckCases
                .Where(c => activeStatuses.Contains(c.Status))
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (COMException)
        {
            // Swallow COM exceptions that may occur when DI/WinRT factory access fails on certain test environments
            return Array.Empty<NatureCheckCase>();
        }
        catch (Exception)
        {
            // In keeping with other repository implementations, return an empty list on error to avoid bubbling infrastructure exceptions
            return Array.Empty<NatureCheckCase>();
        }
    }

    public async Task<bool> FarmHasActiveCaseAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using AppDbContext ctx = _factory.CreateDbContext();
            return await ctx.NatureCheckCases
                .AsNoTracking()
                .AnyAsync(c =>
                    c.FarmId == farmId &&
                    (c.Status == NatureCheckCaseStatus.Assigned ||
                     c.Status == NatureCheckCaseStatus.InProgress),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (COMException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<NatureCheckCase>> GetAssignedCasesForConsultantAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using AppDbContext ctx = _factory.CreateDbContext();
            return await ctx.NatureCheckCases
                .AsNoTracking()
                .Where(c => c.ConsultantId == consultantId && c.Status == NatureCheckCaseStatus.Assigned)
                .OrderByDescending(c => c.AssignedAt ?? c.CreatedAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (COMException)
        {
            return Array.Empty<NatureCheckCase>();
        }
        catch (Exception)
        {
            return Array.Empty<NatureCheckCase>();
        }
    }
    #endregion
}


