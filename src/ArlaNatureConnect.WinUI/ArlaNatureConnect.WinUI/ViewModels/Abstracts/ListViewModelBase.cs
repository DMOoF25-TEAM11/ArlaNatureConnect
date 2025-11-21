using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base ViewModel for list views.
/// Provides the foundational properties and methods needed to manage and interact with a list of
/// entities in the user interface, including loading entities and handling common operations.
/// </summary>
/// <typeparam name="TRepos">The type of the repository used to access the entities.</typeparam>
/// <typeparam name="TEntity">The type of the entities being managed.</typeparam>
public abstract class ListViewModelBase<TRepos, TEntity> : ViewModelBase
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields
    protected readonly TRepos _repository;
    protected TEntity? _entity;
    protected IStatusInfoServices _statusInfoServices;
    protected IAppMessageService _appMessageService;
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewModelBase{TRepos,TEntity}"/> class.
    /// </summary>
    /// <param name="statusInfoServices">Service used to report status information (loading, connectivity).</param>
    /// <param name="appMessageService">Service used to report application messages and errors to the UI.</param>
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
    /// The method reports loading status through <see cref="IStatusInfoServices"/>,
    /// and any errors are reported via <see cref="IAppMessageService"/>.
    /// </summary>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
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
    #region Commands
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    #endregion
    #region Helpers
    #endregion
}
