using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Administrator;

using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

/// <summary>
/// View-model for the administrator side-menu user control.
/// </summary>
public sealed partial class AdministratorPageSideMenuUCViewModel : SideMenuViewModelBase
{
    #region Constants
    private const string _labelDashboards = "Dashboards";
    private const string _labelAdministratePersons = "Administrere personer";
    #endregion
    #region Properties
    public ObservableCollection<NavItem> NavItems { get; } = new();
    #endregion
    #region Properties Commands
    public ICommand DashboardsCommand { get; }
    public ICommand AdministratePersonsCommand { get; }
    #endregion

    // DI constructor used at runtime
    public AdministratorPageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository personRepository)
        : base(statusInfoServices, appMessageService, personRepository)
    {
        using (_statusInfoServices!.BeginLoading())
        {
            // Fire-and-forget initialization; exceptions handled inside InitializeAsync
            _ = InitializeAsync();

            // Initialize per-item commands
            DashboardsCommand = new RelayCommand(OnDashboardsExecuted, CanDashboardsExecute);
            AdministratePersonsCommand = new RelayCommand(OnAdministratePersonsExecuted, CanAdministratePersonsExecute);

            // register navigation factories used by the base to create page contents, include per-item command so buttons can bind directly
            NavItems.Add(new NavItem(_labelDashboards, DashboardsCommand));
            NavItems.Add(new NavItem(_labelAdministratePersons, AdministratePersonsCommand));

            // Ensure command raises CanExecuteChanged when IsLoading changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                {
                    (DashboardsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AdministratePersonsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }
    }

    #region Load Helpers
    /// <summary>
    /// Performs asynchronous initialization for the administrator side-menu view-model.
    /// </summary>
    public async Task InitializeAsync()
    {
        using (_statusInfoServices!.BeginLoading())
        {
            IsLoading = true;
            try
            {
                await LoadAvailablePersonsAsync(RoleName.Admin);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    #endregion
    #region Command Handlers
    private void OnDashboardsExecuted()
    {
        SetSelectedByLabel(_labelDashboards);
        NavigationCommand?.Execute(new Func<UserControl?>(() => new AdministratorDashboardUC()));
    }
    private bool CanDashboardsExecute() => !IsLoading;

    private void OnAdministratePersonsExecuted()
    {
        SetSelectedByLabel(_labelAdministratePersons);
        NavigationCommand?.Execute(new Func<UserControl?>(() => new AdministratePersonUC()));
    }
    private bool CanAdministratePersonsExecute() => !IsLoading;
    #endregion
    #region Private Methods 

    private void SetSelectedByLabel(string label)
    {
        foreach (NavItem item in NavItems)
        {
            item.IsSelected = item.Label == label;
        }
    }

    #endregion
}