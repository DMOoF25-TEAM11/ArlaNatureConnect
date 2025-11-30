using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="Person"/> entities.
/// </summary>
/// <remarks>
/// This repository provides data access methods specific to the <see cref="Person"/> entity.
/// It inherits common CRUD functionality from <see cref="Repository{TEntity}"/> and
/// exposes domain-specific queries such as retrieving persons by role.
///
/// Rationale:
/// - Keep data access concerns encapsulated in infrastructure layer.
/// - Use domain identifiers (RoleId) instead of string role names when querying persons
///   to ensure referential integrity and avoid typos/case-sensitivity issues.
/// - Provide a friendly method to callers that accept a role name and internally resolve
///   the role entity to perform a safe lookup. If the role does not exist an empty list
///   is returned to avoid unexpected exceptions and to make the behaviour explicit.
///
/// Thread-safety and construction:
/// DbContext is not thread-safe. This repository is constructed using an
/// <see cref="IDbContextFactory{AppDbContext}"/>, and methods that interact with the
/// database create a short-lived <see cref="AppDbContext"/> instance for the duration of
/// the operation. That makes operations such as <see cref="GetPersonsByRoleAsync"/>
/// safe to call concurrently from multiple threads when the factory is registered in DI
/// (for example via <c>services.AddDbContextFactory&lt;AppDbContext&gt;()</c>).
///
/// Notes for maintainers:
/// - Prefer registering an <see cref="IDbContextFactory{AppDbContext}"/> for high-concurrency
///   scenarios so each operation uses a fresh context.
/// - The implementation returns an empty list when the requested role is not found or when
///   an error occurs while querying; callers should treat an empty result set as "none found"
///   and not necessarily as an error condition.
/// </remarks>
public class PersonRepository : Repository<Person>, IPersonRepository
{
    //private readonly IDbContextFactory<AppDbContext> _factory;

    /// <summary>
    /// Creates a new instance of <see cref="PersonRepository"/> using an
    /// <see cref="IDbContextFactory{AppDbContext}"/>.
    /// </summary>
    /// <param name="factory">Factory used to create short-lived <see cref="AppDbContext"/> instances.</param>
    public PersonRepository(IDbContextFactory<AppDbContext> factory)
        : base(factory)
    {
        //_factory = factory;
    }

    /// <summary>
    /// Returns all persons that have the specified role name.
    /// </summary>
    /// <remarks>
    /// The method first attempts to resolve the role by name (case-insensitive). If the
    /// role cannot be found in the <c>Roles</c> table, an empty list is returned. When a role
    /// is found the lookup uses the role's <see cref="Role.Id"/> (stored on the person as
    /// <see cref="Person.RoleId"/>) which is the correct and efficient way to filter
    /// persons by role in the database.
    ///
    /// This approach avoids directly comparing a textual role property on <see cref="Person"/>
    /// (which may not exist or be inconsistent) and enforces consistency via the domain
    /// relationship between Person and Role entities.
    ///
    /// The method creates a short-lived <see cref="AppDbContext"/> from the injected
    /// <see cref="IDbContextFactory{AppDbContext}"/> for the duration of the operation. It is
    /// therefore safe for concurrent use when the factory is registered in DI.
    /// </remarks>
    /// <param name="role">The name of the role to filter by (e.g. "Farmer").</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of <see cref="Person"/> that belong to the requested role. Returns an empty list if role not found or an error occurs.</returns>
    public async Task<IEnumerable<Person>> GetPersonsByRoleAsync(string role, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Enumerable.Empty<Person>();

        // Normalize and resolve role entity by name (case-insensitive)
        string normalized = role.Trim().ToLowerInvariant();

        try
        {
            await using AppDbContext ctx = _factory.CreateDbContext();

            Role? roleEntity = await ctx.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name.ToLower() == normalized, ct).ConfigureAwait(false);

            if (roleEntity == null)
                return Enumerable.Empty<Person>();

            return await ctx.Set<Person>()
                .Where(p => p.RoleId == roleEntity.Id)
                .ToListAsync(ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Consider logging the exception rather than swallowing.
            return Enumerable.Empty<Person>();
        }
    }
}