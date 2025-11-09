using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ConsultantPage - håndterer brugerudvalg, navigation og dashboard-visning for konsulenter.
/// 
/// Ansvar:
/// - Modtager Role objekt fra LoginPage via InitializeAsync()
/// - Loader alle tilgængelige personer med Consultant rolle fra repositories
/// - Filtrerer personer baseret på rolle og aktiv status
/// - Sorterer personer alfabetisk (fornavn, efternavn)
/// - Håndterer brugerudvalg fra dropdown-menuen
/// - Håndterer navigation mellem forskellige sektioner (Dashboards, Gårde og Natur Check, Mine opgaver)
/// - Loader dashboard for den valgte konsulent
/// 
/// Brug:
/// Denne ViewModel bruges af ConsultantPage.xaml til at vise en dropdown med alle konsulenter,
/// navigation mellem forskellige sektioner, og når en konsulent vælges, vises deres dashboard.
/// ViewModel'en håndterer også loading state for at vise progress indicator mens data loades.
/// </summary>
public class ConsultantPageViewModel : ViewModelBase
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
    private string _currentNavigationTag = "Farms"; // Default to "Gårde og Natur Check"

    #endregion

    #region Commands

    /// <summary>
    /// Command to choose a user from the dropdown.
    /// Modtager Person objekt som parameter og loader dashboardet for den valgte konsulent.
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
        _currentNavigationTag = tag;
        // Handle navigation logic here
        // For example, switch content based on tag
        switch (tag)
        {
            case "Dashboards":
                // Navigate to or show dashboards view
                break;
            case "Farms":
                // Show farms and nature check view (current view)
                break;
            case "Tasks":
                // Navigate to or show tasks view
                break;
        }
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
