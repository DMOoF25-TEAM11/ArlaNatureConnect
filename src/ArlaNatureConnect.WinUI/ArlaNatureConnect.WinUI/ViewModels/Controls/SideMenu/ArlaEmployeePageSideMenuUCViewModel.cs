using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.ArlaEmployee;

using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

/// <summary>
/// View-model for the Arla Employee side-menu user control.
/// </summary>
public sealed partial class ArlaEmployeePageSideMenuUCViewModel : SideMenuViewModelBase
{
    #region Constants
    private const string _LABEL_DASHBOARDS = "Dashboards";
    private const string _LABEL_FARMS = "Gårde";
    private const string _LABEL_USERS = "Brugere";
    #endregion

    #region Fields
    // Cache property info for better performance (avoid reflection overhead on each navigation)
    private static readonly PropertyInfo? _currentContentProperty = typeof(NavigationViewModelBase).GetProperty("CurrentContent", BindingFlags.Public | BindingFlags.Instance);
    #endregion

    #region Properties
    // Collection exposed to the view to generate navigation buttons dynamically
    public ObservableCollection<NavItem> NavItems { get; } = new();
    #endregion

    #region Commands
    public ICommand DashboardsCommand { get; }
    public ICommand FarmsCommand { get; }
    public ICommand UsersCommand { get; }
    #endregion

    public ArlaEmployeePageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository personRepository,
        INavigationHandler navigationHandler)
        : base(statusInfoServices, appMessageService, personRepository, navigationHandler)
    {
        using (_statusInfoServices!.BeginLoadingOrSaving())
        {
            // Fire-and-forget initialization; exceptions handled inside InitializeAsync
            _ = InitializeAsync();

            // Initialize per-item commands
            DashboardsCommand = new RelayCommand(OnDashboardsExecuted, CanDashboardsExecute);
            FarmsCommand = new RelayCommand(OnFarmsExecuted, CanFarmsExecute);
            UsersCommand = new RelayCommand(OnUsersExecuted, CanUsersExecute);

            // Register navigation factories used by the base to create page contents, include per-item command so buttons can bind directly
            NavItems.Add(new NavItem(_LABEL_DASHBOARDS, DashboardsCommand));
            NavItems.Add(new NavItem(_LABEL_FARMS, FarmsCommand));
            NavItems.Add(new NavItem(_LABEL_USERS, UsersCommand));

            // Ensure command raises CanExecuteChanged when IsLoadingOrSaving changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                {
                    (DashboardsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (FarmsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UsersCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }
    }

    #region Command Handlers
    private void OnDashboardsExecuted()
    {
        using (_statusInfoServices!.BeginLoadingOrSaving())
        {
            SetSelectedByLabel(_LABEL_DASHBOARDS);
            NavigationCommand?.Execute(new Func<UserControl?>(() => new ArlaEmployeeDashboards()));
        }
    }
    private bool CanDashboardsExecute() => !IsLoading;

    private void OnFarmsExecuted()
    {
        SetSelectedByLabel(_LABEL_FARMS);
        // For Farms view, we need to set DataContext to AssignNatureCheckViewModel
        // We'll handle this directly in OnNavigate override
        NavigationCommand?.Execute(new Func<UserControl?>(() => new ArlaEmployeeAssignNatureCheck()));
    }
    private bool CanFarmsExecute() => !IsLoading;

    private void OnUsersExecuted()
    {
        SetSelectedByLabel(_LABEL_USERS);
        NavigationCommand?.Execute(new Func<UserControl?>(() => new ArlaEmployeeUsers()));
    }
    private bool CanUsersExecute() => !IsLoading;
    #endregion

    private void SetSelectedByLabel(string label)
    {
        foreach (NavItem item in NavItems)
        {
            item.IsSelected = item.Label == label;
        }
    }

    /// <summary>
    /// Override OnNavigate to handle DataContext assignment for Farms view.
    /// This ensures DataContext is set correctly before binding evaluation, preventing binding failures.
    /// </summary>
    protected override void OnNavigate(object? parameter)
    {
        // If this is a factory for ArlaEmployeeAssignNatureCheck, handle it specially
        if (parameter is Func<object?> factory && _navigationViewModel is ViewModels.Pages.ArlaEmployeePageViewModel pageVm)
        {
            object? obj = null;
            try { obj = factory(); } catch { obj = null; }

            if (obj is ArlaEmployeeAssignNatureCheck farmsView)
            {
                // Set DataContext to AssignNatureCheckViewModel BEFORE setting CurrentContent
                // This prevents binding failures during XAML evaluation
                farmsView.DataContext = pageVm.AssignNatureCheckViewModel;

                // Set CurrentContent using cached property info (better performance than reflection on each call)
                if (_navigationViewModel is NavigationViewModelBase hostVm && _currentContentProperty != null)
                {
                    _currentContentProperty.SetValue(hostVm, farmsView);
                }
                return;
            }
        }

        // For all other cases, use base implementation
        base.OnNavigate(parameter);
    }

    /// <summary>
    /// Performs asynchronous initialization for the Arla Employee side-menu view-model.
    /// </summary>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            // Load the list of persons that have the Employee role so the side menu can display them.
            await LoadAvailablePersonsAsync(RoleName.Employee);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
