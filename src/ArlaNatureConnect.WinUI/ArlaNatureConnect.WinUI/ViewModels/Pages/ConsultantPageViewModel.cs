using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

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

    private readonly NavigationHandler _navigationHandler;
    private readonly IPersonRepository _personRepository;
    private readonly IRoleRepository _roleRepository;
    private Person? _selectedPerson;
    private Role? _currentRole;
    private List<Person> _availablePersons = new();
    private bool _isLoading;
    private object? _selectedNavigationItem;

    #endregion

    #region Commands

    /// <summary>
    /// Command to choose a user from the dropdown.
    /// Receives Person object as parameter and loads the dashboard for the selected consultant.
    /// </summary>
    public RelayCommand<Person> ChooseUserCommand { get; }

    #endregion

    #region Properties

    /// <summary>
    /// List of available persons with the Consultant role.
    /// </summary>
    public List<Person> AvailablePersons
    {
        get => _availablePersons;
        private set
        {
            _availablePersons = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The currently selected person.
    /// </summary>
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            _selectedPerson = value;
            OnPropertyChanged();
            ChooseUserCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Indicates whether data is being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether a user has been selected and dashboard should be shown.
    /// </summary>
    public bool IsUserSelected => SelectedPerson != null;

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

    public ConsultantPageViewModel(
        NavigationHandler navigationHandler,
        IPersonRepository personRepository,
        IRoleRepository roleRepository)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        
        ChooseUserCommand = new RelayCommand<Person>(ChooseUser, p => p != null);
        InitializeNavigation("Farms"); // Default to "Farms and Nature Check"
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
        await LoadAvailableUsersAsync();
    }

    #endregion

    #region OnChooseUser Command

    /// <summary>
    /// Chooses a user and loads their dashboard.
    /// </summary>
    /// <param name="person">The person to select.</param>
    private void ChooseUser(Person? person)
    {
        if (person == null)
        {
            return;
        }

        SelectedPerson = person;
        OnPropertyChanged(nameof(IsUserSelected));
        LoadDashboard();
    }

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

    /// <summary>
    /// Loads all available persons with the Consultant role.
    /// </summary>
    private async Task LoadAvailableUsersAsync()
    {
        IsLoading = true;
        try
        {
            // Get all roles to find Consultant role
            var allRoles = await _roleRepository.GetAllAsync();
            var consultantRole = allRoles.FirstOrDefault(r => 
                r.Name.Equals("Consultant", StringComparison.OrdinalIgnoreCase) ||
                r.Name.Equals("Konsulent", StringComparison.OrdinalIgnoreCase));

            if (consultantRole == null)
            {
                AvailablePersons = new List<Person>();
                return;
            }

            // Get all persons
            var allPersons = await _personRepository.GetAllAsync();
            
            // Filter by role and active status
            AvailablePersons = allPersons
                .Where(p => p.RoleId == consultantRole.Id && p.IsActive)
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToList();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads the dashboard for the selected user.
    /// </summary>
    private void LoadDashboard()
    {
        // Dashboard loading logic will be implemented here
        // For now, this is a placeholder
    }

    #endregion
}
