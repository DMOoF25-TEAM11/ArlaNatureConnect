using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums; // add content controls namespace
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Farmer;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for FarmerPage - handles user selection and dashboard display for farmers.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via InitializeAsync()
/// - Loads all available persons with Farmer role from repositories
/// - Filters persons based on role and active status
/// - Sorts persons alphabetically (first name, last name)
/// - Handles user selection from dropdown menu
/// - Loads dashboard for the selected farmer
/// 
/// Usage:
/// This ViewModel is used by FarmerPage.xaml to display a dropdown with all farmers,
/// and when a farmer is selected, their dashboard is displayed. The ViewModel also handles loading state
/// to show progress indicator while data is loading.
/// </summary>
public partial class FarmerPageViewModel : NavigationViewModelBase
{
    #region Fields
    #endregion

    #region Commands
    #endregion

    #region Properties
    #endregion

    public FarmerPageViewModel() : base()
    {

    }

    public FarmerPageViewModel(NavigationHandler navigationHandler, IPersonRepository personRepository, IRoleRepository roleRepository)
        : base(navigationHandler, personRepository, roleRepository)
    {
        InitializeNavigation("Dashboards");

        // ensure initial content is created and bound to this VM
        SwitchContentView(CurrentNavigationTag);
    }

    #region SideMenu Handling
    #endregion

    #region Load Handler
    /// <summary>
    /// Initializes the page with the selected role and loads available users.
    /// </summary>
    /// <param name="role">The role that was selected (should be Farmer).</param>
    public async Task InitializeAsync(Role? role)
    {
        _currentRole = role;
        await LoadAvailableUsersAsync(RoleName.Farmer.ToString());
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
