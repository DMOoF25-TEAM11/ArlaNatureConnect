using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;
using ArlaNatureConnect.WinUI.View.Pages.Farmer; // add content controls namespace

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

    private readonly NavigationHandler? _navigationHandler;
    private readonly IPersonRepository? _personRepository;
    private readonly IRoleRepository? _roleRepository;
    private Person? _selectedPerson;
    private Role? _currentRole;
    private List<Person> _availablePersons = new();
    private bool _isLoading;
    private UserControl? _currentContent;

    // Side menu handling fields moved from view
    private UIElement[]? _previousSideMenuChildren;
    private FarmerSideMenuUC? _addedSideMenuControl;

    #endregion

    #region Commands

    /// <summary>
    /// Command to choose a user from the dropdown.
    /// Receives Person object as parameter and loads the dashboard for the selected farmer.
    /// </summary>
    public RelayCommand<Person>? ChooseUserCommand { get; }

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
            ChooseUserCommand?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(IsUserSelected));
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
    /// The current content UserControl that should be displayed in the page's ContentPresenter.
    /// The View owns rendering; ViewModel provides the content instance here for simplicity.
    /// </summary>
    public UserControl? CurrentContent
    {
        get => _currentContent;
        private set
        {
            if (_currentContent == value) return;
            _currentContent = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public FarmerPageViewModel() : base()
    {

    }

    public FarmerPageViewModel(NavigationHandler navigationHandler, IPersonRepository personRepository, IRoleRepository roleRepository) : base()
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));

        ChooseUserCommand = new RelayCommand<Person>(ChooseUser, p => p != null);
        InitializeNavigation("Dashboards");

        // ensure initial content is created and bound to this VM
        SwitchContentView(CurrentNavigationTag);
    }

    #region SideMenu Handling

    /// <summary>
    /// Attaches the view model to the Page instance so it can subscribe to Loaded/Unloaded lifecycle events.
    /// This keeps the view code-behind minimal while allowing the view model to manage application-level UI
    /// like the SideMenu region.
    /// </summary>
    /// <param name="page">The FarmerPage instance.</param>
    public void AttachToView(Page? page)
    {
        if (page == null) return;

        // Subscribe to lifecycle events
        page.Loaded += Page_Loaded;
        page.Unloaded += Page_Unloaded;
    }

    private void Page_Loaded(object? sender, RoutedEventArgs e)
    {
        // Ensure SideMenu is attached when view is loaded
        AttachSideMenuToMainWindow();
    }

    private void Page_Unloaded(object? sender, RoutedEventArgs e)
    {
        // Restore SideMenu when view unloads
        RestoreMainWindowSideMenu();

        if (sender is Page page)
        {
            // detach handlers to avoid leaks
            page.Loaded -= Page_Loaded;
            page.Unloaded -= Page_Unloaded;
        }
    }

    /// <summary>
    /// Finds the MainWindow's SideMenu panel and replaces its children with the FarmerSideMenuUC.
    /// Stores previous children so they can be restored later.
    /// Safe no-op when MainWindow or SideMenu cannot be found (e.g. during unit tests).
    /// </summary>
    public void AttachSideMenuToMainWindow()
    {
        try
        {
            MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
            if (mainWindow == null)
                return;

            if (mainWindow.Content is not FrameworkElement root)
                return;

            Panel? SideMenu = FindSideMenuPanel(root);
            if (SideMenu == null)
                return;

            // If we've already added our SideMenu control, ensure DataContext is synced and return
            if (_addedSideMenuControl is not null && SideMenu.Children.Contains(_addedSideMenuControl))
            {
                _addedSideMenuControl.DataContext = this;
                return;
            }

            // Store previous children so they can be restored
            _previousSideMenuChildren = SideMenu.Children.Cast<UIElement>().ToArray();

            SideMenu.Children.Clear();

            _addedSideMenuControl = new FarmerSideMenuUC();
            _addedSideMenuControl.DataContext = this;

            SideMenu.Children.Add(_addedSideMenuControl);
        }
        catch
        {
            // Ignore errors during UI attach (useful for test environments)
        }
    }

    /// <summary>
    /// Restores the MainWindow's SideMenu to its previous children if available.
    /// </summary>
    public void RestoreMainWindowSideMenu()
    {
        try
        {
            MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
            if (mainWindow == null)
                return;

            if (mainWindow.Content is not FrameworkElement root)
                return;

            Panel? SideMenu = FindSideMenuPanel(root);
            if (SideMenu == null)
                return;

            // Only restore if we previously replaced children
            if (_previousSideMenuChildren == null)
                return;

            SideMenu.Children.Clear();
            foreach (UIElement child in _previousSideMenuChildren)
            {
                SideMenu.Children.Add(child);
            }

            _previousSideMenuChildren = null;
            _addedSideMenuControl = null;
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Finds the SideMenu panel starting from the provided root. Tries FindName first then visual tree traversal.
    /// </summary>
    private Panel? FindSideMenuPanel(FrameworkElement root)
    {
        if (root == null) return null;

        // Try straightforward FindName first
        if (root.FindName("SideMenu") is Panel p)
            return p;

        // Fallback: traverse visual tree
        try
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is Panel panel && (panel.Name == "SideMenu"))
                    return panel;

                if (child is FrameworkElement fe)
                {
                    Panel? found = FindSideMenuPanel(fe);
                    if (found != null)
                        return found;
                }
            }
        }
        catch
        {
            // ignore traversal errors
        }

        return null;
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
            IEnumerable<Role> allRoles = await (_roleRepository?.GetAllAsync(CancellationToken.None) 
                ?? Task.FromResult(Enumerable.Empty<Role>()));
            Role? farmerRole = allRoles.FirstOrDefault(r => 
                r.Name.Equals("Farmer", StringComparison.OrdinalIgnoreCase) ||
                r.Name.Equals("Landmand", StringComparison.OrdinalIgnoreCase));

            if (farmerRole == null)
            {
                AvailablePersons = new List<Person>();
                return;
            }

            // Get all persons
            IEnumerable<Person> allPersons = await (_personRepository?.GetAllAsync(CancellationToken.None) 
                ?? Task.FromResult(Enumerable.Empty<Person>()));
            
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

    /// <summary>
    /// Overrides navigation to also switch the content view when the tag changes.
    /// </summary>
    protected override void NavigateToView(string? tag)
    {
        base.NavigateToView(tag);
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
