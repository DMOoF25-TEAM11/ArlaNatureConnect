using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;

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
    /// <summary>
    /// Repository used to access entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    protected readonly TRepos _repository;

    /// <summary>
    /// Backing field for the <see cref="Entity"/> property.
    /// </summary>
    protected TEntity? _entity;

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
    /// <param name="repository">Repository used to access <typeparamref name="TEntity"/> instances.</param>
    protected ListViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository)
    {
        _statusInfoServices = statusInfoServices;
        _appMessageService = appMessageService;
        _repository = repository;
    }

    #region Properties
    #endregion

    #region Observables Properties
    /// <summary>
    /// Gets or sets the current entity instance displayed or edited by the view model.
    /// Setting this property raises the <see cref="ViewModelBase.OnPropertyChanged"/> notification.
    /// </summary>
    public TEntity? Entity
    {
        get => _entity;
        set
        {
            _entity = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the repository instance used by the view model to load and persist entities.
    /// </summary>
    public TRepos Repository
    {
        get => _repository;
    }
    #endregion

    #region Load handler
    /// <summary>
    /// Loads an entity by its identifier into the <see cref="Entity"/> property.
    /// </summary>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    /// The method reports loading status via <see cref="IStatusInfoServices.BeginLoading"/> and
    /// propagates errors to <see cref="IAppMessageService.AddErrorMessage"/> instead of throwing.
    /// Implementations of <see cref="IRepository{T}.GetByIdAsync"/> are expected to return the entity or null.
    /// </remarks>
    public async Task LoadAsync(Guid id)
    {
        _entity = null;
        using (_statusInfoServices.BeginLoading())
        {
            try
            {
                Entity = await _repository.GetByIdAsync(id) ?? throw new Exception($"Entity {nameof(Entity)} not found");
            }
            catch (Exception ex)
            {
                _appMessageService.AddErrorMessage("Failed to load entity: " + ex.Message);
            }
        }
    }
    #endregion

    #region Helpers
    #endregion
}
