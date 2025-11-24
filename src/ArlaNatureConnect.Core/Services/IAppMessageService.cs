using System.ComponentModel;

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Provides a simple application-wide message store that exposes informational and error messages
/// for the UI and other components.
/// </summary>
/// <remarks>
/// This interface centralizes how components communicate status and error information across the
/// application. Implementations typically hold transient messages that can be bound to the UI and
/// are observable via <see cref="INotifyPropertyChanged"/> so views can react when messages
/// are added, removed or cleared. The service separates informational (status) messages from
/// error messages and optionally associates messages with a specific entity via <see cref="EntityName"/>.
///
/// Reasons for having this interface:
/// - Decouples message producers (services, view models) from message consumers (views).
/// - Enables easy unit testing by mocking the interface.
/// - Provides a single place to manage message lifecycle (add, clear) and to raise property
///   change notifications for UI binding.
/// </remarks>
public interface IAppMessageService : INotifyPropertyChanged
{
    /// <summary>
    /// An optional name of the current entity or context the messages relate to.
    /// </summary>
    /// <remarks>
    /// Consumers can set this to provide additional context for displayed messages (for example,
    /// the identifier of the record currently being edited). Implementations should raise
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> when the value changes.
    /// </remarks>
    string? EntityName { get; set; }

    /// <summary>
    /// Returns <c>true</c> when there is at least one informational (status) message available.
    /// </summary>
    bool HasStatusMessages { get; }

    /// <summary>
    /// Returns <c>true</c> when there is at least one error message available.
    /// </summary>
    bool HasErrorMessages { get; }

    /// <summary>
    /// A read-only collection of informational status messages.
    /// </summary>
    /// <remarks>
    /// The collection is intended to be consumed by the UI (for example, rendered in a status
    /// panel). Implementations may return an immutable snapshot or a live view. Consumers should
    /// not attempt to modify the returned collection directly.
    /// </remarks>
    IEnumerable<string> StatusMessages { get; }

    /// <summary>
    /// A read-only collection of error messages.
    /// </summary>
    /// <remarks>
    /// Error messages are typically displayed prominently to the user. Implementations may
    /// include additional information such as timestamps or error codes, but this interface
    /// only exposes the textual messages.
    /// </remarks>
    IEnumerable<string> ErrorMessages { get; }

    /// <summary>
    /// Adds an informational message to the application message service.
    /// </summary>
    /// <param name="message">The informational message to add. Must not be <c>null</c> or empty.</param>
    /// <remarks>
    /// Implementations should raise property change notifications for <see cref="StatusMessages"/>
    /// and <see cref="HasStatusMessages"/> when a message is added.
    /// </remarks>
    void AddInfoMessage(string message);

    /// <summary>
    /// Adds an error message to the application message service.
    /// </summary>
    /// <param name="message">The error message to add. Must not be <c>null</c> or empty.</param>
    /// <remarks>
    /// Implementations should raise property change notifications for <see cref="ErrorMessages"/>
    /// and <see cref="HasErrorMessages"/> when a message is added.
    /// </remarks>
    void AddErrorMessage(string message);

    /// <summary>
    /// Clears all stored error messages.
    /// </summary>
    /// <remarks>
    /// Implementations should raise property change notifications for <see cref="ErrorMessages"/>
    /// and <see cref="HasErrorMessages"/> after clearing.
    /// </remarks>
    void ClearErrorMessages();
}
