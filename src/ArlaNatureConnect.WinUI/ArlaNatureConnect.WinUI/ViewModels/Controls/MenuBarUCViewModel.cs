using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Dialogs;

using Microsoft.UI.Xaml;

using System.Windows.Input;

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
        try
        { // Prefer to close the DI-registered main window if available var main = App.HostInstance?.Services.GetService<MainWindow>(); if (main != null) { main.Close(); return; }
          // Fallback to Application.Current.Exit with null guard
            App? app = Microsoft.UI.Xaml.Application.Current as App;
            app?.Exit();

            // Last resort: force process exit (avoid unless necessary)
            // System.Environment.Exit(0);
        }
        catch
        {
            // swallow/log as appropriate; avoid throwing from ViewModel command
        }
    }

    private async void OnAbout()
    {
        // Show about dialog
        AboutDialog dlg = new();

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
