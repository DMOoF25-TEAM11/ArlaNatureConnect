using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Core.Abstract;

/// <summary>
/// Defines a generic asynchronous CRUD repository abstraction for domain entities.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <remarks>
/// This interface exists to decouple application logic from persistence details.
/// By depending on this abstraction callers can:
/// - Swap different persistence implementations (in-memory, EF Core, Dapper, etc.) without changing business code.
/// - Simplify unit testing by providing lightweight in-memory or mocked implementations.
/// - Centralize common data access behaviours (e.g., error handling, transactions) behind a single contract.
/// Implementations should preserve asynchronous semantics and honor cancellation via <see cref="System.Threading.CancellationToken"/>.
/// 
/// Inheriting XML Documentation with <c>&lt;inheritdoc/&gt;</c>:
/// <para>
/// Implementations of this interface can inherit the XML documentation from the interface members by using
/// the <c>&lt;inheritdoc/&gt;</c> tag on classes or members. This keeps implementation comments consistent with
/// the contract and reduces duplication. It also helps tooling (IntelliSense, documentation generators) show
/// the same guidance to callers of concrete implementations.
/// </para>
/// <para>
/// How to use:
/// - Place <c>&lt;inheritdoc/&gt;</c> on an implementing class or member to inherit comments from the interface.
/// - Optionally add a short summary or additional remarks on the implementation; the inherited text will be merged
///   by most documentation tools, with the explicit text supplementing the inherited content.
/// </para>
/// <para>
/// Why we have it:
/// - Reduces duplication of documentation across multiple implementations.
/// - Ensures behaviour described by the interface stays in sync with implementations.
/// - Improves discoverability for consumers of the concrete types.
/// </para>
/// <example>
/// Example showing how an implementation can inherit the documentation:
/// <code>
/// // interface (this file)
/// public interface IRepository<TEntity> where TEntity : IEntity { /* ... */ }
/// 
/// // implementation
/// /// <inheritdoc/>
/// public class EfRepository : IRepository<MyEntity>
/// {
///     /// <inheritdoc/>
///     public Task AddAsync(MyEntity entity, CancellationToken cancellationToken = default) =>
///         // implement add using EF Core
///         Task.CompletedTask;
/// }
/// </code>
/// </example>
/// </remarks>
// Inline: Implementations should prefer using <inheritdoc/> to inherit documentation from this contract.
public interface IRepository<TEntity>
    where TEntity : IEntity
{
    #region Create operations
    /// <summary>
    /// Adds a single <typeparamref name="TEntity"/> to the underlying store.
    /// </summary>
    /// <param name="entity">The entity to add. Implementations may set identity properties (e.g. Id) if needed.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    /// <remarks>
    /// Implementations should validate the entity where appropriate and throw well-defined exceptions
    /// for invalid states. This method does not return the entity; callers who need the updated instance
    /// (for example to read generated keys) should rely on their implementation's behaviour or use other APIs.
    /// </remarks>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of <typeparamref name="TEntity"/> instances to the underlying store.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous add-range operation.</returns>
    /// <remarks>
    /// This method is intended for batch inserts. Implementations can optimize for bulk operations
    /// (for example by using bulk insert APIs) to reduce round trips and improve throughput.
    /// </remarks>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    #endregion
    #region Read operations
    /// <summary>
    /// Retrieves a single <typeparamref name="TEntity"/> by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous get operation. The task result contains the entity if found;
    /// otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The exact meaning of the identifier is determined by concrete implementations. Callers should
    /// not assume any specific retrieval strategy (eager vs lazy loading) from this contract.
    /// </remarks>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all instances of <typeparamref name="TEntity"/> from the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous get-all operation. The task result is a collection of entities.</returns>
    /// <remarks>
    /// For large data sets callers should prefer paginated query methods (not part of this minimal contract)
    /// to avoid loading excessive amounts of data into memory. Implementations may return a snapshot of
    /// data or a live queryable result depending on technology used.
    /// </remarks>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    #endregion
    #region Update operations
    /// <summary>
    /// Updates an existing <typeparamref name="TEntity"/> in the underlying store.
    /// </summary>
    /// <param name="entity">The entity with updated values. Implementations should use the entity's identity to locate the existing record.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    /// <remarks>
    /// Concurrency control (optimistic or pessimistic) is implementation-specific. Callers should
    /// be prepared to handle concurrency-related exceptions if the implementation enforces version checks.
    /// </remarks>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    #endregion
    #region Delete operations
    /// <summary>
    /// Deletes the entity identified by <paramref name="id"/> from the underlying store.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <remarks>
    /// Implementations should decide whether to perform a hard delete or a soft delete and document that behaviour.
    /// </remarks>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    #endregion
}
