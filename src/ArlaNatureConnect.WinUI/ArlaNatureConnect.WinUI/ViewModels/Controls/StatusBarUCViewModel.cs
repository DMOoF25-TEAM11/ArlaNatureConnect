using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Dispatching;

using System.Runtime.InteropServices;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices? _statusInfoServices;
    private volatile bool _hasDbConnection;
    private volatile bool _isBusy;

    private readonly object _statusInfoLock = new();
    private DispatcherQueue? _dispatcher; // will be set from UI thread
    private SynchronizationContext? _syncContext;
    private bool _isInitializedForUi;
    #endregion

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices ?? throw new ArgumentNullException(nameof(statusInfoServices));

        // do NOT subscribe or capture UI dispatcher here because constructor may run on a non-UI thread.
        _dispatcher = null;
        _syncContext = null;

        // initialize observable properties from the service (read-only, safe to read)
        _hasDbConnection = _statusInfoServices.HasDbConnection;
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
    public string BusySymbol => IsBusy ? "⏳" : "✔️";

    // New computed label property for UI text
    public string BusyLabel => IsBusy ? "Busy:" : "Idle:";

    // Use a green check emoji that matches unit tests
    public string DbConnectionSymbol => HasDbConnection ? "✅" : "❌";
    #endregion


    #region Helpers
    /// <summary>
    /// Initialize the viewmodel for UI usage. Must be called from the UI thread (for example from the control's constructor/Loaded handler).
    /// This sets the dispatcher, captures the synchronization context and subscribes to service events.
    /// </summary>
    public void InitializeForUi(DispatcherQueue? dispatcher)
    {
        if (_isInitializedForUi) return;

        _dispatcher = dispatcher;
        _syncContext = SynchronizationContext.Current;

        // subscribe to status changes so the viewmodel can notify the view
        _statusInfoServices!.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;

        // perform an initial read/update on the UI thread
        if (_dispatcher != null && _dispatcher.HasThreadAccess)
        {
            UpdateFromService();
        }
        else if (_syncContext != null)
        {
            _syncContext.Post(_ => UpdateFromService(), null);
        }
        else
        {
            // try to update in-place; swallowing potential COM exceptions
            try { UpdateFromService(); } catch (COMException) { }
        }

        _isInitializedForUi = true;
    }

    private void UpdateFromService()
    {
        lock (_statusInfoLock)
        {
            if (_statusInfoServices == null) throw new InvalidOperationException("_statusInfoServices is not initialized.");
            bool isLoading = _statusInfoServices.IsLoading;
            bool hasDb = _statusInfoServices.HasDbConnection;

            // Use property setters which raise change notifications
            IsBusy = isLoading;
            HasDbConnection = hasDb;
        }
    }

    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        // Try to dispatch to UI thread if possible
        try
        {
            // prefer captured dispatcher
            DispatcherQueue? dq = _dispatcher;
            if (dq != null)
            {
                if (dq.HasThreadAccess)
                {
                    UpdateFromService();
                    return;
                }

                if (dq.TryEnqueue(UpdateFromService)) return;
            }

            // fallback to synchronization context if available
            if (_syncContext != null)
            {
                _syncContext.Post(_ => UpdateFromService(), null);
                return;
            }

            // last resort: try to run on current thread but catch COM exceptions to avoid crashing on wrong-thread UI calls
            try
            {
                UpdateFromService();
            }
            catch (COMException)
            {
                // swallow WinRT wrong-thread COM exceptions and skip this update
            }
        }
        catch (COMException)
        {
            // swallow any COM exceptions thrown while attempting to marshal
        }
    }
    #endregion
}
