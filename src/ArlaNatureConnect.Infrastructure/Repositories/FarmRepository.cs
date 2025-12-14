using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class FarmRepository : Repository<Farm>, IFarmRepository
{
    public FarmRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }

    /// <summary>
    /// Returns a farm with the specified CVR number, if one exists.
    /// </summary>
    /// <param name="cvr">The CVR number to search for.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The farm with the matching CVR, or null if not found.</returns>
    public async Task<Farm?> GetByCvrAsync(string cvr, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cvr))
            return null;

        try
        {
            await using AppDbContext ctx = _factory.CreateDbContext();
            return await ctx.Set<Farm>()
                .FirstOrDefaultAsync(f => f.CVR != null && f.CVR == cvr.Trim(), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
