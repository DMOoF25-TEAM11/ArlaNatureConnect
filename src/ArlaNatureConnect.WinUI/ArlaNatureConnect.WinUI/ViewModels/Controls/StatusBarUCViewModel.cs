using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices? _statusInfoServices;
    private bool _hasDbConnection;
    private bool _isBusy;
    #endregion

    #region Fields Commands
    #endregion

    #region Event handlers
    #endregion

    public StatusBarUCViewModel()
    {
        // Parameterless ctor left for design-time only; avoid accessing service members here.
    }

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices ?? throw new ArgumentNullException(nameof(statusInfoServices));

        // subscribe to status changes so the viewmodel can notify the view
        _statusInfoServices.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;

        // initialize observable properties from the service
        _hasDbConnection = _statusInfoServices.HasDbConnection;
        _isBusy = _statusInfoServices.IsLoading;

        // Start async initialization without blocking the constructor.
        // The actual loading scope is inside InitializeAsync so BeginLoading will cover the async work.
        _ = InitializeAsync();
    }

    #region Properties

    // Expose current busy state (observable)
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
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
        if (_statusInfoServices is null)
            throw new InvalidOperationException("_statusInfoServices is not initialized.");

        using (_statusInfoServices.BeginLoading())
        {
            // perform async initialization here
            await Task.Delay(200).ConfigureAwait(false);

            // ensure UI is updated with current status values after initialization
            StatusInfoServices_StatusInfoChanged(this, EventArgs.Empty);
        }
    }

    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        // update cached observable properties from the service
        IsBusy = _statusInfoServices?.IsLoading ?? false;
        HasDbConnection = _statusInfoServices?.HasDbConnection ?? false;

        // raise property changed for dependent properties
        OnPropertyChanged(nameof(BusySymbol));
        OnPropertyChanged(nameof(DbConnectionSymbol));
    }
    #endregion
}
