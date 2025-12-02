using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Centralizes app-wide status reporting so multiple components can signal loading and
/// database-connection state in a consistent way. This prevents UI flicker and race conditions
/// by keeping <c>IsLoadingOrSaving</c> true until every independent caller finishes (reference-counted
/// tokens), so the UI and other consumers observe reliable state.
///
/// How to use:
/// - Call <see cref="BeginLoadingOrSaving"/> when a component starts an operation that should
///   mark the app as loading. Dispose the returned token when the operation completes.
/// - Read <see cref="IsLoadingOrSaving"/> to know whether any active loading callers remain.
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
    // Use System.Threading.Lock for synchronization
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
    /// True while one or more callers have an active loading token from <see cref="BeginLoadingOrSaving"/>.
    /// Setting this property directly will replace the internal reference-count with 0/1.
    ///
    /// How to use:
    /// - Prefer <see cref="BeginLoadingOrSaving"/> to participate in reference-counted loading.
    /// - Read this property to drive UI indicators (e.g. spinners).
    ///
    /// Why we have it:
    /// - Ensures UI shows loading only while there are active operations, preventing flicker
    ///   caused by short-lived operations starting and stopping rapidly.
    /// </summary>
    public bool IsLoadingOrSaving
    {
        get
        {
            lock (_syncLock)
            {
                return _loadingCount > 0;
            }
        }
        //set
        //{
        //    bool notify = false;
        //    lock (_syncLock)
        //    {
        //        bool currently = _loadingCount > 0;
        //        if (currently == value) return;
        //        _loadingCount = value ? 1 : 0;
        //        notify = true;
        //    }
        //    if (notify) NotifyStatusChanged(nameof(IsLoadingOrSaving));
        //}
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
        OnPropertyChanged(propertyName);
        EventHandler? statusHandler = StatusInfoChanged;

        try { statusHandler?.Invoke(this, EventArgs.Empty); } catch { }
    }


    /// <summary>
    /// Acquire a loading token. When the returned <see cref="IDisposable"/> is disposed the
    /// internal loading count is decremented. Use this when multiple independent callers need
    /// to indicate "loading" and you want <see cref="IsLoadingOrSaving"/> to be true until every caller
    /// has finished (disposed their token).
    ///
    /// Typical usage:
    /// <code>
    /// using (statusService.BeginLoadingOrSaving())
    /// {
    ///     await SomeWorkAsync();
    /// }
    /// </code>
    /// </summary>
    public IDisposable BeginLoadingOrSaving()
    {
        int newVal;
        lock (_syncLock)
        {
            _loadingCount++;
            newVal = _loadingCount;
        }
        if (newVal == 1) NotifyStatusChanged(nameof(IsLoadingOrSaving));

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
            if (raise) NotifyStatusChanged(nameof(IsLoadingOrSaving));
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
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Provides a disposable object that executes a specified action when disposed.
    /// </summary>
    /// <remarks>
    /// Use this class to ensure that a particular action is performed when the object is disposed,
    /// such as releasing resources or performing cleanup logic. The action is guaranteed to be invoked
    /// at most once, even if <see cref="Dispose"/> is called multiple times. This class is thread-safe.
    /// </remarks>
    private sealed partial class ActionOnDispose(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose
            ?? throw new ArgumentNullException(nameof(onDispose));
        private int _disposed;

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
