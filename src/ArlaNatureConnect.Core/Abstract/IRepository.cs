using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArlaNatureConnect.Core.Abstract;

/// <summary>
/// Defines a generic asynchronous CRUD repository abstraction.
/// Implementations may be purely in-memory or provide persistence.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    #region Create operations
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    #endregion
    #region Read operations
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    #endregion
    #region Update operations
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    #endregion
    #region Delete operations
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    #endregion
}
