using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.View.Pages.Farmer;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ArlaEmployeePage - handles dashboard display for Arla employees.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via Initialize()
/// - Stores the selected role for future use
/// - Handles navigation between different sections (Dashboards, Farms, Users)
/// - Prepares dashboard display for Arla employees
/// 
/// Usage:
/// This ViewModel is used by ArlaEmployeePage.xaml to display the dashboard for Arla employees.
/// Unlike FarmerPage and ConsultantPage, Arla employees do not need to select a specific
/// user, as they have direct access to the dashboard. The ViewModel handles navigation between
/// different content views and initialization with the role.
/// </summary>
public class ArlaEmployeePageViewModel : NavigationViewModelBase
{
    #region Fields
    #endregion

    #region Properties

    #endregion

    #region Constructor

    public ArlaEmployeePageViewModel() : base()
    {
        
    }

    public ArlaEmployeePageViewModel(NavigationHandler navigationHandler, IPersonRepository personRepository, IRoleRepository roleRepository) 
        : base(navigationHandler, personRepository, roleRepository)
    {
        InitializeNavigation("Dashboards");

        // ensure initial content is created and bound to this VM
        SwitchContentView(CurrentNavigationTag);
    }

    #endregion

    #region Load Handler

    /// <summary>
    /// Initializes the page with the selected role.
    /// </summary>
    /// <param name="role">The role that was selected (should be ArlaEmployee).</param>
    public async Task InitializeAsync(Role? role)
    {
        _currentRole = role;
        await LoadAvailableUsersAsync(RoleName.Employee.ToString());
    }

    #endregion

    #region Helpers
    /// <summary>
    /// Loads the dashboard for the selected user.
    /// </summary>
    private void LoadDashboard()
    {
        // Dashboard logic will be implemented here        
    }

    /// <summary>
    /// Overrides navigation to also switch the content view when the tag changes.
    /// </summary>
    protected override void NavigateToView(object? parameter)
    {
        base.NavigateToView(parameter);
        // When CurrentNavigationTag is updated, switch the view content
        SwitchContentView(CurrentNavigationTag);
    }

    /// <summary>
    /// Switches the content view by creating the appropriate UserControl and assigning its DataContext.
    /// Note: Creating UI controls in ViewModel is a pragmatic choice for this prototype.
    /// </summary>
    /// <param name="navigationTag">The tag to switch to.</param>
    private void SwitchContentView(string? navigationTag)
    {
        try
        {
            UserControl? newContent = navigationTag switch
            {
                "Dashboards" => new FarmerDashboards(),
                "Farms" => new FarmerNatureCheck(),
                "Tasks" => new FarmerTasks(),
                _ => new FarmerDashboards(),
            };

            if (newContent != null)
            {
                newContent.DataContext = this;
                CurrentContent = newContent;
            }
        }
        catch
        {
            // In unit tests or when XAML context is not available, 
            // we gracefully fail and leave CurrentContent as null
            // This allows tests to run without requiring UI initialization
        }
    }


    #endregion

}
