using System.ComponentModel;

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Provides application-wide status information and control for UI components and services.
/// </summary>
/// <remarks>
/// This interface abstracts global status such as whether the application is currently performing
/// a long-running operation and whether a database connection is available. Consumers (views,
/// view-models, and services) use this interface to observe changes via <see cref="StatusInfoChanged"/>
/// and to enter a loading scope via <see cref="BeginLoadingOrSaving"/>. The scoped loader returned by
/// <see cref="BeginLoadingOrSaving"/> ensures <see cref="IsLoadingOrSaving"/> is set while an operation runs and
/// automatically resets when the scope is disposed. Abstracting this behavior enables easier
/// testing and decouples UI concerns from concrete implementations.
/// </remarks>
public interface IStatusInfoServices : IDisposable, INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets a value indicating whether a long-running operation is in progress.
    /// </summary>
    /// <remarks>
    /// Implementations should raise <see cref="StatusInfoChanged"/> when this value changes.
    /// </remarks>
    bool IsLoadingOrSaving { get; }

    /// <summary>
    /// Gets or sets a value indicating whether a database connection is currently available.
    /// </summary>
    /// <remarks>
    /// This flag can be used by UI components to show offline states or disable features that
    /// require database access. Implementations should raise <see cref="StatusInfoChanged"/>
    /// when this value changes.
    /// </remarks>
    bool HasDbConnection { get; set; }

    /// <summary>
    /// Occurs when any status information exposed by this service has changed.
    /// </summary>
    /// <remarks>
    /// Subscribers should update UI or internal state in response to this event.
    /// </remarks>
    event EventHandler? StatusInfoChanged;

    /// <summary>
    /// Begins a loading scope and returns an <see cref="IDisposable"/> that ends the scope when disposed.
    /// </summary>
    /// <returns>
    /// An <see cref="IDisposable"/> which, when disposed, will end the loading scope (typically by setting <see cref="IsLoadingOrSaving"/> to false).
    /// </returns>
    /// <remarks>
    /// Use this method to ensure the <see cref="IsLoadingOrSaving"/> flag is set for the duration of an operation.
    /// Typical usage:
    /// <code>
    /// using (status.BeginLoadingOrSaving()) { await DoWorkAsync(); }
    /// </code>
    /// </remarks>
    IDisposable BeginLoadingOrSaving();
}
