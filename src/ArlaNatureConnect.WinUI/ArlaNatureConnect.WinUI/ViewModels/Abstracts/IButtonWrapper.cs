namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Represents a minimal wrapper for a button-like element used by view models.
/// </summary>
/// <remarks>
/// This abstraction allows view models to interact with button controls or
/// button-like view elements without referencing UI framework types directly.
/// It is useful for decoupling, unit testing, and providing a uniform contract
/// for passing command parameters or arbitrary metadata (Tag) from views to
/// view models.
/// </remarks>
public interface IButtonWrapper
{
    /// <summary>
    /// Gets or sets the parameter that will be passed to a command when the
    /// associated button is invoked.
    /// </summary>
    /// <remarks>
    /// Use this property to forward contextual information (for example an
    /// identifier or a data object) from the view to the command handler in
    /// the view model. The type is <see cref="object"/> to keep the contract
    /// flexible; callers should cast to the expected type.
    /// </remarks>
    object? CommandParameter { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary metadata object associated with the button.
    /// </summary>
    /// <remarks>
    /// The <see cref="Tag"/> property can be used to store additional state or
    /// identifiers related to the button that are not necessarily passed to
    /// commands. This mirrors common UI paradigms where controls expose a
    /// <c>Tag</c> for lightweight metadata.
    /// </remarks>
    object? Tag { get; set; }
}
