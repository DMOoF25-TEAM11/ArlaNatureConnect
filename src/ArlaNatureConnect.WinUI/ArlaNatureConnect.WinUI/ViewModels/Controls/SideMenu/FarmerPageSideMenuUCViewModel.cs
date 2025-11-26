using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Farmer;

using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

/// <summary>
/// View-model for the farmer side-menu user control.
/// </summary>
/// <remarks>
/// This view-model provides the data and actions required by the farmer side-menu UI.
/// It reuses common functionality from <see cref="SideMenuViewModelBase"/> such as
/// loading available persons for a role and exposing the selected person. Keeping role-
/// specific logic in a small derived class keeps the codebase modular and easier to test.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="FarmerPageSideMenuUCViewModel"/> class.
/// </remarks>
/// <param name="statusInfoServices">Service used to display loading/connection status to the UI.</param>
/// <param name="appMessageService">Service used to report informational and error messages to the UI.</param>
/// <param name="personRepository">Repository used to load person entities from the data store.</param>
/// <remarks>
/// The constructor delegates most work to the base type which wires up the repository and
/// status/message services. Keeping constructor minimal avoids doing async work during
/// construction — async initialization is performed via <see cref="InitializeAsync"/>.
/// </remarks>
public sealed partial class FarmerPageSideMenuUCViewModel : SideMenuViewModelBase
{
    #region Types
    // simple model for dynamic navigation buttons
    public sealed record NavItem(string Label, System.Windows.Input.ICommand? Command = null);
    #endregion

    #region Properties
    // Collection exposed to the view to generate navigation buttons dynamically
    public ObservableCollection<NavItem> NavItems { get; } = new();

    // Per-item RelayCommands
    public ICommand DashboardsCommand { get; }
    public ICommand NaturCheckCommand { get; }
    public ICommand TasksCommand { get; }
    #endregion


    public FarmerPageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository personRepository)
        : base(statusInfoServices, appMessageService, personRepository)
    {
        // Initialize per-item commands
        DashboardsCommand = new RelayCommand(OnDashboardsExecuted, CanDashboardsExecute);
        NaturCheckCommand = new RelayCommand(OnNaturCheckExecuted, CanNaturCheckExecute);
        TasksCommand = new RelayCommand(OnTasksExecuted, CanTasksExecute);

        // register navigation factories used by the base to create page contents, include per-item command so buttons can bind directly
        NavItems.Add(new NavItem("Dashboards", DashboardsCommand));
        NavItems.Add(new NavItem("Natur Check", NaturCheckCommand));
        NavItems.Add(new NavItem("Mine opgaver", TasksCommand));

        // Ensure command raises CanExecuteChanged when IsLoading changes
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsLoading))
            {
                (DashboardsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (NaturCheckCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (TasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        };

        // Fire-and-forget initialization; exceptions handled inside InitializeAsync
        _ = InitializeAsync();
    }


    #region Command Handlers
    // Per-item execute/can methods
    private void OnDashboardsExecuted()
    {
        NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerDashboards()));
    }
    private bool CanDashboardsExecute() => !IsLoading;

    private void OnNaturCheckExecuted()
    {
        NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerNatureCheck()));
    }
    private bool CanNaturCheckExecute() => !IsLoading;

    private void OnTasksExecuted()
    {
        NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerTasks()));
    }
    private bool CanTasksExecute() => !IsLoading;

    #endregion

    /// <summary>
    /// Performs asynchronous initialization for the view-model.
    /// </summary>
    /// <returns>
    /// A task that completes when the available persons for the farmer role have been loaded.
    /// </returns>
    /// <remarks>
    /// This method deliberately delegates to the shared <see cref="SideMenuViewModelBase.LoadAvailablePersonsAsync(string)"/>
    /// implementation so role-specific view-models only need to specify the role name. Keeping this
    /// method small makes it easier to call from the view's lifecycle code (for example when the
    /// view is loaded) without duplicating repository logic.
    /// </remarks>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            // Load the list of persons that have the Farmer role so the side menu can display them.
            await LoadAvailablePersonsAsync(RoleName.Farmer);
        }
        finally
        {
            IsLoading = false;
        }
        await Task.CompletedTask;
    }
}
