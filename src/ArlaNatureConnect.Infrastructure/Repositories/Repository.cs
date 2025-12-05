using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Abstract;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

/// <summary>
/// Generic repository base implementing <see cref="IRepository{TEntity}"/> for EF Core.
/// </summary>
/// <typeparam name="TEntity">Entity type implementing <see cref="IEntity"/>.</typeparam>
/// <remarks>
/// <para>Inheriting XML Documentation with <c>&lt;inheritdoc/&gt;</c>:</para>
/// <para>
/// Each method below uses <c>&lt;inheritdoc/&gt;</c> to inherit documentation from the
/// <see cref="IRepository{TEntity}"/> interface. This keeps documentation DRY and ensures
/// interface-level docs appear in implementers and generated API docs.
/// </para>
/// <para>How to use:</para>
/// <list type="bullet">
/// <item>Document the contract on the interface (for example <see cref="IRepository{TEntity}"/>).</item>
/// <item>Apply <c>&lt;inheritdoc/&gt;</c> on implementing members to inherit that documentation.</item>
/// </list>
/// <example>
/// <code>
/// /// &lt;summary&gt;
/// /// Repository for <c>Person</c> entities.
/// /// &lt;/summary&gt;
/// public class PersonRepository : Repository<Person>
/// {
///     public PersonRepository(IDbContextFactory<AppDbContext> factory) : base(factory) { }
/// }
/// </code>
/// </example>
/// </remarks>
public abstract class Repository<TEntity>(IDbContextFactory<AppDbContext> factory) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly IDbContextFactory<AppDbContext> _factory = factory; // factory used to create AppDbContext instances

    #region CRUD Operations
    /// <inheritdoc/>
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        await dbSet.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);

        // Reload the persisted entity from the database so any DB-generated values (like Id) are populated.
        TEntity? persisted = await dbSet.FindAsync([entity.Id], cancellationToken);
        return persisted ?? entity;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        await dbSet.AddRangeAsync(entities, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return entities;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        TEntity? entity = await dbSet.FindAsync([id], cancellationToken);
        if (entity != null)
        {
            dbSet.Remove(entity);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<TEntity>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();

        TEntity? tracked = await dbSet.FindAsync([entity.Id], cancellationToken);
        if (tracked == null)
        {
            // No tracked entity in this context — attach but avoid re-attaching navigation graph
            ctx.Attach(entity);
            ctx.Entry(entity).State = EntityState.Modified;
        }
        else
        {
            // Copy scalar/foreign-key values into the tracked instance, avoid copying navigation objects
            ctx.Entry(tracked).CurrentValues.SetValues(entity);
            // If you need to update FK properties explicitly:
            // ctx.Entry(tracked).Property("RoleId").CurrentValue = entity.GetType().GetProperty("RoleId")?.GetValue(entity);
            // Leave navigation properties untouched or handle them explicitly to avoid duplicate instances.
        }

        await ctx.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
