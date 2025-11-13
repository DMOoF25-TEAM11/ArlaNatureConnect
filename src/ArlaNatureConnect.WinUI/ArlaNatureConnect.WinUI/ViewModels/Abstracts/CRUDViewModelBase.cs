using System.Windows.Input;

using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Commands;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

public abstract class CRUDViewModelBase<TRepos, TEntity> : ViewModelBase
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields
    // Service for displaying status information to the user
    protected IStatusInfoServices _statusInfoServices;
    // Service for displaying application messages
    protected IAppMessageService _appMessageService;
    // The repository for performing CRUD operations
    protected readonly TRepos _repository;
    // The entity being created, read, updated, or deleted
    protected TEntity? _entity;
    #endregion
    #region Fields Commands
    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion
    #region Event handlers
    public event EventHandler<TEntity?>? EntitySaved;

    #endregion

    protected CRUDViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository)
    {
        _statusInfoServices = statusInfoServices;
        _appMessageService = appMessageService;
        _repository = repository;

        AddCommand = new RelayCommand(async () => await OnAddAsync(), CanAdd);
        SaveCommand = new RelayCommand(async () => await OnSaveAsync(), CanSave);
        DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), CanDelete);
        CancelCommand = new RelayCommand(async () => await OnCancelAsync(), CanCancel);
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
            _appMessageService.ErrorMessages.Append("Error loading entity: " + ex.Message);
        }
        finally
        {
            OnPropertyChanged(nameof(Entity));
        }
    }

    #region Properties
    protected bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        protected set
        {
            if (_isSaving == value) return;
            _isSaving = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }
    protected bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        protected set
        {
            if (_isEditMode == value) return;
            _isEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAddMode));
            RefreshCommandStates();
        }
    }
    public bool IsAddMode => !IsEditMode;
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
    protected virtual bool CanSubmitCore() => !IsSaving && !_appMessageService.HasErrorMessages;
    protected virtual bool CanAdd() => CanSubmitCore() && IsAddMode;
    protected virtual bool CanSave() => CanSubmitCore() && IsEditMode;
    protected virtual bool CanDelete() => IsEditMode && !IsSaving;
    protected virtual bool CanCancel()
    {
        return true;
    }
    #endregion
    #region OnXXX Command
    protected async virtual Task OnAddAsync()
    {
        if (!CanAdd()) return;
        IsSaving = true;

        await Task.CompletedTask;
    }
    protected async virtual Task OnSaveAsync()
    {
        await Task.CompletedTask;
    }
    protected async virtual Task OnDeleteAsync()
    {
        await Task.CompletedTask;
    }
    protected async Task OnCancelAsync() => await OnResetAsync();
    protected async Task OnResetAsync()
    {
        _appMessageService.ClearErrorMessages();
        _entity = null;
        await OnResetFormAsync();
        IsEditMode = false;
        await Task.CompletedTask;
    }

    // Abstract hooks for derived view models:
    protected abstract Task OnResetFormAsync();
    protected abstract Task<TEntity> OnAddFormAsync();
    protected abstract Task OnSaveFormAsync();
    protected abstract Task OnLoadFormAsync(TEntity entity);

    #endregion
    #region Helpers
    protected void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
    #endregion
}
