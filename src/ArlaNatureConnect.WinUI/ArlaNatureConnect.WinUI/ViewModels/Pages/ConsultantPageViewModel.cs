using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.View.Pages.Consultant;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ConsultantPage - handles user selection, navigation and dashboard display for consultants.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via InitializeAsync()
/// - Loads all available persons with Consultant role from repositories
/// - Filters persons based on role and active status
/// - Sorts persons alphabetically (first name, last name)
/// - Handles user selection from dropdown menu
/// - Handles navigation between different sections (Dashboards, Farms and Nature Check, My Tasks)
/// - Loads dashboard for the selected consultant
/// 
/// Usage:
/// This ViewModel is used by ConsultantPage.xaml to display a dropdown with all consultants,
/// navigation between different sections, and when a consultant is selected, their dashboard is displayed.
/// The ViewModel also handles loading state to show progress indicator while data is loading.
/// </summary>
public class ConsultantPageViewModel : NavigationViewModelBase
{
    #region Fields
    #endregion

    #region Commands
    #endregion

    #region Properties
    #endregion

    #region Constructor

    public ConsultantPageViewModel()
    {

    }

    public ConsultantPageViewModel(NavigationHandler navigationHandler, IPersonRepository personRepository, IRoleRepository roleRepository)
        : base(navigationHandler, personRepository, roleRepository)
    {
        InitializeNavigation("Dashboards");

        // ensure initial content is created and bound to this VM
        SwitchContentView(CurrentNavigationTag);
    }

    #endregion

    #region Load Handler

    /// <summary>
    /// Initializes the page with the selected role and loads available users.
    /// </summary>
    /// <param name="role">The role that was selected (should be Consultant).</param>
    public async Task InitializeAsync(Role? role)
    {
        _currentRole = role;
        await LoadAvailableUsersAsync(RoleName.Consultant.ToString());
    }

    #endregion


    #region Helpers
    /// <summary>
    /// Overrides navigation to also switch the content view when the tag changes.
    /// </summary>
    protected override void NavigateToView(object? parameter)
    {
        base.NavigateToView(parameter);
        // When CurrentNavigationTag is updated, switch the view content
        SwitchContentView(CurrentNavigationTag);
    }

    private void SwitchContentView(string? navigationTag)
    {
        try
        {
            UserControl? newContent = navigationTag switch
            {
                "Dashboards" => new ConsultantDashboards(),
                "Farms" => new ConsultantNatureCheck(),
                "Tasks" => new ConsultantTasks(),
                _ => new ConsultantDashboards(),
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
