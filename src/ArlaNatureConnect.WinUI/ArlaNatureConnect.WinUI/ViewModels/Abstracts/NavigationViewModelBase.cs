using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
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

/// <summary>
/// Base view-model that centralizes navigation and side-menu integration logic used by role-based pages.
///
/// Why this class exists:
/// - Encapsulates common navigation command handling and content switching so derived view-models (e.g. farmer/consultant)
///   can focus on role-specific data loading and business logic.
/// - Provides side-menu attach/restore helpers that move UI wiring out of views and into a testable view-model layer.
/// - Exposes observable properties (current tag, selected person, available persons, current content and loading flag)
///   that pages bind to for a consistent navigation/user-selection experience across the app.
/// </summary>
public abstract class NavigationViewModelBase : ViewModelBase, INavigationViewModelBase
{
    #region Fields
    private string _currentNavigationTag = string.Empty;

    private readonly NavigationHandler? _navigationHandler;
    private readonly IPersonRepository? _person_repository;
    private readonly IRoleRepository? _role_repository;
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

    /// <summary>
    /// Command used to choose a person from a list. Derived classes wire this command to selection behaviour.
    /// </summary>
    public RelayCommand<Person>? ChooseUserCommand { get; }
    #endregion

    /// <summary>
    /// Parameterless constructor used for design-time and tests. Derived classes that need repositories should use
    /// the constructor that accepts dependencies.
    /// </summary>
    public NavigationViewModelBase() : base()
    {
    }

    /// <summary>
    /// Construct a navigation view-model with required services.
    /// </summary>
    /// <param name="navigationHandler">Navigation handler used for frame navigation.</param>
    /// <param name="personRepository">Repository used to load persons for the active role.</param>
    /// <param name="roleRepository">Repository used to resolve roles if needed.</param>
    protected NavigationViewModelBase(NavigationHandler navigationHandler, IPersonRepository? personRepository, IRoleRepository? roleRepository) : base()
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        _person_repository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _role_repository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        ChooseUserCommand = new RelayCommand<Person>(ChooseUser, p => p != null);
    }

    #region Properties
    /// <summary>
    /// Command to navigate between different content views.
    /// Accepts either a string tag or a delegate (Func<string>/Action) as parameter.
    /// Initialized via <see cref="InitializeNavigation"/>.
    /// </summary>
    public RelayCommand<object>? NavigationCommand { get; protected set; }

    /// <summary>
    /// The currently selected navigation tag used by the view to determine which content is active.
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

    /// <summary>
    /// Persons available for selection for the current role (bind to UI control).
    /// </summary>
    public List<Person> AvailablePersons
    {
        get => _availablePersons;
        private set
        {
            _availablePersons = value ?? new List<Person>();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Currently selected person. Setting this updates dependent state and command availability.
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
    /// Indicates whether the view-model is performing an asynchronous load operation.
    /// The view can bind to this property to show a progress indicator.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        protected set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Convenience property indicating whether a person has been selected.
    /// </summary>
    public bool IsUserSelected => SelectedPerson != null;

    /// <summary>
    /// The current content UserControl for the page. Derived view-models may create and assign user controls here.
    /// </summary>
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
    #region Commands
    ICommand? INavigationViewModelBase.NavigationCommand => NavigationCommand;
    #endregion
    #region Helpers
    /// <summary>
    /// Initializes the navigation command and sets an initial tag. Call from derived class constructors.
    /// </summary>
    /// <param name="defaultTag">Default navigation tag (for example "Dashboards").</param>
    protected void InitializeNavigation(string defaultTag = "")
    {
        _currentNavigationTag = defaultTag;
        NavigationCommand = new RelayCommand<object>(NavigateToView, CanNavigate);
    }

    /// <summary>
    /// Default asynchronous initializer. Derived view-models should override this to perform role-specific initialization
    /// (for example call <see cref="LoadAvailableUsersAsync"/>). The base implementation is a no-op completed task to
    /// simplify callers that treat initialization as optional.
    /// </summary>
    /// <param name="role">Role used to scope initialization; may be <c>null</c>.</param>
    /// <returns>A completed task in the base implementation; derived classes return actual initialization tasks.</returns>
    public virtual Task InitializeAsync(Role? role)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines whether the navigation command can execute for the supplied parameter.
    /// Returns false for <c>null</c> or whitespace strings; delegates are permitted.
    /// </summary>
    /// <param name="parameter">Navigation parameter (string or delegate).</param>
    /// <returns><c>true</c> when navigation can proceed.</returns>
    private bool CanNavigate(object? parameter)
    {
        if (parameter == null) return false;
        if (parameter is string s) return !string.IsNullOrWhiteSpace(s);
        // allow any delegate
        if (parameter is Delegate) return true;
        return true;
    }

    /// <summary>
    /// Handles navigation requests. Supports a variety of parameter shapes:
    /// - string: sets <see cref="CurrentNavigationTag"/>
    /// - Func&lt;string&gt;: invokes and uses returned tag
    /// - Func&lt;Task&lt;string&gt;&gt;: invokes asynchronously and sets tag when completed
    /// - Action: invokes the delegate for arbitrary page-level actions
    /// - Func&lt;NavigationViewModelBase, string&gt;: invokes with this view-model
    /// - fallback: parameter.ToString()
    ///
    /// The method logs exceptions via <see cref="Debug.WriteLine"/> but does not throw to avoid breaking the UI.
    /// Derived classes may override to react to tag changes (for example switch content controls).
    /// </summary>
    /// <param name="parameter">The navigation parameter.</param>
    protected virtual void NavigateToView(object? parameter)
    {
        if (parameter == null)
            return;

        try
        {
            switch (parameter)
            {
                case string tag when !string.IsNullOrWhiteSpace(tag):
                    CurrentNavigationTag = tag;
                    return;

                case Func<string> tagFunc:
                    try
                    {
                        string result = tagFunc();
                        if (!string.IsNullOrWhiteSpace(result))
                            CurrentNavigationTag = result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NavigateToView: func threw: {ex}");
                    }
                    return;

                case Func<Task<string>> taskFunc:
                    try
                    {
                        Task<string> task = taskFunc();
                        if (task == null) return;

                        task.ContinueWith(t =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion && !string.IsNullOrWhiteSpace(t.Result))
                            {
                                CurrentNavigationTag = t.Result;
                            }
                            else if (t.Exception != null)
                            {
                                Debug.WriteLine($"NavigateToView: taskFunc faulted: {t.Exception}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NavigateToView: taskFunc invocation failed: {ex}");
                    }
                    return;

                case Action action:
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NavigateToView: action threw: {ex}");
                    }
                    return;

                case Func<NavigationViewModelBase, string> vmTagFunc:
                    try
                    {
                        string result = vmTagFunc(this);
                        if (!string.IsNullOrWhiteSpace(result))
                            CurrentNavigationTag = result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NavigateToView: vmTagFunc threw: {ex}");
                    }
                    return;

                default:
                    try
                    {
                        string? s = parameter?.ToString();
                        if (!string.IsNullOrWhiteSpace(s))
                            CurrentNavigationTag = s;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NavigateToView: fallback conversion failed: {ex}");
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NavigateToView: unexpected error: {ex}");
        }
    }

    /// <summary>
    /// Load persons for the specified role using the injected repository. Updates <see cref="AvailablePersons"/> and the
    /// <see cref="IsLoading"/> flag. Swallows exceptions and logs them to avoid leaving the UI in a loading state.
    /// </summary>
    /// <param name="roleName">The name of the role to load persons for.</param>
    /// <returns>A task that completes when loading finishes.</returns>
    protected async Task LoadAvailableUsersAsync(string roleName)
    {
        if (_person_repository == null)
            return;

        IsLoading = true;
        try
        {
            List<Person> persons = await _person_repository.GetPersonsByRoleAsync(roleName);
            AvailablePersons = persons ?? new List<Person>();
            Debug.WriteLine($"Loaded {AvailablePersons.Count} persons with role {roleName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadAvailableUsersAsync failed: {ex}");
            AvailablePersons = new List<Person>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
    #region SideMenu Handling
    /// <summary>
    /// Attach the view-model to the page so it can subscribe to page lifecycle events (Loaded/Unloaded).
    /// </summary>
    /// <param name="page">The page to attach to. If null the call is ignored.</param>
    public void AttachToView(Microsoft.UI.Xaml.Controls.Page? page)
    {
        if (page == null) return;

        // Subscribe to lifecycle events
        page.Loaded += Page_Loaded;
        page.Unloaded += Page_Unloaded;
    }

    /// <summary>
    /// Attach the view-model's side-menu control to the application's main window side-menu panel.
    /// Stores previous children so they can be restored later.
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
        catch (Exception ex)
        {
            Debug.WriteLine($"AttachSideMenuToMainWindow failed: {ex}");
        }

    }

    /// <summary>
    /// Restore the previously stored main window side menu children. Intended to be invoked when navigating away
    /// from a page that temporarily replaced the side menu.
    /// </summary>
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
        catch (Exception ex)
        {
            Debug.WriteLine($"RestoreMainWindowSideMenu failed: {ex}");
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

    /// <summary>
    /// Finds the main window's side-menu panel by name or by traversing the visual tree.
    /// Returns null if no panel named "SideMenu" is found.
    /// </summary>
    /// <param name="root">Root element to start the search from.</param>
    /// <returns>The side-menu <see cref="Panel"/> or <c>null</c> if not found.</returns>
    private static Panel? FindSideMenuPanel(FrameworkElement root)
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
        catch (Exception ex)
        {
            Debug.WriteLine($"FindSideMenuPanel traversal failed: {ex}");
        }

        return null;
    }

    #endregion
    #region OnChooseUser Command
    /// <summary>
    /// Selects the provided person and updates related state. Derived classes may react to selection by showing a dashboard.
    /// </summary>
    /// <param name="person">The person to select; if null the call is ignored.</param>
    protected void ChooseUser(Person? person)
    {
        if (person == null)
        {
            return;
        }

        SelectedPerson = person;
        // LoadDashboard(); // Derived classes can implement dashboard loading when selection changes
    }

    #endregion
}
