using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// Includes PropertyChanged notification and navigation support.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region Fields
    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Property Changed

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}
