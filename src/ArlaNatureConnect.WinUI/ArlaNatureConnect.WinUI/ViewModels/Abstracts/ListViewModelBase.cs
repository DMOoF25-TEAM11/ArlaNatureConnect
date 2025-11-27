using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;

using System.Collections.ObjectModel;

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
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields

    protected TEntity _selectedItem;

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
    protected ListViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository)
    {
        _statusInfoServices = statusInfoServices;
        _appMessageService = appMessageService;
        _items = repository;

        // Start loading items (fire-and-forget). LoadAllAsync updates the ObservableCollection on the UI thread.
        _ = LoadAllAsync();
    }

    #region Observables Properties
    public TEntity SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            _selectedItem = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Collection of entities exposed to the view. ObservableCollection ensures UI updates when items change.
    /// </summary>
    public ObservableCollection<TEntity> Items { get; } = new ObservableCollection<TEntity>();
    #endregion
    #region Load Handlers
    /// <summary>
    /// Loads all entities from the repository and populates the Items collection.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task LoadAllAsync(CancellationToken ct = default)
    {
        using (_statusInfoServices.BeginLoading())
        {
            try
            {
                // Await repository call without ConfigureAwait(false) so continuation runs on the captured
                // synchronization context (UI thread) and it's safe to update ObservableCollection directly.
                IEnumerable<TEntity> all = await _items.GetAllAsync(ct);

                // Update collection on UI thread
                Items.Clear();
                foreach (TEntity it in all ?? Array.Empty<TEntity>())
                {
                    Items.Add(it);
                }
            }
            catch (Exception ex)
            {
                // Surface error to UI via AppMessageService rather than throwing from a fire-and-forget task.
                try { _appMessageService?.AddErrorMessage("Failed to load items: " + ex.Message); } catch { }
            }
            finally
            {
                OnPropertyChanged(nameof(Items));
            }
        }
    }
    #endregion

}
