using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Centralizes app-wide status reporting so multiple components can signal loading and
/// database-connection state in a consistent way. This prevents UI flicker and race conditions
/// by keeping <c>IsLoading</c> true until every independent caller finishes (reference-counted
/// tokens), so the UI and other consumers observe reliable state.
///
/// How to use:
/// - Call <see cref="BeginLoading"/> when a component starts an operation that should
///   mark the app as loading. Dispose the returned token when the operation completes.
/// - Read <see cref="IsLoading"/> to know whether any active loading callers remain.
/// - Set or read <see cref="HasDbConnection"/> to indicate whether a database connection
///   is available.
///
/// Why we have it:
/// - To provide a single source of truth for global loading/db connection state so multiple
///   independent components do not fight over UI indicators and can avoid flicker.
/// - To provide change notifications via <see cref="PropertyChanged"/> and
///   <see cref="StatusInfoChanged"/> so UIs and other services can react to status changes.
///
/// Implements <see cref="IStatusInfoServices"/>.
/// </summary>
public partial class StatusInfoService : IStatusInfoServices
{
    #region Fields
    // Reference-count for active loading callers. Use lock-based synchronization to be thread-safe.
    private int _loadingCount;
    // 0 = false, 1 = true for DB connectivity state.
    private int _hasDbConnection;
    // disposed guard (0 = false, 1 = true)
    private int _disposed;
    // sync object for lock-based atomicity
    private readonly object _syncLock = new();
    #endregion

    #region Event handlers
    public event EventHandler? StatusInfoChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    public StatusInfoService()
    {
    }


    #region Properties

    /// <summary>
    /// True while one or more callers have an active loading token from <see cref="BeginLoading"/>.
    /// Setting this property directly will replace the internal reference-count with 0/1.
    ///
    /// How to use:
    /// - Prefer <see cref="BeginLoading"/> to participate in reference-counted loading.
    /// - Read this property to drive UI indicators (e.g. spinners).
    ///
    /// Why we have it:
    /// - Ensures UI shows loading only while there are active operations, preventing flicker
    ///   caused by short-lived operations starting and stopping rapidly.
    /// </summary>
    public bool IsLoading
    {
        get
        {
            // Lock to ensure a consistent, thread-safe read of the reference-count.
            lock (_syncLock)
            {
                return _loadingCount > 0;
            }
        }
        set
        {
            bool notify = false;
            lock (_syncLock)
            {
                bool currently = _loadingCount > 0;
                if (currently == value) return;
                _loadingCount = value ? 1 : 0;
                notify = true;
            }
            if (notify) NotifyStatusChanged(nameof(IsLoading));
        }
    }

    /// <summary>
    /// Indicates whether a database connection is available. Use lock-based synchronization for thread-safety.
    ///
    /// How to use:
    /// - Set this to true when the app establishes a DB connection, false when lost.
    /// - Observe this property to disable features that require DB access.
    /// </summary>
    public bool HasDbConnection
    {
        get
        {
            // Lock to ensure a consistent read of the connectivity flag.
            lock (_syncLock)
            {
                return _hasDbConnection != 0;
            }
        }
        set
        {
            bool notify = false;
            lock (_syncLock)
            {
                int newVal = value ? 1 : 0;
                if (_hasDbConnection == newVal) return;
                _hasDbConnection = newVal;
                notify = true;
            }
            if (notify) NotifyStatusChanged(nameof(HasDbConnection));
        }
    }

    #endregion

    #region Helpers
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChangedEventHandler? handler = PropertyChanged;
        try { handler?.Invoke(this, new PropertyChangedEventArgs(name)); } catch { }
    }

    private void NotifyStatusChanged([CallerMemberName] string? propertyName = null)
    {
        // Invoke handlers directly. UI subscribers must marshal to UI thread.
        PropertyChangedEventHandler? propHandler = PropertyChanged;
        EventHandler? statusHandler = StatusInfoChanged;

        try { propHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName)); } catch { }
        try { statusHandler?.Invoke(this, EventArgs.Empty); } catch { }
    }


    /// <summary>
    /// Acquire a loading token. When the returned <see cref="IDisposable"/> is disposed the
    /// internal loading count is decremented. Use this when multiple independent callers need
    /// to indicate "loading" and you want <see cref="IsLoading"/> to be true until every caller
    /// has finished (disposed their token).
    ///
    /// Typical usage:
    /// <code>
    /// using (statusService.BeginLoading())
    /// {
    ///     await SomeWorkAsync();
    /// }
    /// </code>
    /// </summary>
    public IDisposable BeginLoading()
    {
        int newVal;
        lock (_syncLock)
        {
            _loadingCount++;
            newVal = _loadingCount;
        }
        if (newVal == 1) NotifyStatusChanged(nameof(IsLoading));

        // Return a disposable token that decrements the counter when disposed.
        return new ActionOnDispose(() =>
        {
            bool raise = false;
            lock (_syncLock)
            {
                if (_loadingCount > 0)
                {
                    _loadingCount--;
                    if (_loadingCount == 0) raise = true;
                }
            }
            if (raise) NotifyStatusChanged(nameof(IsLoading));
        });
    }


    /// <summary>
    /// Disposes the StatusInfoService, releasing any resources. This is idempotent.
    /// </summary>
    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed == 1) return;
            _disposed = 1;
        }
    }

    /// <summary>
    /// Provides a disposable object that executes a specified action when disposed.
    /// </summary>
    /// <remarks>
    /// Use this class to ensure that a particular action is performed when the object is disposed,
    /// such as releasing resources or performing cleanup logic. The action is guaranteed to be invoked
    /// at most once, even if <see cref="Dispose"/> is called multiple times. This class is thread-safe.
    /// </remarks>
    private sealed partial class ActionOnDispose : IDisposable
    {
        private readonly Action _onDispose;
        private int _disposed;

        public ActionOnDispose(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        // Inline comment: Ensure the action only runs once even if Dispose is called repeatedly.
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try { _onDispose(); } catch { }
            }
        }
    }

    #endregion
}
