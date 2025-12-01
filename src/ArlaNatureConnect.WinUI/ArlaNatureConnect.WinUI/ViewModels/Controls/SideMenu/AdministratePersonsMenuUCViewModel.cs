using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

public sealed partial class AdministratePersonsMenuUCViewModel : ViewModelBase
{
    #region Fields
    #endregion
    #region Fields Commands
    private RelayCommand<object> _navigationCommand;
    #endregion
    #region Event handlers
    #endregion

    public AdministratePersonsMenuUCViewModel()
    {
        _navigationCommand = new RelayCommand<object>(OnNavigation);
    }

    #region Properties
    #endregion
    #region Observables Properties
    #endregion
    #region Load handler
    #endregion
    #region Commands
    public ICommand NavigationCommand => _navigationCommand ??= new RelayCommand<object>(OnNavigation);
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    private void OnNavigation(object? commandParameter)
    {
    }
    #endregion
    #region Helpers
    #endregion
}