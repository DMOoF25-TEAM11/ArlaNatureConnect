using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;

using Microsoft.UI.Xaml.Controls;

using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Contract for view-models that drive role-aware navigation and side-menu integration in WinUI pages.
/// 
/// Why this interface exists:
/// - Provides a minimal surface that pages and controls can depend on without referencing concrete view-model types.
/// - Encapsulates navigation-related commands, observable properties and lifecycle hooks used by the application's
///   navigation pages (for example pages that display dashboards, lists and task content per user/role).
/// - Makes it easier to write unit tests and UI components that operate against an interface rather than concrete
///   implementations, enabling mocking and decoupling between views and view-model implementations.
/// </summary>
public interface INavigationViewModelBase
{

    #region Commands
    /// <summary>
    /// Command used to choose a <see cref="Person"/> (for example selecting a farmer or consultant from a list).
    /// The command receives a <see cref="Person"/> instance as its parameter and performs selection logic in the view-model.
    /// </summary>
    RelayCommand<Person>? ChooseUserCommand { get; }

    /// <summary>
    /// General navigation command used by the side menu/navigation UI.
    /// Accepts different parameter shapes: a <see cref="string"/> tag, a delegate producing a tag, or an <see cref="Action"/>.
    /// Invoking this command instructs the view-model to switch the current content view or perform a page-level action.
    /// </summary>
    ICommand? NavigationCommand { get; }
    #endregion
    #region Properties
    /// <summary>
    /// Indicates whether the view-model is currently performing an asynchronous operation (for example loading candidate users).
    /// The view can bind to this property to show progress indicators.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// The currently active content control for the page. Implementations often create simple UserControl instances and set their
    /// DataContext to the view-model so the page can host dynamic content in a content area.
    /// </summary>
    UserControl? CurrentContent { get; }
    #endregion

    /// <summary>
    /// Optional asynchronous initializer invoked when navigating to a page that hosts this view-model.
    /// Implementations should accept a <see cref="Role"/> describing the current user's role and perform any
    /// required asynchronous initialization (for example loading available users).
    /// </summary>
    /// <param name="role">The role used to scope initialization; may be <c>null</c> when no role is provided.</param>
    /// <returns>A task that completes when initialization is finished.</returns>
    Task InitializeAsync(Role? role);

    #region SideMenu Handling
    /// <summary>
    /// Attach the view-model to a page instance so the view-model can subscribe to lifecycle events (Loaded/Unloaded)
    /// and perform view-related wiring if necessary. Passing <c>null</c> should be a no-op.
    /// </summary>
    /// <param name="page">The page to attach to, or <c>null</c> to detach/no-op.</param>
    void AttachToView(Page? page);

    /// <summary>
    /// Attach the view-model's side-menu UI to the application's main window side menu area.
    /// Implementations typically replace the main window's side-menu children with a dedicated control bound to this view-model.
    /// </summary>
    void AttachSideMenuToMainWindow();

    #endregion
}
