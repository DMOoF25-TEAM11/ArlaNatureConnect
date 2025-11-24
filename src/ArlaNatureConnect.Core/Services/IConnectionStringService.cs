namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Abstraction responsible for managing the application's database connection string.
/// </summary>
/// <remarks>
/// This interface centralizes responsibilities related to the connection string: detecting
/// whether a stored connection string exists, validating that a connection string can be
/// used to connect to the database, persisting a connection string and reading it back.
///
/// The interface exists to decouple persistence and validation logic from callers (UI,
/// services and tests). Implementations can store the connection string in different
/// places (configuration file, secure store, platform-specific credential stores) and
/// can be mocked during unit tests. It also provides a single place to add additional
/// validation, logging or security measures related to connection strings.
/// </remarks>
public interface IConnectionStringService
{
    /// <summary>
    /// Determines whether a persisted connection string exists.
    /// </summary>
    /// <returns>
    /// A task that resolves to <c>true</c> if a connection string has been saved and is
    /// available to read; otherwise <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync();

    /// <summary>
    /// Validates that the provided connection string can be used to establish a
    /// connection to the database.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> when a connection can be established using the
    /// supplied connection string; otherwise <c>false</c>. Implementations should avoid
    /// throwing on transient failures and instead return <c>false</c> where appropriate.
    /// </returns>
    Task<bool> CanConnectionStringConnect(string connectionString);

    /// <summary>
    /// Persists the provided connection string using the implementation's chosen
    /// storage mechanism.
    /// </summary>
    /// <param name="connectionString">The connection string to persist.</param>
    /// <returns>A task that completes when the save operation has finished.</returns>
    /// <remarks>
    /// Implementations are responsible for choosing an appropriate storage location and
    /// any required protections (encryption, secure platform stores). Callers should not
    /// assume the storage is readable by other processes.
    /// </remarks>
    Task SaveAsync(string connectionString);

    /// <summary>
    /// Reads the persisted connection string, if one exists.
    /// </summary>
    /// <returns>
    /// A task that resolves to the stored connection string, or <c>null</c> if none is
    /// available.
    /// </returns>
    Task<string?> ReadAsync();
}
