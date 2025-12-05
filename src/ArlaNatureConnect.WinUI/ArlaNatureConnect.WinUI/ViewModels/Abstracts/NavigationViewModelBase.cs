using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using System.Diagnostics;
using System.Reflection;
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
///
/// Inheriting XML Documentation with <c>&lt;inheritdoc/&gt;</c>:
/// <para>
/// Use <c>&lt;inheritdoc/&gt;</c> on derived types or overridden members to inherit the documentation emitted by this
/// base class. This helps avoid duplication and keeps docs consistent when the behaviour is the same.
/// </para>
/// <para>Why use it:</para>
/// <list type="bullet">
///   <item><description>Keeps documentation DRY when derived classes implement the same behavior.</description></item>
///   <item><description>Ensures IntelliSense shows the same guidance for derived members without retyping comments.</description></item>
///   <item><description>Works well for large hierarchies where base behaviour is the canonical description.</description></item>
/// </list>
///
/// <example>
/// <code language="csharp">
/// // Base class already documents the behaviour
/// public abstract class NavigationViewModelBase
/// {
///     /// &lt;summary&gt;
///     /// Perform async initialization for the view-model.
///     /// &lt;/summary&gt;
///     public virtual Task InitializeAsync(Role? role)
///     {
///         return Task.CompletedTask;
///     }
/// }
///
/// // Derived class can inherit the documentation instead of repeating it
/// /// &lt;inheritdoc/&gt;
/// public class MyRoleViewModel : NavigationViewModelBase
/// {
///     /// &lt;inheritdoc/&gt;
///     public override Task InitializeAsync(Role? role)
///     {
///         // role-specific initialization here
///         return base.InitializeAsync(role);
///     }
/// }
/// </code>
/// </example>
///
/// <remarks>
/// When using <c>&lt;inheritdoc/&gt;</c> in a library project, ensure XML documentation file generation is enabled
/// for the build so the inherited comments are available to consuming projects and IntelliSense.
/// </remarks>
/// </summary>
public abstract class NavigationViewModelBase(INavigationHandler navigationHandler) : ViewModelBase, INavigationViewModelBase
{
    #region Fields
    // Keep the handler as the interface type. Casting to a concrete NavigationHandler
    // breaks tests that supply a mocked INavigationHandler (Castle proxy).
    private readonly INavigationHandler _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));

    // Side menu handling fields moved from view
    private UIElement[]? _previousSideMenuChildren;
    private UIElement? _addedSideMenuControl;
    private IServiceScope? _sideMenuScope;

    // NEW: type of the side menu control to attach (set by derived VMs)
    protected Type? SideMenuControlType { get; set; }
    // NEW: explicit side menu view-model type to resolve via DI
    protected Type? SideMenuViewModelType { get; set; }
    #endregion
    #region Fields Commands

    public RelayCommand<Person>? ChooseUserCommand { get; protected set; }
    #endregion
    #region Properties
    /// <summary>
    /// Command to navigate between different content views.
    /// Accepts either a <see cref="string"/> tag or a delegate such as <see cref="Func{UserControl}"/>.
    /// Initialized via <see cref="InitializeNavigation"/>.
    /// </summary>
    public RelayCommand<object>? NavigationCommand { get; protected set; }

    /// <summary>
    /// Indicates whether the view-model is performing an asynchronous load operation.
    /// The view can bind to this property to show a progress indicator.
    /// </summary>
    public bool IsLoading
    {
        get;
        protected set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The current content <see cref="UserControl"/> for the page. Derived view-models may create and assign user controls here.
    /// </summary>
    public UserControl? CurrentContent
    {
        get;
        protected set
        {
            if (field == value) return;
            field = value;
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
    //protected void InitializeNavigation(string defaultTag = "")
    //{
    //    _currentNavigationTag = defaultTag;
    //    _navigationTag = defaultTag ?? string.Empty;
    //    NavigationCommand = new RelayCommand<object>(NavigateToView, CanNavigate);
    //}

    /// <summary>
    /// Default asynchronous initializer. Derived view-models should override this to perform role-specific initialization
    /// (for example call <see cref="LoadAvailableUsersAsync(string)"/>). The base implementation is a no-op completed task to
    /// simplify callers that treat initialization as optional.
    /// </summary>
    /// <param name="role">Role used to scope initialization; may be <c>null</c>.</param>
    /// <returns>A completed task in the base implementation; derived classes return actual initialization tasks.</returns>
    public virtual Task InitializeAsync(Role? role)
    {
        // Inline comment: default implementation intentionally returns a completed task
        // so that callers can await InitializeAsync without checking for null/override.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines whether the navigation command can execute for the supplied parameter.
    /// Returns false for <c>null</c> or whitespace strings; delegates are permitted.
    /// </summary>
    /// <param name="parameter">Navigation parameter (string or delegate).</param>
    /// <returns><c>true</c> when navigation can proceed.</returns>
    private static bool CanNavigate(object? parameter)
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

                default:
                    throw new InvalidOperationException("NavigateToView: unsupported parameter type");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("NavigateToView: unexpected error", ex);
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

            //if (_addedSideMenuControl is FrameworkElement added && sideMenuPanel.Children.Contains(added))
            //{
            //    // If control already added, ensure DataContext points to the current page VM
            //    //added.DataContext = this;
            //    return;
            //}

            _previousSideMenuChildren = [.. sideMenuPanel.Children.Cast<UIElement>()];
            sideMenuPanel.Children.Clear();

            object? resolvedVm = null;
            UserControl? control = null;

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
                if (control != null)
                {
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
            }


            // 1) If explicit view type provided by VM, try to resolve or instantiate it
            if (SideMenuControlType != null && typeof(UserControl).IsAssignableFrom(SideMenuControlType))
            {
                control = App.HostInstance?.Services.GetService(SideMenuControlType) as UserControl
                          ?? Activator.CreateInstance(SideMenuControlType) as UserControl;
            }

            // 2) If VM type was resolved but no control yet, try convention-based view lookup
            if (control == null && resolvedVm != null)
            {
                Type vmType = resolvedVm.GetType();
                // Example convention: ViewModels.Controls.SideMenu.XxxViewModel -> Views.Controls.SideMenu.XxxUC
                string vmNs = vmType.Namespace ?? string.Empty;
                string viewNs = vmNs.Replace("ViewModels", "Views");
                string viewName = vmType.Name.Replace("ViewModel", "UC"); // adjust if project uses different suffix
                string fullViewType = $"{viewNs}.{viewName}";
                Type? viewType = Type.GetType(fullViewType);
                if (viewType != null && typeof(UserControl).IsAssignableFrom(viewType))
                {
                    control = App.HostInstance?.Services.GetService(viewType) as UserControl
                              ?? Activator.CreateInstance(viewType) as UserControl;
                }
            }

            // 3) Final safety: if still null, bail out instead of dereferencing
            if (control == null)
            {
                // log or handle the missing view; restore previous children
                if (_previousSideMenuChildren != null)
                {
                    foreach (UIElement child in _previousSideMenuChildren) sideMenuPanel.Children.Add(child);
                    _previousSideMenuChildren = null;
                }
                return;
            }

            // now safe to set DataContext and add the control
            if (resolvedVm != null)
            {
                // Preferred typed call if using the concrete base type
                if (resolvedVm is SideMenuViewModelBase sideVm)
                {
                    sideVm.SetHostPageViewModel(this);
                }
                else
                {
                    // Fallback: try to call a SetHostPageViewModel method if present
                    MethodInfo? method = resolvedVm.GetType().GetMethod("SetHostPageViewModel", [typeof(object)]);
                    method?.Invoke(resolvedVm, [this]);
                }
            }

            control.DataContext = resolvedVm ?? this;
            _addedSideMenuControl = control;
            sideMenuPanel.Children.Add(control);
        }
        catch { }
    }


    /// <summary>
    /// Restore the previously stored main window side menu children. Intended to be invoked when navigating away
    /// from a page that temporarily replaced the side menu.
    /// </summary>
    //public virtual void RestoreMainWindowSideMenu()
    //{
    //    // If the main window's XamlRoot isn't available, the window is closed or not ready.
    //    if (App.MainWindowXamlRoot == null) return;

    //    try
    //    {
    //        MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
    //        if (mainWindow?.Content is not FrameworkElement root) return;
    //        Panel? sideMenuPanel = FindSideMenuPanel(root); if (sideMenuPanel == null) return;
    //        if (_previousSideMenuChildren == null) return;

    //        sideMenuPanel.Children.Clear();
    //        foreach (UIElement child in _previousSideMenuChildren) sideMenuPanel.Children.Add(child);

    //        _previousSideMenuChildren = null;

    //        // Dispose scope for resolved side-menu VM if created
    //        try { _sideMenuScope?.Dispose(); } catch { }
    //        _sideMenuScope = null;
    //        _addedSideMenuControl = null;
    //    }
    //    catch { }
    //}

    private void Page_Loaded(object? sender, RoutedEventArgs e) => AttachSideMenuToMainWindow();
    private void Page_Unloaded(object? sender, RoutedEventArgs e)
    {
        //RestoreMainWindowSideMenu();
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
