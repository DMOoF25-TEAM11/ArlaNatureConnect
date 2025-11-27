using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// Implements <see cref="INotifyPropertyChanged"/> to support property-change notifications
/// required by data-binding frameworks (e.g. WinUI, WPF, UWP).
/// </summary>
/// <remarks>
/// Derive concrete ViewModel classes from this abstract class. Use <see cref="OnPropertyChanged(string?)"/>
/// within property setters to notify the UI of updates. Do not attempt to instantiate this abstract class directly.
/// </remarks>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region Events
    /// <summary>
    /// Occurs when a property value changes.
    /// Consumers (typically UI bindings) subscribe to this event to be notified that a property value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Property Changed

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the property specified by <paramref name="name"/>.
    /// </summary>
    /// <param name="name">
    /// The name of the property that changed. This parameter is optional; when omitted, the
    /// compiler will supply the caller member name due to the <see cref="CallerMemberNameAttribute"/>.
    /// </param>
    /// <remarks>
    /// Use this method in property setters to notify listeners of changes, for example:
    /// <code>
    /// private string? _name;
    /// public string? Name
    /// {
    ///     get => _name;
    ///     set
    ///     {
    ///         if (value == _name) return;
    ///         _name = value;
    ///         OnPropertyChanged(); // CallerMemberName provides "Name"
    ///     }
    /// }
    /// </code>
    /// The method invokes <see cref="PropertyChanged"/> safely using the null-conditional operator.
    /// </remarks>
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    #endregion
}
