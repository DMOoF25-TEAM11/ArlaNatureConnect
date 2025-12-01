using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

/// <summary>
/// SQL Server repository for <see cref="Role"/> lookups used across UC flows.
/// </summary>
/// <remarks>
/// How to use and why we have it:
/// <list type="bullet">
/// <item><description>Use this repository to perform lookups and CRUD operations for <see cref="Role"/> entities.</description></item>
/// <item><description>It encapsulates data-access concerns and keeps callers decoupled from EF Core details.</description></item>
/// <item><description>When possible use <c>&lt;inheritdoc/&gt;</c> on implementing members to inherit documentation from interfaces or base types
/// so documentation stays consistent across implementations.</description></item>
/// </list>
/// <para />
/// Code example:
/// <code>
/// // create factory (DI normally provides this)
/// IDbContextFactory<AppDbContext> factory = /* ... */;
/// var repo = new RoleRepository(factory);
/// Role? r = await repo.GetByNameAsync("Farmer");
/// if (r != null) Console.WriteLine(r.Name);
/// </code>
/// </remarks>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(IDbContextFactory<AppDbContext> factory)
        : base(factory)
    {
    }

    /// <summary>
    /// Find a <see cref="Role"/> by its name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs a case-sensitive lookup of the role name using a short-lived
    /// <see cref="AppDbContext"/> created from the injected <see cref="IDbContextFactory{AppDbContext}"/>.
    /// </para>
    /// <para>
    /// Inheriting XML documentation example: if the interface or a base member provided
    /// documentation for this method you could write <c>&lt;inheritdoc/&gt;</c> here to reuse it.
    /// For example: <c>&lt;inheritdoc cref="ArlaNatureConnect.Core.Abstract.IRoleRepository.GetByNameAsync" /&gt;</c>
    /// </para>
    /// <example>
    /// <code>
    /// // Usage example
    /// IDbContextFactory<AppDbContext> factory = /* resolved from DI */;
    /// var repo = new RoleRepository(factory);
    /// Role? role = await repo.GetByNameAsync("Admin");
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="roleName">Name of the role to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching <see cref="Role"/>, or <c>null</c> when not found or on error.</returns>
    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        try
        {
            await using AppDbContext ctx = _factory.CreateDbContext();

            return await ctx.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Inline comment: Swallow exceptions (including COM exceptions) so UI layers calling this
            // method remain resilient — callers will receive null when an error occurs.
            return null;
        }
    }
}
