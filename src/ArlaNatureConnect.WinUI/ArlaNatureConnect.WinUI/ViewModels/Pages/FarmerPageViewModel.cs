using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

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
public class FarmerPageViewModel : ViewModelBase
{
    #region Fields

    private readonly NavigationHandler _navigationHandler;
    private readonly IPersonRepository _personRepository;
    private readonly IRoleRepository _roleRepository;
    private Person? _selectedPerson;
    private Role? _currentRole;
    private List<Person> _availablePersons = new();
    private bool _isLoading;

    #endregion

    #region Commands

    /// <summary>
    /// Command to choose a user from the dropdown.
    /// Receives Person object as parameter and loads the dashboard for the selected farmer.
    /// </summary>
    public RelayCommand<Person> ChooseUserCommand { get; }

    #endregion

    #region Properties

    /// <summary>
    /// List of available persons with the Farmer role.
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

    #endregion

    #region Constructor

    public FarmerPageViewModel(
        NavigationHandler navigationHandler,
        IPersonRepository personRepository,
        IRoleRepository roleRepository)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        
        ChooseUserCommand = new RelayCommand<Person>(ChooseUser, p => p != null);
        InitializeNavigation("Dashboards"); // Default to "Dashboards"
    }

    #endregion

    #region Load Handler

    /// <summary>
    /// Initializes the page with the selected role and loads available users.
    /// </summary>
    /// <param name="role">The role that was selected (should be Farmer).</param>
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

    #region Helpers

    /// <summary>
    /// Loads all available persons with the Farmer role.
    /// </summary>
    private async Task LoadAvailableUsersAsync()
    {
        IsLoading = true;
        try
        {
            // Get all roles to find Farmer role
            var allRoles = await _roleRepository.GetAllAsync();
            var farmerRole = allRoles.FirstOrDefault(r => 
                r.Name.Equals("Farmer", StringComparison.OrdinalIgnoreCase) ||
                r.Name.Equals("Landmand", StringComparison.OrdinalIgnoreCase));

            if (farmerRole == null)
            {
                AvailablePersons = new List<Person>();
                return;
            }

            // Get all persons
            var allPersons = await _personRepository.GetAllAsync();
            
            // Filter by role and active status
            AvailablePersons = allPersons
                .Where(p => p.RoleId == farmerRole.Id && p.IsActive)
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
        // Dashboard logic will be implemented here        
    }

    #endregion
}
