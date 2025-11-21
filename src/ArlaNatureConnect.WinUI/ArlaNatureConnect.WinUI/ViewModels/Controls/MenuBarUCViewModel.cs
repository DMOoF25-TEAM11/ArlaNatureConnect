using System.Windows.Input;

using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class MenuBarUCViewModel : ViewModelBase
{
    #region Fields
    #endregion
    #region Fields Commands
    private readonly RelayCommand _exitCommand;
    private readonly RelayCommand _aboutCommand;
    #endregion
    #region Event handlers
    #endregion

    public MenuBarUCViewModel() : base()
    {
        _exitCommand = new RelayCommand(OnExit);
        _aboutCommand = new RelayCommand(OnAbout);
    }

    #region Properties
    #endregion
    #region Observables Properties

    #endregion
    #region Load handler
    #endregion
    #region Commands
    public ICommand OpenAboutDialogCommand => _aboutCommand;
    public ICommand ExitCommand => _exitCommand;
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    private void OnExit()
    {
        App.Current.Exit();
    }

    private async void OnAbout()
    {
        // Show about dialog
        var dlg = new Views.Dialogs.AboutDialog();

        // Safely get a XamlRoot for the dialog. Use helper on App which resolves the main window's content XamlRoot.
        XamlRoot? xr = App.MainWindowXamlRoot;
        if (xr != null)
        {
            dlg.XamlRoot = xr;
            await dlg.ShowAsync();
        }
        else
        {
            // Fallback: try Window.Current.Content (works in many scenarios) or simply show without XamlRoot
            try
            {
                dlg.XamlRoot = Microsoft.UI.Xaml.Window.Current?.Content?.XamlRoot;
                await dlg.ShowAsync();
            }
            catch
            {
                // Last resort: attempt to show without setting XamlRoot (may fail on some hosts)
                await dlg.ShowAsync();
            }
        }
    }
    #endregion
    #region Helpers
    #endregion

}
