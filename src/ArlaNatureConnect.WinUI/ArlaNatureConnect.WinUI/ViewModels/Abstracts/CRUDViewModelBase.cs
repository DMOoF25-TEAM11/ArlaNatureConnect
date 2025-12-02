using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Commands;

using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base view-model that provides a lightweight scaffold for Create / Read / Update / Delete workflows.
/// </summary>
/// <typeparam name="TRepos">Items type used to persist <typeparamref name="TEntity"/>. Must implement <see cref="IRepository{TEntity}"/>.</typeparam>
/// <typeparam name="TEntity">Entity type managed by the view-model.</typeparam>
public abstract class CRUDViewModelBase<TRepos, TEntity>
    : ListViewModelBase<TRepos, TEntity>
    where TRepos : notnull, IRepository<TEntity>
    where TEntity : class
{
    #region Fields
    protected bool _isSaving;
    protected bool _isEditMode;
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
    #region Properties
    protected TRepos Repository { get; set; }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="CRUDViewModelBase{TRepos, TEntity}"/> class.
    /// </summary>
    /// <param name="statusInfoServices">Service used to report loading state and connection information.</param>
    /// <param name="appMessageService">Service used to show messages and errors to the UI.</param>
    /// <param name="repository">Items used to load and persist <typeparamref name="TEntity"/> instances.</param>
    protected CRUDViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository, bool autoLoad = true)
        : base(statusInfoServices, appMessageService, repository, autoLoad)
    {
        // copy repository reference into this class's backing field so both _items (base) and Repository (derived) are usable
        Repository = repository;

        SelectedEntityChanged += CRUDViewModelBase_SelectedEntityChanged;

        AddCommand = new RelayCommand(async () => await OnAddAsync(), CanAdd);
        SaveCommand = new RelayCommand(async () => await OnSaveAsync(), CanSave);
        DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), CanDelete);
        CancelCommand = new RelayCommand(async () => await OnCancelAsync(), CanCancel);
        RefreshCommand = new RelayCommand(() => RefreshCommandStates());
    }

    //protected CRUDViewModelBase(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, TRepos repository)
    //    : this(statusInfoServices, appMessageService, repository, true)
    //{
    //    SelectedEntityChanged += CRUDViewModelBase_SelectedEntityChanged;
    //}




    #region Load handler
    /// <summary>
    /// Loads an entity by id and prepares the view-model for edit mode if the entity exists.
    /// This method hides the base implementation to augment loading with form hooks and edit-mode state.
    /// </summary>
    /// <param name="id">Identifier of the entity to load.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    /// The implementation uses <see cref="IStatusInfoServices.BeginLoadingOrSaving"/> to report loading state and
    /// calls <see cref="OnLoadFormAsync(TEntity)"/> when an entity is found. Exceptions are swallowed intentionally
    /// to keep UI flows predictable; derived implementations should surface errors via <see cref="IAppMessageService"/> when appropriate.
    /// </remarks>
    public async Task LoadAsync(Guid id)
    {
        // report loading status (BeginLoadingOrSaving should return an IDisposable that clears the loading flag)
        using (_statusInfoServices.BeginLoadingOrSaving())
        {

            try
            {
                // Call repository inside try so synchronous exceptions are caught here
                Task<TEntity?> loadTask = Repository.GetByIdAsync(id);
                TEntity? entity = await loadTask;

                // Set the backing Entity and invoke load hook if found
                SelectedItem = entity;

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
                OnPropertyChanged(nameof(SelectedItem));
                // Raise for SelectedItem (existing property) and for "Entity" which some consumers
                // (including tests) expose as an alias to SelectedItem in derived classes.
                // Raising both ensures listeners observing either name are notified.
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged("Entity");
            }
        }
    }

    #endregion
    #region Commands

    #endregion
    #region CanXXX Command
    /// <summary>
    /// Core check used by command CanExecute logic. Incorporates save state and application error messages.
    /// </summary>
    protected virtual bool CanSubmitCore() => !IsSaving && !_appMessageService.HasErrorMessages;

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
        if (!CanSave()) return;
        IsSaving = true;
        try
        {
            // Call the derived implementation that performs the actual form save logic
            // Do not use ConfigureAwait(false) here so continuations (reload/collection updates)
            // run on the captured synchronization context (UI thread).
            await OnSaveFormAsync();
        }
        finally
        {
            IsSaving = false;
        }
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
        _appMessageService.ClearErrorMessages();
        SelectedItem = null!;
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
    #region Events and Handlersprotected
    private void CRUDViewModelBase_SelectedEntityChanged(object? sender, TEntity? e)
    {
        _ = HandleSelectedItemChangeAsync(e);
    }
    #endregion
    #region Helpers
    /// <summary>
    /// Raises <see cref="ICommand.CanExecuteChanged"/> on internal commands to re-evaluate availability.
    /// </summary>
    protected void RefreshCommandStates()
    {
        // Each RaiseCanExecuteChanged may trigger UI handlers that can throw (for example COMExceptions
        // from UI interop). Invoke each command's raiser individually and swallow exceptions so that
        // a single faulty handler cannot break view-model logic. This mirrors the defensive approach
        // used in other services (e.g. StatusInfoService.NotifyStatusChanged) and in ViewModelBase
        // where COMExceptions from PropertyChanged handlers are swallowed.
        try { (AddCommand as RelayCommand)?.RaiseCanExecuteChanged(); } catch (System.Runtime.InteropServices.COMException) { } catch { }
        try { (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); } catch (System.Runtime.InteropServices.COMException) { } catch { }
        try { (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged(); } catch (System.Runtime.InteropServices.COMException) { } catch { }
        try { (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged(); } catch (System.Runtime.InteropServices.COMException) { } catch { }
        try { (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged(); } catch (System.Runtime.InteropServices.COMException) { } catch { }
    }

    private async Task HandleSelectedItemChangeAsync(TEntity? item)
    {
        try
        {
            if (item == null)
            {
                await OnResetFormAsync().ConfigureAwait(true);
                IsEditMode = false;
            }
            else
            {
                await OnLoadFormAsync(item).ConfigureAwait(true);
                IsEditMode = true;
            }
        }
        catch { }
    }
    #endregion
}
