using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Abstract;

// Add dispatcher support
using Microsoft.UI.Dispatching;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base ViewModel for list views.
/// Provides the foundational properties and methods needed to manage and interact with a list of
/// entities in the user interface, including loading entities and handling common operations.
/// </summary>
/// <typeparam name="TRepos">The type of the repository used to access the entities. Must implement <see cref="IRepository{T}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the entities being managed. Must be a reference type.</typeparam>
/// <remarks>
/// Derive concrete list view models from this class to obtain a repository reference, shared
/// status and messaging services, and a standard asynchronous <see cref="LoadAsync(Guid)"/> pattern.
/// </remarks>
public abstract class ListViewModelBase<TRepos, TEntity> : ViewModelBase
    where TRepos : IRepository<TEntity>
    where TEntity : IEntity
{
    #region Fields

    protected TEntity? _selectedItem;

    /// <summary>
    /// Items used to access entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    protected TRepos _items;

    /// <summary>
    /// Service used to report loading and connectivity status to the UI.
    /// </summary>
    protected IStatusInfoServices _statusInfoServices;

    /// <summary>
    /// Service used to report application messages and errors to the UI.
    /// </summary>
    protected IAppMessageService _appMessageService;
    #endregion
    #region Events
    public event EventHandler<TEntity?>? SelectedEntityChanged;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewModelBase{TRepos,TEntity}"/> class.
    /// </summary>
    /// <param name="statusInfoServices">
    /// Service used to report status information such as loading state and DB connectivity.
    /// </param>
    /// <param name="appMessageService">
    /// Service used to surface application messages and errors to the UI.
    /// </param>
    /// <param name="repository">Items used to access <typeparamref name="TEntity"/> instances.</param>
    /// <param name="autoLoad">If true the Items collection will be automatically loaded by calling <see cref="LoadAllAsync"/>; otherwise loading is deferred.</param>
    protected ListViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository, bool autoLoad = true)
    {
        _statusInfoServices = statusInfoServices;
        _appMessageService = appMessageService;
        _items = repository;

        // Subscribe to status service property changes so view-models forward those notifications
        // to their own PropertyChanged event. This allows consumers to bind to service properties
        // (for example IsLoadingOrSaving) through the view-model.
        try
        {
            _statusInfoServices.PropertyChanged += StatusInfoServices_PropertyChanged;
        }
        catch
        {
            // swallow - best-effort subscription for mocked or partial services
        }

        // Start loading items (fire-and-forget) when requested. LoadAllAsync updates the ObservableCollection on the UI thread.
        if (autoLoad)
        {
            // Try to schedule the load on the UI dispatcher so ObservableCollection updates happen on the UI thread.
            try
            {
                DispatcherQueue? dq = DispatcherQueue.GetForCurrentThread();
                if (dq != null && dq.HasThreadAccess)
                {
                    // Already on UI thread: start load directly
                    _ = LoadAllAsync();
                }
                else if (dq != null)
                {
                    // Enqueue to UI thread
                    dq.TryEnqueue(async () => await LoadAllAsync());
                }
                else
                {
                    // No dispatcher available; run in background but ensure LoadAllAsync will marshal updates if needed
                    _ = Task.Run(async () => await LoadAllAsync());
                }
            }
            catch
            {
                // Best-effort fallback
                _ = LoadAllAsync();
            }
        }
    }

    private void StatusInfoServices_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            // Forward the property name from the status service to this view-model's PropertyChanged
            // so consumers observing the view-model receive the same property notifications.
            OnPropertyChanged(e.PropertyName);
        }
        catch
        {
            // swallow to avoid tests failing due to unexpected exceptions from handlers
        }
    }

    #region Observables Properties
    public TEntity? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (object.Equals(_selectedItem, value)) return;
            _selectedItem = value;
            OnPropertyChanged();

            // Call synchronous hook so derived classes can react immediately to selection changes
            try
            {
                OnSelectedItemChanged(_selectedItem);
                SelectedEntityChanged?.Invoke(this, _selectedItem);
            }
            catch
            {
                // Swallow - view-models should not throw from selection changed handlers
            }
        }
    }

    /// <summary>
    /// Collection of entities exposed to the view. ObservableCollection ensures UI updates when items change.
    /// </summary>
    public ObservableCollection<TEntity> Items { get; } = [];
    #endregion

    /// <summary>
    /// Hook invoked synchronously when the SelectedItem property changes.
    /// Derived classes may override to update dependent state (for example load a form).
    /// Implementations should avoid long-running work since this is invoked on the caller thread.
    /// </summary>
    /// <param name="item">The newly selected item (may be null).</param>
    protected virtual void OnSelectedItemChanged(TEntity? item)
    {
        // default no-op
    }

    #region Load Handlers
    /// <summary>
    /// Loads all entities from the repository and populates the Items collection.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task LoadAllAsync(CancellationToken ct = default)
    {
        using (_statusInfoServices.BeginLoadingOrSaving())
        {
            try
            {
                IEnumerable<TEntity> all = await _items.GetAllAsync(ct);
                Items.Clear();
                foreach (TEntity it in all ?? [])
                {
                    Items.Add(it);
                }
                SelectedItem = default;
            }
            catch (Exception ex)
            {
                // Report error to UI and do not propagate for fire-and-forget/automatic load scenarios
                try { _appMessageService?.AddErrorMessage("Failed to load items: " + ex.Message); } catch { }
                // Swallow the exception so callers (especially fire-and-forget callers) don't observe it.
                return;
            }
            finally
            {
                OnPropertyChanged(nameof(Items));
            }
        }
    }
    #endregion

}
