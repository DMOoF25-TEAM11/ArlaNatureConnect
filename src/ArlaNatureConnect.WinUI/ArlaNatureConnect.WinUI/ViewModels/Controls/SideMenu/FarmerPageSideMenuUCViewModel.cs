using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Farmer;

using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

/// <summary>
/// View-model for the farmer side-menu user control.
/// </summary>
public sealed partial class FarmerPageSideMenuUCViewModel : SideMenuViewModelBase
{
    #region Constants
    private const string _labelDashboards = "Dashboards";
    private const string _labelNaturCheck = "Natur Check";
    private const string _labelTasks = "Mine opgaver";
    #endregion

    #region Properties
    // Collection exposed to the view to generate navigation buttons dynamically
    public ObservableCollection<NavItem> NavItems { get; } = new();
    #endregion

    #region Properties Commands
    public ICommand DashboardsCommand { get; }
    public ICommand NaturCheckCommand { get; }
    public ICommand TasksCommand { get; }
    #endregion


    public FarmerPageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository personRepository,
        INavigationHandler navigationHandler)
        : base(statusInfoServices, appMessageService, personRepository, navigationHandler)
    {
        using (_statusInfoServices!.BeginLoading())
        {

            // Fire-and-forget initialization; exceptions handled inside InitializeAsync
            _ = InitializeAsync();

            // Initialize per-item commands
            DashboardsCommand = new RelayCommand(OnDashboardsExecuted, CanDashboardsExecute);
            NaturCheckCommand = new RelayCommand(OnNaturCheckExecuted, CanNaturCheckExecute);
            TasksCommand = new RelayCommand(OnTasksExecuted, CanTasksExecute);

            // register navigation factories used by the base to create page contents, include per-item command so buttons can bind directly
            NavItems.Add(new NavItem(_labelDashboards, DashboardsCommand));
            NavItems.Add(new NavItem(_labelNaturCheck, NaturCheckCommand));
            NavItems.Add(new NavItem(_labelTasks, TasksCommand));

            // Ensure command raises CanExecuteChanged when IsLoading changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                {
                    (DashboardsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (NaturCheckCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (TasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }
    }

    #region Command Handlers
    // Per-item execute/can methods
    private void OnDashboardsExecuted()
    {
        using (_statusInfoServices!.BeginLoading())
        {
            SetSelectedByLabel(_labelDashboards);
            NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerDashboards()));
        }
    }
    private bool CanDashboardsExecute() => !IsLoading;

    private void OnNaturCheckExecuted()
    {
        SetSelectedByLabel(_labelNaturCheck);
        NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerNatureCheck()));
    }
    private bool CanNaturCheckExecute() => !IsLoading;

    private void OnTasksExecuted()
    {
        SetSelectedByLabel(_labelTasks);
        NavigationCommand?.Execute(new Func<UserControl?>(() => new FarmerTasks()));
    }
    private bool CanTasksExecute() => !IsLoading;

    #endregion

    private void SetSelectedByLabel(string label)
    {
        foreach (NavItem item in NavItems)
        {
            item.IsSelected = item.Label == label;
        }
    }

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
