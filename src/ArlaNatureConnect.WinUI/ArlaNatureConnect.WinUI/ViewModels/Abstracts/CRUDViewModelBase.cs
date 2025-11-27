using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Commands;

using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base view-model that provides a lightweight scaffold for Create / Read / Update / Delete workflows.
/// </summary>
/// <typeparam name="TRepos">Repository type used to persist <typeparamref name="TEntity"/>. Must implement <see cref="IRepository{TEntity}"/>.</typeparam>
/// <typeparam name="TEntity">Entity type managed by the view-model.</typeparam>
public abstract class CRUDViewModelBase<TRepos, TEntity> : ListViewModelBase<TRepos, TEntity>
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields
    // The entity being created, read, updated, or deleted
    // (backing fields and services are inherited from ListViewModelBase)
    // protected TEntity? _entity;
    #endregion
    #region Fields Commands
    /// <summary>
    /// Command used to start adding a new entity.
    /// </summary>
    public ICommand AddCommand { get; }

    /// <summary>
    /// Command used to persist the current entity (add or update depending on mode).
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command used to delete the currently loaded entity.
    /// </summary>
    public ICommand DeleteCommand { get; }

    /// <summary>
    /// Command used to cancel an edit operation and reset the form.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command used to refresh command CanExecute states.
    /// </summary>
    public ICommand RefreshCommand { get; }
    #endregion
    #region Event handlers
    /// <summary>
    /// Raised after an entity has been successfully saved by the view-model.
    /// Listeners can use this event to react to persistence operations (for example navigate or refresh lists).
    /// </summary>
    //public event EventHandler<TEntity?>? EntitySaved;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="CRUDViewModelBase{TRepos, TEntity}"/> class.
    /// </summary>
    /// <param name="statusInfoServices">Service used to report loading state and connection information.</param>
    /// <param name="appMessageService">Service used to surface messages and errors to the UI.</param>
    /// <param name="repository">Repository used to load and persist <typeparamref name="TEntity"/> instances.</param>
    protected CRUDViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository)
        : base(statusInfoServices, appMessageService, repository)
    {
        AddCommand = new RelayCommand(async () => await OnAddAsync(), CanAdd);
        SaveCommand = new RelayCommand(async () => await OnSaveAsync(), CanSave);
        DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), CanDelete);
        CancelCommand = new RelayCommand(async () => await OnCancelAsync(), CanCancel);
        RefreshCommand = new RelayCommand(() => RefreshCommandStates());
    }

    /// <summary>
    /// Loads an entity by id and prepares the view-model for edit mode if the entity exists.
    /// This method hides the base implementation to augment loading with form hooks and edit-mode state.
    /// </summary>
    /// <param name="id">Identifier of the entity to load.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    /// The implementation uses <see cref="IStatusInfoServices.BeginLoading"/> to report loading state and
    /// calls <see cref="OnLoadFormAsync(TEntity)"/> when an entity is found. Exceptions are swallowed intentionally
    /// to keep UI flows predictable; derived implementations should surface errors via <see cref="IAppMessageService"/> when appropriate.
    /// </remarks>
    public new async Task LoadAsync(Guid id)
    {
        // report loading status (BeginLoading should return an IDisposable that clears the loading flag)
        using IDisposable loading = StatusInfoServices.BeginLoading();

        try
        {
            // Call repository inside try so synchronous exceptions are caught here
            Task<TEntity?> loadTask = Repository.GetByIdAsync(id);
            TEntity? entity = await loadTask;

            // Set the backing Entity and invoke load hook if found
            base.Entity = entity;

            if (entity != null)
            {
                await OnLoadFormAsync(entity);
                IsEditMode = true;
            }
            else
            {
                IsEditMode = false;
            }
        }
        catch (Exception)
        {
            // swallow/log as intended by tests (do not add error message)
        }
        finally
        {
            // Ensure PropertyChanged for Entity is raised even when repository throws
            OnPropertyChanged(nameof(Entity));
        }
    }

    #region Properties
    protected bool _isSaving;

    /// <summary>
    /// Gets a value indicating whether a save/delete operation is in progress.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the view-model is in edit mode (an existing entity is loaded).
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the view-model is in add mode (no entity loaded).
    /// </summary>
    public bool IsAddMode => !IsEditMode;
    #endregion
    #region Observables Properties
    /// <summary>
    /// Gets or sets the current entity. This shadows the base property and forwards to <see cref="ListViewModelBase{TRepos,TEntity}.Entity"/>.
    /// Setting the property raises property change notifications.
    /// </summary>
    public new TEntity? Entity
    {
        get => base.Entity;
        set => base.Entity = value;
    }

    /// <summary>
    /// Gets the repository instance used by the view-model. This shadows the base property.
    /// </summary>
    public new TRepos Repository => base.Repository;
    #endregion
    #region Load handler
    #endregion
    #region Commands

    #endregion
    #region CanXXX Command
    /// <summary>
    /// Core check used by command CanExecute logic. Incorporates save state and application error messages.
    /// </summary>
    protected virtual bool CanSubmitCore() => !IsSaving && !AppMessageService.HasErrorMessages;

    /// <summary>
    /// Determines whether the Add command can execute.
    /// </summary>
    protected virtual bool CanAdd() => CanSubmitCore() && IsAddMode;

    /// <summary>
    /// Determines whether the Save command can execute.
    /// </summary>
    protected virtual bool CanSave() => CanSubmitCore() && IsEditMode;

    /// <summary>
    /// Determines whether the Delete command can execute.
    /// </summary>
    protected virtual bool CanDelete() => IsEditMode && !IsSaving;

    /// <summary>
    /// Determines whether the Cancel command can execute. Derived classes may override to add additional checks.
    /// </summary>
    protected virtual bool CanCancel()
    {
        return true;
    }
    #endregion
    #region OnXXX Command
    /// <summary>
    /// Handler invoked by the Add command. Derived classes may override to provide behavior when adding.
    /// The base implementation ensures the command guard and sets <see cref="IsSaving"/> to true. Derived implementations
    /// are expected to complete any required work and reset state as appropriate.
    /// </summary>
    protected async virtual Task OnAddAsync()
    {
        if (!CanAdd()) return;
        IsSaving = true;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Handler invoked by the Save command. Derived classes must override to perform persistence operations.
    /// </summary>
    protected async virtual Task OnSaveAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handler invoked by the Delete command. Derived classes must override to perform deletion.
    /// </summary>
    protected async virtual Task OnDeleteAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handler invoked by the Cancel command; resets the form.
    /// </summary>
    protected async Task OnCancelAsync() => await OnResetAsync();

    /// <summary>
    /// Resets the view-model to a default state and clears error messages.
    /// </summary>
    /// <returns>A completed task when reset is finished.</returns>
    protected async Task OnResetAsync()
    {
        AppMessageService.ClearErrorMessages();
        base.Entity = null;
        await OnResetFormAsync();
        IsEditMode = false;
        await Task.CompletedTask;
    }

    // Abstract hooks for derived view models:
    /// <summary>
    /// Reset form-specific fields and validation state.
    /// </summary>
    protected abstract Task OnResetFormAsync();

    /// <summary>
    /// Create and return a new entity instance from form-bound values when adding.
    /// </summary>
    /// <returns>The entity instance to be added.</returns>
    protected abstract Task<TEntity> OnAddFormAsync();

    /// <summary>
    /// Perform save logic for the current entity (add or update). Derived classes must implement persistence here.
    /// </summary>
    protected abstract Task OnSaveFormAsync();

    /// <summary>
    /// Populate form-specific fields from a loaded entity. Called by <see cref="LoadAsync(Guid)"/> when an entity is found.
    /// </summary>
    /// <param name="entity">Loaded entity instance.</param>
    protected abstract Task OnLoadFormAsync(TEntity entity);

    #endregion
    #region Helpers
    /// <summary>
    /// Raises <see cref="ICommand.CanExecuteChanged"/> on internal commands to re-evaluate availability.
    /// </summary>
    protected void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
    #endregion
}
