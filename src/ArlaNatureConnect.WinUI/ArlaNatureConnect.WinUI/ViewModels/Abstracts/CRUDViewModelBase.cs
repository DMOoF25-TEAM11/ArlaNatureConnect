using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

public abstract class CRUDViewModelBase<TRepos, TEntity> : ViewModelBase
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields
    // Service for displaying status information to the user
    IStatusInfoServices StatusInfoServices;
    // Service for displaying application messages
    IAppMessageService AppMessageService;
    // The repository for performing CRUD operations
    protected readonly TRepos _repository;
    // The entity being created, read, updated, or deleted
    protected TEntity? _entity;
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    protected CRUDViewModelBase(IStatusInfoServices statusInfoServices, TRepos repository)
    {
        StatusInfoServices = statusInfoServices;
        _repository = repository;
    }

    public async Task LoadAsync(Guid id)
    {

        _entity = null;
        try
        {
            TEntity? entity = await _repository.GetByIdAsync(id);
            if (entity is null)
            {

            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log error, show message to user)
            AppMessageService.ErrorMessages.Append("Error loading entity: " + ex.Message);
        }
        finally
        {
            OnPropertyChanged(nameof(Entity));
        }
    }

    #region Properties
    #endregion
    #region Observables Properties
    public TEntity? Entity
    {
        get => _entity;
        set => _entity = value;
    }

    public TRepos Repository
    {
        get => _repository;
    }
    #endregion
    #region Load handler
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
