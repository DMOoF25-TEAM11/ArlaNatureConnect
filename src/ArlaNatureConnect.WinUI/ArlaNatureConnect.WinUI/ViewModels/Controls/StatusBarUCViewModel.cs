using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices? _statusInfoServices;
    private bool _hasDbConnection;

    private readonly Lock _statusInfoLock = new();
    #endregion

    #region Fields Commands
    #endregion

    #region Event handlers
    #endregion

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices;

        // subscribe to status changes so the viewmodel can notify the view
        _statusInfoServices.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;

        // initialize observable properties from the service
        _hasDbConnection = _statusInfoServices.HasDbConnection;
    }

    #region Properties

    // Expose current busy state (observable)
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            // notify dependent computed properties
            OnPropertyChanged(nameof(BusySymbol));
            OnPropertyChanged(nameof(BusyLabel));
        }
    }

    // Expose database connection state (observable)
    public bool HasDbConnection
    {
        get => _hasDbConnection;
        private set
        {
            if (_hasDbConnection == value) return;
            _hasDbConnection = value;
            OnPropertyChanged();
            // notify dependent computed property
            OnPropertyChanged(nameof(DbConnectionSymbol));
        }
    }

    // Simple symbol properties (can be bound to a TextBlock/Glyph) - use emoji to avoid relying on font glyphs
    public string BusySymbol
    {
        get
        {
            return IsBusy ? "⏳" : "✔️";
        }
    }

    // New computed label property for UI text
    public string BusyLabel => IsBusy ? "Busy:" : "Idle:";

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

    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        lock (_statusInfoLock)
        {
            if (_statusInfoServices == null) throw new InvalidOperationException("_statusInfoServices is not initialized.");
            bool isLoading = _statusInfoServices.IsLoading;
            bool hasDb = _statusInfoServices.HasDbConnection;

            IsBusy = isLoading;
            HasDbConnection = hasDb;

            // Always raise BusySymbol/Bu syLabel property changed when IsBusy changes
            OnPropertyChanged(nameof(BusySymbol));
            OnPropertyChanged(nameof(BusyLabel));
            // Always raise DbConnectionSymbol property changed when HasDbConnection changes
            OnPropertyChanged(nameof(DbConnectionSymbol));
        }
    }
    #endregion
}
