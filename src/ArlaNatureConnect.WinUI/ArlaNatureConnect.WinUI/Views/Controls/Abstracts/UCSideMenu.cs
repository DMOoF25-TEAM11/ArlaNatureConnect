using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using System.ComponentModel;

namespace ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

/// <summary>
/// A base user control that represents a side navigation menu used across pages.
/// </summary>
/// <remarks>
/// This abstract control centralizes the behavior for a navigation sidebar that
/// contains multiple navigation buttons. It decouples UI update logic (such as
/// applying active/inactive styles based on the current navigation tag) from
/// concrete view implementations and view models. The control listens to the
/// <see cref="INotifyPropertyChanged"/> events on the DataContext when it
/// implements <see cref="INavigationViewModelBase"/> and updates the visual
/// state accordingly. Provide derived classes (or override methods) to customize
/// how buttons are discovered or wrapped for testing.
/// </remarks>
public abstract partial class UCSideMenu : UserControl
{
    private INotifyPropertyChanged? _previousViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="UCSideMenu"/> class and
    /// hooks up framework events needed to initialize and react to DataContext
    /// changes.
    /// </summary>
    public UCSideMenu()
    {
        // Subscribe to Loaded event to initialize button styles
        Loaded += Sidebar_Loaded;
        // Subscribe to DataContextChanged event to handle ViewModel changes
        DataContextChanged += Sidebar_DataContextChanged;
    }

    /// <summary>
    /// Handles the control's <see cref="FrameworkElement.Loaded"/> event and
    /// triggers an initial styling pass for navigation buttons.
    /// </summary>
    /// <param name="sender">The sender of the event (the control).</param>
    /// <param name="e">Event arguments for the loaded event.</param>
    private void Sidebar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    /// <summary>
    /// Handles DataContext changes for the control and wires/unwires
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> subscriptions on the
    /// view model so the control can react to changes in the
    /// <see cref="INavigationViewModelBase.CurrentNavigationTag"/> property.
    /// </summary>
    /// <param name="sender">The element whose DataContext changed.</param>
    /// <param name="args">The data context change arguments.</param>
    private void Sidebar_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from previous ViewModel if it exists
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Subscribe to CurrentNavigationTag property changes to update button styles
        if (args?.NewValue is INotifyPropertyChanged viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _previousViewModel = viewModel;
            UpdateButtonStyles();
        }
        else
        {
            _previousViewModel = null;
        }
    }

    /// <summary>
    /// Handles <see cref="INotifyPropertyChanged.PropertyChanged"/> events raised
    /// by the current DataContext (when it implements
    /// <see cref="INavigationViewModelBase"/>). If the current navigation tag
    /// changes the method triggers a refresh of the button styles.
    /// </summary>
    /// <param name="sender">The object that raised the event (the view model).</param>
    /// <param name="e">Property change event arguments.</param>
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Use the navigation property name from the interface to avoid depending on concrete VM types
        if (e.PropertyName == nameof(INavigationViewModelBase.CurrentNavigationTag))
        {
            UpdateButtonStyles();
        }
    }

    /// <summary>
    /// Returns a collection of <see cref="IButtonWrapper"/> instances that
    /// represent the navigation buttons for this control. The default
    /// implementation wraps real <see cref="Button"/> instances discovered in
    /// the visual tree. Override this in tests or derived classes to provide
    /// alternate wrappers (for example, test doubles) without creating UI
    /// elements.
    /// </summary>
    /// <returns>An enumerable of <see cref="IButtonWrapper"/>.</returns>
    protected virtual IEnumerable<IButtonWrapper> GetNavigationButtonWrappers()
    {
        return GetNavigationButtons().Select(b => new ButtonWrapper(b));
    }

    /// <summary>
    /// Updates the styles of the navigation buttons according to the value of
    /// <see cref="INavigationViewModelBase.CurrentNavigationTag"/> on the
    /// DataContext. The method looks up two resource styles: "ArlaNavButton"
    /// for the default state and "ArlaNavButtonActive" for the active state.
    /// </summary>
    /// <remarks>
    /// The default algorithm resets all navigation buttons to the normal style
    /// and then applies the active style to the button whose CommandParameter
    /// or Tag matches the current navigation tag (matching supports string
    /// equality and delegates such as <c>Func&lt;string,bool&gt;</c> or
    /// <c>Predicate&lt;string&gt;</c> stored on the button).
    /// </remarks>
    private void UpdateButtonStyles()
    {
        if (DataContext is not INavigationViewModelBase viewModel)
        {
            return;
        }

        if (Application.Current.Resources.TryGetValue("ArlaNavButton", out object navStyle) &&
            Application.Current.Resources.TryGetValue("ArlaNavButtonActive", out object activeStyle))
        {
            Style? navStyleTyped = navStyle as Microsoft.UI.Xaml.Style;
            Style? activeStyleTyped = activeStyle as Microsoft.UI.Xaml.Style;

            // Reset navigation buttons to normal style
            foreach (IButtonWrapper wrapper in GetNavigationButtonWrappers())
            {
                if (wrapper is ButtonWrapper bw && bw.UnderlyingButton != null)
                {
                    bw.UnderlyingButton.Style = navStyleTyped;
                }
            }

            // Find the active button for the current tag and apply the active style
            IButtonWrapper? activeWrapper = GetButtonWrapperForTag(viewModel.CurrentNavigationTag ?? string.Empty);
            if (activeWrapper is ButtonWrapper activeBw && activeBw.UnderlyingButton != null)
            {
                activeBw.UnderlyingButton.Style = activeStyleTyped;
            }
        }
    }

    /// <summary>
    /// Returns the collection of actual <see cref="Button"/> instances used as
    /// navigation buttons. The default implementation searches the visual
    /// subtree rooted at this control. Override to provide a custom set or a
    /// specific ordering of buttons.
    /// </summary>
    /// <returns>An enumerable of <see cref="Button"/> controls.</returns>
    protected virtual IEnumerable<Button> GetNavigationButtons()
    {
        return FindDescendantButtons(this);
    }

    /// <summary>
    /// Maps the provided navigation tag to a matching <see cref="IButtonWrapper"/>.
    /// Matching follows this precedence:
    /// 1. A <c>Func&lt;string,bool&gt;</c> stored in <see cref="IButtonWrapper.CommandParameter"/>.
    /// 2. A <c>Predicate&lt;string&gt;</c> stored in <see cref="IButtonWrapper.CommandParameter"/>.
    /// 3. A <c>Func&lt;string,bool&gt;</c> stored in <see cref="IButtonWrapper.Tag"/>.
    /// 4. A <c>Predicate&lt;string&gt;</c> stored in <see cref="IButtonWrapper.Tag"/>.
    /// 5. String equality against the <see cref="IButtonWrapper.CommandParameter"/>.
    /// 6. String equality against the <see cref="IButtonWrapper.Tag"/>.
    /// </summary>
    /// <param name="tag">The navigation tag to match.</param>
    /// <returns>The matching <see cref="IButtonWrapper"/>, or <c>null</c> if none match.</returns>
    protected virtual IButtonWrapper? GetButtonWrapperForTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return null;

        foreach (IButtonWrapper wrapper in GetNavigationButtonWrappers())
        {
            if (wrapper == null) continue;

            try
            {
                if (wrapper.CommandParameter is Func<string, bool> cpFunc && cpFunc(tag))
                    return wrapper;
            }
            catch { }

            try
            {
                if (wrapper.CommandParameter is Predicate<string> cpPred && cpPred(tag))
                    return wrapper;
            }
            catch { }

            try
            {
                if (wrapper.Tag is Func<string, bool> tagFunc && tagFunc(tag))
                    return wrapper;
            }
            catch { }

            try
            {
                if (wrapper.Tag is Predicate<string> tagPred && tagPred(tag))
                    return wrapper;
            }
            catch { }

            if (wrapper.CommandParameter != null && string.Equals(wrapper.CommandParameter?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                return wrapper;

            if (wrapper.Tag != null && string.Equals(wrapper.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                return wrapper;
        }

        return null;
    }

    /// <summary>
    /// Backwards-compatible adapter that returns the underlying <see cref="Button"/>
    /// for the matched wrapper (if it is a <c>ButtonWrapper</c>), or <c>null</c>.
    /// </summary>
    /// <param name="tag">The navigation tag to match.</param>
    /// <returns>The matched <see cref="Button"/>, or <c>null</c> if none match.</returns>
    protected virtual Button? GetButtonForTag(string tag)
    {
        IButtonWrapper? wrapper = GetButtonWrapperForTag(tag);
        return wrapper is ButtonWrapper bw ? bw.UnderlyingButton : null;
    }

    /// <summary>
    /// Traverses the visual tree rooted at <paramref name="root"/> and yields
    /// every <see cref="Button"/> descendant. This is implemented as a
    /// breadth-first search to avoid deep recursion on complex visual trees.
    /// </summary>
    /// <param name="root">Root element to search for descendant buttons.</param>
    /// <returns>An enumerable of descendant <see cref="Button"/> instances.</returns>
    private static IEnumerable<Button> FindDescendantButtons(DependencyObject root)
    {
        if (root == null)
            yield break;

        Queue<DependencyObject> queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            DependencyObject current = queue.Dequeue();
            int count = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(current, i);
                if (child is Button btn)
                    yield return btn;

                if (child != null)
                    queue.Enqueue(child);
            }
        }
    }
}
