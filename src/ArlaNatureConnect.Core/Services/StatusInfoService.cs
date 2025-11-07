namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Centralizes app-wide status reporting so multiple components can signal loading and database-connection state
/// in a consistent way. This prevents UI flicker and race conditions by keeping `IsLoading` true until every
/// independent caller finishes (reference-counted tokens), so the UI and other consumers observe reliable state.
/// Implements <see cref="IStatusInfoServices"/>.
/// </summary>
/// </summary>
public class StatusInfoService : IStatusInfoServices
{
    #region Fields
    private int _loadingCount;
    private readonly object _loadingLock = new();
    private bool _hasDbConnection;
    #endregion

    #region Event handlers
    public event EventHandler? StatusInfoChanged;
    #endregion

    public StatusInfoService()
    {

    }

    #region Properties

    public bool IsLoading
    {
        get => _loadingCount > 0;
        // Setter kept to match the interface; setting true makes count =1, false clears all counts
        set
        {
            bool raise;
            lock (_loadingLock)
            {
                var wasLoading = _loadingCount > 0;
                _loadingCount = value ? 1 : 0;
                raise = wasLoading != (_loadingCount > 0);
            }

            if (raise) OnStatusInfoChanged();
        }
    }

    /// <summary>
    /// Acquire a loading token. When the returned IDisposable is disposed the loading count is decremented.
    /// Use this when multiple independent callers need to indicate "loading" and you want IsLoading to be true
    /// until every caller has finished (disposed their token).
    /// </summary>
    public IDisposable BeginLoading()
    {
        lock (_loadingLock)
        {
            var wasLoading = _loadingCount > 0;
            _loadingCount++;
            if (!wasLoading && _loadingCount > 0)
            {
                OnStatusInfoChanged();
            }
        }

        return new ActionOnDispose(() =>
        {
            bool raise = false;
            lock (_loadingLock)
            {
                if (_loadingCount > 0) _loadingCount--;
                if (_loadingCount == 0) raise = true;
            }

            if (raise) OnStatusInfoChanged();
        });
    }

    public bool HasDbConnection
    {
        get => _hasDbConnection;
        set
        {
            if (_hasDbConnection == value) return;
            _hasDbConnection = value;
            OnStatusInfoChanged();
        }
    }
    #endregion

    #region Helpers
    private void OnStatusInfoChanged()
    {
        try
        {
            StatusInfoChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // swallow subscriber exceptions
        }
    }
    #endregion

    // Small private helper that runs an Action when disposed.
    private sealed class ActionOnDispose : IDisposable
    {
        private readonly Action _onDispose;
        private int _disposed;

        public ActionOnDispose(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try
                {
                    _onDispose();
                }
                catch
                {
                    // swallow
                }
            }
        }
    }
}
