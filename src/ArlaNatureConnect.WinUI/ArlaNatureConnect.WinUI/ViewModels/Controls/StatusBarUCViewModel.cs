using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices _statusInfoServices;
    #endregion

    #region Fields Commands
    #endregion

    #region Event handlers
    #endregion

    public StatusBarUCViewModel()
    {
        
    }

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices;
        using (_statusInfoServices.BeginLoading())
        {
            // perform any async initialization here
            InitializeAsync().ConfigureAwait(false);
            // subscribe to status changes so the viewmodel can notify the view
            _statusInfoServices.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;
        }
    }

    #region Properties

    // Expose current busy state (maps to service.IsLoading)
    public bool IsBusy
    {
        get
        {
            return _statusInfoServices.IsLoading;
        }
    }

    // Expose database connection state
    public bool HasDbConnection
    {
        get
        {
            return _statusInfoServices.HasDbConnection;
        }
    }

    // Simple symbol properties (can be bound to a TextBlock/Glyph) - use emoji to avoid relying on font glyphs
    public string BusySymbol
    {
        get
        {
            return IsBusy ? "⏳" : string.Empty;
        }
    }

    public string DbConnectionSymbol
    {
        get
        {
            return HasDbConnection ? "✅" : "❌";
        }
    }

    #endregion

    #region Observables Properties
    #endregion

    #region Load handler
    #endregion

    #region Commands
    #endregion

    #region CanXXX Command
    #endregion

    #region OnXXX Command
    #endregion

    #region Helpers
    public async Task InitializeAsync()
    {
        using (_statusInfoServices.BeginLoading())
        {
            // perform async initialization here
            await Task.Delay(200); // example
        }
    }

    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        // raise property changed for dependent properties
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(HasDbConnection));
        OnPropertyChanged(nameof(BusySymbol));
        OnPropertyChanged(nameof(DbConnectionSymbol));
    }
    #endregion
}
