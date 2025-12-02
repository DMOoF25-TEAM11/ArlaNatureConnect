namespace ArlaNatureConnect.Domain.Abstract;

/// <summary>
/// Represents a domain entity that exposes a unique identifier.
/// </summary>
/// <remarks>
/// Use the <c>&lt;inheritdoc/&gt;</c> tag on implementing members to inherit this documentation.
/// This reduces duplication and keeps documentation consistent between interfaces/base types
/// and their implementations.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// // Interface declares the contract and documentation
/// public interface IEntity
/// {
///     Guid Id { get; }
/// }
///
/// // Implementation can inherit the documentation for the Id property from IEntity
/// public class Person : IEntity
/// {
///     /// <inheritdoc/> // Inherits the summary and remarks from IEntity.Id
///     public Guid Id { get; init; }
/// }
/// ]]></code>
/// </example>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    /// <remarks>
    /// Implementations can use <c>&lt;inheritdoc/&gt;</c> to inherit this summary instead of
    /// copying it. This keeps the interface as the single source of truth for the contract
    /// and its documentation.
    /// </remarks>
    Guid Id { get; } // Unique identifier for the entity
}
