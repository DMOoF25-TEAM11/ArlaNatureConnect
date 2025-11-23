using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using System.Diagnostics;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

public abstract class NavigationViewModelBase : ViewModelBase, INavigationViewModel
{
    #region Fields
    private string _currentNavigationTag = string.Empty;

    private readonly NavigationHandler? _navigationHandler;
    private readonly IPersonRepository? _personRepository;
    private readonly IRoleRepository? _roleRepository;
    private Person? _selectedPerson;
    protected Role? _currentRole;
    private List<Person> _availablePersons = new();
    private bool _isLoading;
    private UserControl? _currentContent;

    // Side menu handling fields moved from view
    private UIElement[]? _previousSideMenuChildren;
    private UIElement? _addedSideMenuControl;
    #endregion
    #region Fields Commands
    public RelayCommand<Person>? ChooseUserCommand { get; }
    #endregion
    #region Event handlers
    #endregion

    public NavigationViewModelBase() : base ()
    { 
    }

    protected NavigationViewModelBase(NavigationHandler navigationHandler, IPersonRepository? personRepository, IRoleRepository? roleRepository) :base()
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        ChooseUserCommand = new RelayCommand<Person>(ChooseUser, p => p != null);
    }

    #region Properties
    /// <summary>
    /// Command to navigate between different content views.
    /// Accepts either a string tag or a delegate (Func<string>/Action) as parameter.
    /// Initialized via InitializeNavigation() method.
    /// </summary>
    public RelayCommand<object>? NavigationCommand { get; protected set; }

    #endregion
    #region Observables Properties
    /// <summary>
    /// The currently selected navigation tag.
    /// Used to determine which content view to display and which navigation button is active.
    /// </summary>
    public string CurrentNavigationTag
    {
        get => _currentNavigationTag;
        protected set
        {
            if (_currentNavigationTag != value)
            {
                _currentNavigationTag = value;
                OnPropertyChanged();
            }
        }
    }
    public List<Person> AvailablePersons
    {
        get => _availablePersons;
        private set
        {
            _availablePersons = value ?? new List<Person>();
            OnPropertyChanged();
        }
    }
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
    public bool IsLoading
    {
        get => _isLoading;
        protected set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    public bool IsUserSelected => SelectedPerson != null;
    public UserControl? CurrentContent
    {
        get => _currentContent;
        protected set
        {
            if (_currentContent == value) return;
            _currentContent = value;
            OnPropertyChanged();
        }
    }
    #endregion
    #region Load handler


    #endregion
    #region Commands
    ICommand? INavigationViewModel.NavigationCommand => NavigationCommand;
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    #endregion
    #region Helpers
    /// <summary>
    /// Initializes the navigation command. Should be called in the constructor of derived classes.
    /// </summary>
    /// <param name="defaultTag">The default navigation tag to use when initializing.</param>
    protected void InitializeNavigation(string defaultTag = "")
    {
        _currentNavigationTag = defaultTag;
        NavigationCommand = new RelayCommand<object>(NavigateToView, CanNavigate);
    }

    private bool CanNavigate(object? parameter)
    {
        if (parameter == null) return false;
        if (parameter is string s) return !string.IsNullOrWhiteSpace(s);
        // allow delegates
        if (parameter is Delegate) return true;
        return true;
    }

    /// <summary>
    /// Navigates to the specified content view based on the navigation tag or delegate passed as parameter.
    /// Supports string tags, Func<string> to compute tag, or Action delegates to perform custom behavior.
    /// </summary>
    /// <param name="parameter">The navigation parameter (string or delegate).</param>
    protected virtual void NavigateToView(object? parameter)
    {
        if (parameter == null)
            return;

        // If a string is passed, use it as tag
        if (parameter is string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            CurrentNavigationTag = tag;
            return;
        }

        // If a delegate that returns a string is passed, invoke to get tag
        if (parameter is Func<string> tagFunc)
        {
            try
            {
                string result = tagFunc();
                if (!string.IsNullOrWhiteSpace(result))
                    CurrentNavigationTag = result;
            }
            catch
            {
                // swallow delegate exceptions
            }

            return;
        }

        // If an async-style Func<Task<string>> is passed, try to handle synchronously by getting Result (best-effort)
        if (parameter is System.Func<System.Threading.Tasks.Task<string>> taskFunc)
        {
            try
            {
                Task<string> task = taskFunc();
                task.Wait();
                string result = task.Result;
                if (!string.IsNullOrWhiteSpace(result))
                    CurrentNavigationTag = result;
            }
            catch
            {
            }

            return;
        }

        // If an Action is passed, invoke it (used for page-level actions)
        if (parameter is Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // ignore
            }

            return;
        }

        // If a delegate taking this ViewModel and returning string
        if (parameter is Func<NavigationViewModelBase, string> vmTagFunc)
        {
            try
            {
                string result = vmTagFunc(this);
                if (!string.IsNullOrWhiteSpace(result))
                    CurrentNavigationTag = result;
            }
            catch
            {
            }

            return;
        }

        // otherwise try to convert to string
        try
        {
            string? s = parameter?.ToString();
            if (!string.IsNullOrWhiteSpace(s))
                CurrentNavigationTag = s;
        }
        catch
        {
        }
    }
    protected async Task LoadAvailableUsersAsync(string roleName)
    {
        IsLoading = true;
        try
        {
            List<Person> persons = await _personRepository!.GetPersonsByRoleAsync(roleName);
            AvailablePersons = persons ?? new List<Person>();
            Debug.WriteLine($"Loaded {AvailablePersons.Count} persons with role {roleName}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
        #region SideMenu Handling
    public void AttachToView(Microsoft.UI.Xaml.Controls.Page? page)
    {
        if (page == null) return;

        // Subscribe to lifecycle events
        page.Loaded += Page_Loaded;
        page.Unloaded += Page_Unloaded;
    }
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
            if (_addedSideMenuControl is FrameworkElement added && SideMenu.Children.Contains(added))
            {
                added.DataContext = this;
                return;
            }

            // Store previous children so they can be restored
            _previousSideMenuChildren = SideMenu.Children.Cast<UIElement>().ToArray();

            SideMenu.Children.Clear();

            FarmerSideMenuUC control = new FarmerSideMenuUC();
            control.DataContext = this;
            _addedSideMenuControl = control;

            SideMenu.Children.Add(control);
        }
        catch
        {
            // Ignore errors during UI attach (useful for test environments)
        }

    }
    public virtual void RestoreMainWindowSideMenu()
    {
        // If the main window's XamlRoot isn't available, the window is closed or not ready.
        if (App.MainWindowXamlRoot == null)
            return;

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

            if (_previousSideMenuChildren == null)
                return;

            SideMenu.Children.Clear();
            foreach (UIElement child in _previousSideMenuChildren)
                SideMenu.Children.Add(child);

            _previousSideMenuChildren = null;
            _addedSideMenuControl = null;
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // Window already closed — ignore restore
        }
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

    #region OnChooseUser Command
    /// <summary>
    /// Chooses a user and loads their dashboard.
    /// </summary>
    /// <param name="person">The person to select.</param>
    protected void ChooseUser(Person? person)
    {
        if (person == null)
        {
            return;
        }

        SelectedPerson = person;
        //LoadDashboard();
    }

#endregion
}
