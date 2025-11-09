using System.ComponentModel;
using System.Runtime.CompilerServices;
using ArlaNatureConnect.WinUI.Commands;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// Includes PropertyChanged notification and navigation support.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region Fields

    private string _currentNavigationTag = string.Empty;

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Navigation Support

    /// <summary>
    /// Command to navigate between different content views.
    /// Receives navigation tag as string parameter (e.g., "Dashboards", "Farms", "Tasks").
    /// Initialized via InitializeNavigation() method.
    /// </summary>
    public RelayCommand<string>? NavigationCommand { get; protected set; }

    /// <summary>
    /// The currently selected navigation tag.
    /// Used to determine which content view to display and which navigation button is active.
    /// </summary>
    public string CurrentNavigationTag
    {
        get => _currentNavigationTag;
        protected set
        {
            if (_currentNavigationTag != value)
            {
                _currentNavigationTag = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Initializes the navigation command. Should be called in the constructor of derived classes.
    /// </summary>
    /// <param name="defaultTag">The default navigation tag to use when initializing.</param>
    protected void InitializeNavigation(string defaultTag = "")
    {
        _currentNavigationTag = defaultTag;
        NavigationCommand = new RelayCommand<string>(NavigateToView, tag => !string.IsNullOrWhiteSpace(tag));
    }

    /// <summary>
    /// Navigates to the specified content view based on the navigation tag.
    /// </summary>
    /// <param name="tag">The navigation tag.</param>
    protected virtual void NavigateToView(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        // Update current navigation tag - this will notify the view to switch content
        CurrentNavigationTag = tag;
    }

    #endregion

    #region Property Changed

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}
