using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Consultant;

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

    private readonly NavigationHandler? _navigationHandler;
    private readonly IPersonRepository? _personRepository;
    private readonly IRoleRepository? _roleRepository;
    private Person? _selectedPerson;
    private List<Person> _availablePersons = new();
    private bool _isLoading = false;
    private object? _selectedNavigationItem;

    #endregion

    #region Commands
    #endregion

    #region Properties

    /// <summary>
    /// The currently selected navigation item.
    /// </summary>
    public object? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            _selectedNavigationItem = value;
            OnPropertyChanged();
        }
    }

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

    #region OnChooseUser Command
    #endregion

    #region Navigation Handler

    /// <summary>
    /// Handles navigation item selection.
    /// </summary>
    /// <param name="tag">The tag of the selected navigation item.</param>
    public void OnNavigationItemSelected(string tag)
    {
        NavigateToView(tag);
    }

    #endregion

    #region Helpers
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
