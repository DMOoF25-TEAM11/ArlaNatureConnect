using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;

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
///
/// How to use:
/// - Derive from this class for role-based pages and call <see cref="InitializeNavigation(string)"/> from the derived
///   constructor to set up navigation command handling.
/// - Optionally assign <see cref="ContentFactory"/> to provide custom content creation logic for navigation tags.
/// - Call <see cref="AttachSideMenuToMainWindow"/> (or use <see cref="AttachToView(Page?)"/>) to attach the shared side menu.
/// </summary>
public abstract class NavigationViewModelBase : ViewModelBase, INavigationViewModelBase
{
    #region Fields
    private string _currentNavigationTag = string.Empty;

    // field to store last requested navigation tag (used by SwitchContentView and navigation flow)
    private string _navigationTag = string.Empty;

    /// <summary>
    /// Delegate that derived classes may provide to create the appropriate <see cref="UserControl"/> for a navigation tag.
    /// If null, the base implementation will create Farmer controls as a fallback.
    /// </summary>
    protected Func<string, UserControl?>? ContentFactory;

    private readonly NavigationHandler? _navigation_handler; // renamed internal field left for existing code compatibility if needed
    private readonly NavigationHandler? _navigationHandler; // original
    private bool _isLoading;
    private UserControl? _currentContent;

    // Side menu handling fields moved from view
    private UIElement[]? _previousSideMenuChildren;
    private UIElement? _addedSideMenuControl;
    private IServiceScope? _sideMenuScope; // NEW: scope for resolved side-menu VM

    // NEW: map of navigation tags to user-control factories
    private readonly Dictionary<string, Func<UserControl>> _contentFactories = new();
    // NEW: type of the side menu control to attach (set by derived VMs)
    protected Type? SideMenuControlType { get; set; }
    // Optional explicit factory if more advanced construction needed
    protected Func<NavigationViewModelBase, UserControl>? SideMenuFactory { get; set; }
    // NEW: explicit side menu view-model type to resolve via DI
    protected Type? SideMenuViewModelType { get; set; }
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
    protected NavigationViewModelBase(NavigationHandler navigationHandler) : base()
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
    }

    #region Properties
    /// <summary>
    /// Command to navigate between different content views.
    /// Accepts either a <see cref="string"/> tag or a delegate such as <see cref="Func{UserControl}"/>.
    /// Initialized via <see cref="InitializeNavigation"/>.
    /// </summary>
    public RelayCommand<object>? NavigationCommand { get; protected set; }

    /// <summary>
    /// The currently selected navigation tag used by the view to determine which content is active.
    /// Setting this property will also attempt to update <see cref="CurrentContent"/> by calling
    /// <see cref="SwitchContentView(string?)"/>.
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
                // store last requested tag and switch content for derived view-models
                _navigationTag = value ?? string.Empty;

                // Ensure content is synchronized with the tag and surface any failures so tests and callers
                // can detect and handle underlying factory/control creation errors.
                SwitchContentView(_navigationTag);
            }
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
    /// The current content <see cref="UserControl"/> for the page. Derived view-models may create and assign user controls here.
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
        _navigationTag = defaultTag ?? string.Empty;
        NavigationCommand = new RelayCommand<object>(NavigateToView, CanNavigate);
    }

    /// <summary>
    /// Default asynchronous initializer. Derived view-models should override this to perform role-specific initialization
    /// (for example call <see cref="LoadAvailableUsersAsync(string)"/>). The base implementation is a no-op completed task to
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
    /// - <see cref="Func{UserControl}"/>: invokes and uses returned control
    /// - <see cref="string"/>: sets <see cref="CurrentNavigationTag"/>
    /// - <see cref="Func{string}"/>: invokes and uses returned tag
    /// - <see cref="Func{Task{string}}"/>: invokes asynchronously and sets tag when completed
    /// - <see cref="Action"/>: invokes the delegate for arbitrary page-level actions
    /// - <see cref="Func{NavigationViewModelBase, string}"/>: invokes with this view-model
    /// - fallback: parameter.ToString()
    ///
    /// This method logs errors to <see cref="Debug"/> and attempts to preserve UI stability by avoiding throws
    /// originating from user-supplied delegates.
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
                    CurrentNavigationTag = tag; // will trigger SwitchContentView
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
                        // bubble details to debug only; do not interrupt UI
                        throw new Exception("NavigateToView: tagFunc threw", ex);
                    }
                    return;

                case Func<Task<string>> taskFunc:
                    try
                    {
                        Task<string> task = taskFunc();
                        if (task == null) return;

                        // ContinueWith ensures we don't block the UI thread; update tag when ready
                        task.ContinueWith(t =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion && !string.IsNullOrWhiteSpace(t.Result))
                            {
                                CurrentNavigationTag = t.Result;
                            }
                            else if (t.Exception != null)
                            {
                                throw new Exception("NavigateToView: taskFunc task failed", t.Exception);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("NavigateToView: taskFunc invocation failed", ex);
                    }
                    return;

                case Action action:
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("NavigateToView: action threw", ex);
                    }
                    return;

                case Func<UserControl?> contentFunc:
                    try
                    {
                        // Create the control and set it as current content
                        UserControl? ctrl = contentFunc();
                        if (ctrl != null)
                        {
                            ctrl.DataContext = this;
                            CurrentContent = ctrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("NavigateToView: contentFunc threw", ex);
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
                        throw new Exception("NavigateToView: vmTagFunc threw", ex);
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
                        throw new Exception("NavigateToView: fallback conversion failed", ex);
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("NavigateToView: unexpected error", ex);
        }

        // Ensure content is synchronized with the tag after handling navigation
        SwitchContentView(CurrentNavigationTag);
    }

    // NEW: registration API
    protected void RegisterContent(string tag, Func<UserControl> factory)
    {
        if (string.IsNullOrWhiteSpace(tag) || factory == null) return;
        _contentFactories[tag] = factory;
    }

    /// <summary>
    /// Switches the content view by creating an appropriate <see cref="UserControl"/> and assigning its DataContext.
    /// Default implementation creates Farmer page contents. Derived view-models may override to provide
    /// role-specific content controls or assign a custom <see cref="ContentFactory"/>.
    /// </summary>
    /// <param name="navigationTag">The tag to switch to. If null or empty the <see cref="CurrentNavigationTag"/> is used.</param>
    protected virtual void SwitchContentView(string? navigationTag)
    {
        try
        {
            string tag = string.IsNullOrWhiteSpace(navigationTag) ? CurrentNavigationTag : navigationTag!;

            UserControl? newContent = null;

            if (_contentFactories.TryGetValue(tag, out Func<UserControl>? factory))
            {
                try
                {
                    newContent = factory();
                }
                catch (Exception ex)
                {
                    throw new Exception("SwitchContentView: factory threw", ex);
                }
            }
            else if (ContentFactory != null)
            {
                try
                {
                    newContent = ContentFactory(tag);
                }
                catch (Exception ex)
                {
                    throw new Exception("SwitchContentView: ContentFactory threw", ex);
                }
            }

            if (newContent != null)
            {
                // Set the DataContext so the content control can bind to this view-model
                newContent.DataContext = this;
                CurrentContent = newContent;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("SwitchContentView failed", ex);
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
            if (mainWindow?.Content is not FrameworkElement root) return;
            Panel? sideMenuPanel = FindSideMenuPanel(root);
            if (sideMenuPanel == null) return;

            if (_addedSideMenuControl is FrameworkElement added && sideMenuPanel.Children.Contains(added))
            {
                // If control already added, ensure DataContext points to the current page VM
                //added.DataContext = this;
                return;
            }

            _previousSideMenuChildren = sideMenuPanel.Children.Cast<UIElement>().ToArray();
            sideMenuPanel.Children.Clear();

            UserControl control = SideMenuFactory != null ? SideMenuFactory(this) : (SideMenuControlType != null ? (UserControl)Activator.CreateInstance(SideMenuControlType) : throw new InvalidOperationException("No SideMenuControlType or SideMenuFactory available"));

            object? resolvedVm = null;

            // Prefer explicit type if provided; create a scope for scoped services so DI lifetime is correct
            if (SideMenuViewModelType != null && App.HostInstance != null)
            {
                try
                {
                    // dispose previous scope if any
                    _sideMenuScope?.Dispose();
                    _sideMenuScope = App.HostInstance.Services.CreateScope();
                    resolvedVm = _sideMenuScope.ServiceProvider.GetService(SideMenuViewModelType);
                }
                catch { resolvedVm = null; }
            }
            else
            {
                // Fallback convention: replace Views.Controls.SideMenu namespace with ViewModels.Controls.SideMenu and append ViewModel
                Type controlType = control.GetType();
                string? ns = controlType.Namespace;
                if (!string.IsNullOrWhiteSpace(ns) && ns.Contains("Views.Controls.SideMenu"))
                {
                    string vmNs = ns.Replace("Views.Controls.SideMenu", "ViewModels.Controls.SideMenu");
                    string vmFull = vmNs + "." + controlType.Name + "ViewModel";
                    Type? vmType = Type.GetType(vmFull);
                    if (vmType != null)
                    {
                        try
                        {
                            _sideMenuScope?.Dispose();
                            _sideMenuScope = App.HostInstance?.Services.CreateScope();
                            resolvedVm = _sideMenuScope?.ServiceProvider.GetService(vmType);
                        }
                        catch { resolvedVm = null; }
                    }
                }
            }

            if (resolvedVm != null)
            {
                control.DataContext = resolvedVm;
            }
            else
            {
                // fallback to host page VM so bindings still work
                control.DataContext = this;
            }

            _addedSideMenuControl = control;
            sideMenuPanel.Children.Add(control);
        }
        catch { }
    }

    /// <summary>
    /// Restore the previously stored main window side menu children. Intended to be invoked when navigating away
    /// from a page that temporarily replaced the side menu.
    /// </summary>
    public virtual void RestoreMainWindowSideMenu()
    {
        // If the main window's XamlRoot isn't available, the window is closed or not ready.
        if (App.MainWindowXamlRoot == null) return;

        try
        {
            MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
            if (mainWindow?.Content is not FrameworkElement root) return;
            Panel? sideMenuPanel = FindSideMenuPanel(root); if (sideMenuPanel == null) return;
            if (_previousSideMenuChildren == null) return;

            sideMenuPanel.Children.Clear();
            foreach (UIElement child in _previousSideMenuChildren) sideMenuPanel.Children.Add(child);

            _previousSideMenuChildren = null;

            // Dispose scope for resolved side-menu VM if created
            try { _sideMenuScope?.Dispose(); } catch { }
            _sideMenuScope = null;
            _addedSideMenuControl = null;
        }
        catch { }
    }

    private void Page_Loaded(object? sender, RoutedEventArgs e) => AttachSideMenuToMainWindow();
    private void Page_Unloaded(object? sender, RoutedEventArgs e)
    {
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
                if (child is Panel panel && panel.Name == "SideMenu") return panel;
                if (child is FrameworkElement fe)
                {
                    Panel? found = FindSideMenuPanel(fe);
                    if (found != null) return found;
                }
            }
        }
        catch { }

        return null;
    }

    #endregion
}
