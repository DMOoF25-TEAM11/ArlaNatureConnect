using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public abstract class Repository<TEntity>(IDbContextFactory<AppDbContext> factory) : IRepository<TEntity>
    where TEntity : class
{
    protected readonly IDbContextFactory<AppDbContext> _factory = factory;

    #region CRUD Operations
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        await dbSet.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        await dbSet.AddRangeAsync(entities, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
    }

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

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        DbSet<TEntity> dbSet = ctx.Set<TEntity>();
        dbSet.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
