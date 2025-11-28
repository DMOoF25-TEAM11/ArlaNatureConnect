using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices? _statusInfoServices;
    private volatile bool _hasDbConnection;
    private volatile bool _isBusy;

    private readonly object _statusInfoLock = new();
    private bool _isInitializedForUi;
    #endregion

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices ?? throw new ArgumentNullException(nameof(statusInfoServices));

        // initialize observable properties from the service (read-only, safe to read)
        // Use the safe wrapper which catches COMException instead of accessing the service property directly
        _hasDbConnection = _statusInfo_services_HasDb();
    }

    #region Observables Properties
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
        // Prefer reading directly from the service when available so tests using Moq setups that return a closure value
        // will reflect changes immediately. Fall back to cached field if service is null.
        get => _statusInfoServices != null ? _statusInfo_services_HasDb() : _hasDbConnection;
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
    public string BusySymbol => IsBusy ? "⏳" : "✔️";

    // New computed label property for UI text
    public string BusyLabel => IsBusy ? "Busy:" : "Idle:";

    // Use a green check emoji that matches unit tests
    public string DbConnectionSymbol => HasDbConnection ? "✅" : "❌";
    #endregion


    #region Helpers
    /// <summary>
    /// Initialize the viewmodel for UI usage. Tests call this to subscribe to the status service event.
    /// </summary>
    public void InitializeForUi(object? _ = null)
    {
        if (_isInitializedForUi) return;

        _statusInfoServices!.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;

        // perform an initial read/update
        UpdateFromService();

        _isInitializedForUi = true;
    }

    private void UpdateFromService()
    {
        lock (_statusInfoLock)
        {
            if (_statusInfoServices == null) throw new InvalidOperationException("_statusInfo_services is not initialized.");
            bool isLoading = _statusInfo_services_IsLoading();
            bool hasDb = _statusInfo_services_HasDb();

            // Debug logging to help investigate unit test failures
            Debug.WriteLine($"[StatusBarUCViewModel] UpdateFromService: service.IsLoading={isLoading}, service.HasDbConnection={hasDb}, field_hasDb={_hasDbConnection}");

            // Always assign internal fields and always raise notifications so consumers receive updates
            _isBusy = isLoading;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(BusySymbol));
            OnPropertyChanged(nameof(BusyLabel));

            _hasDbConnection = hasDb;
            OnPropertyChanged(nameof(HasDbConnection));
            OnPropertyChanged(nameof(DbConnectionSymbol));

            Debug.WriteLine($"[StatusBarUCViewModel] After Update: _isBusy={_isBusy}, _hasDb_connection={_hasDbConnection}, IsBusy={IsBusy}, HasDbConnection={HasDbConnection}");
        }
    }

    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        try
        {
            UpdateFromService();
        }
        catch (COMException)
        {
            // swallow COM exceptions which may occur when WinRT objects are accessed from the wrong thread
        }
    }

    // Helper wrappers to call service properties and allow catching exceptions in a controlled place
    private bool _statusInfo_services_IsLoading()
    {
        try { return _statusInfoServices!.IsLoading; } catch (COMException) { return false; }
    }
    private bool _statusInfo_services_HasDb()
    {
        try { return _statusInfoServices!.HasDbConnection; } catch (COMException) { return false; }
    }

    #endregion
}
