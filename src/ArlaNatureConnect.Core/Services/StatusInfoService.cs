using System.Threading; // Add this at the top

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Centralizes app-wide status reporting so multiple components can signal loading and database-connection state
/// in a consistent way. This prevents UI flicker and race conditions by keeping `IsLoading` true until every
/// independent caller finishes (reference-counted tokens), so the UI and other consumers observe reliable state.
/// Implements <see cref="IStatusInfoServices"/>.
/// </summary>
/// </summary>
public partial class StatusInfoService : IStatusInfoServices
{
    #region Fields
    private int _loadingCount;
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
        get => _loadingCount >0;
        set
        {
            if (value)
            {
                // set to1 and raise only if previously zero
                var prev = Interlocked.Exchange(ref _loadingCount,1);
                if (prev ==0) OnStatusInfoChanged();
            }
            else
            {
                // set to0 and raise if it was previously >0
                var prev = Interlocked.Exchange(ref _loadingCount,0);
                if (prev >0) OnStatusInfoChanged();
            }
        }
    }

    /// <summary>
    /// Acquire a loading token. When the returned IDisposable is disposed the loading count is decremented.
    /// Use this when multiple independent callers need to indicate "loading" and you want IsLoading to be true
    /// until every caller has finished (disposed their token).
    /// </summary>
    public IDisposable BeginLoading()
    {
        var newVal = Interlocked.Increment(ref _loadingCount);
        if (newVal ==1)
        {
            OnStatusInfoChanged();
        }

        return new ActionOnDispose(() =>
        {
            bool raise = false;

            // Decrement safely without allowing negative counts
            while (true)
            {
                var current = Volatile.Read(ref _loadingCount);
                if (current <=0) break;
                var desired = current -1;
                if (Interlocked.CompareExchange(ref _loadingCount, desired, current) == current)
                {
                    if (desired ==0) raise = true;
                    break;
                }
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
    private sealed partial class ActionOnDispose : IDisposable
    {
        private readonly Action _onDispose;
        private int _disposed;

        public ActionOnDispose(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed,1) ==0)
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
