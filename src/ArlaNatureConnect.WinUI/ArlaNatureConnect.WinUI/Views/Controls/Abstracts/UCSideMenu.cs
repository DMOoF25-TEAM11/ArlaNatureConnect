using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using System.ComponentModel;
using System.Linq;

namespace ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

public abstract partial class UCSideMenu : UserControl
{
    private INotifyPropertyChanged? _previousViewModel;

    protected UCSideMenu()
    {
        // Subscribe to Loaded event to initialize button styles
        Loaded += Sidebar_Loaded;
        // Subscribe to DataContextChanged event to handle ViewModel changes
        DataContextChanged += Sidebar_DataContextChanged;
    }

    private void Sidebar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

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

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Use the navigation property name from the interface to avoid depending on concrete VM types
        if (e.PropertyName == nameof(INavigationViewModel.CurrentNavigationTag))
        {
            UpdateButtonStyles();
        }
    }

    /// <summary>
    /// Returns the collection of navigation buttons as wrappers. Default implementation wraps
    /// the real Button instances found in the visual tree. Derived test classes can override
    /// this to provide test doubles without creating real WinUI Button objects.
    /// </summary>
    protected virtual IEnumerable<IButtonWrapper> GetNavigationButtonWrappers()
    {
        return GetNavigationButtons().Select(b => new ButtonWrapper(b));
    }

    /// <summary>
    /// Updates the button styles based on the current navigation tag in the ViewModel.
    /// Default implementation finds buttons in the visual tree and matches the button's CommandParameter
    /// or Tag against the current navigation tag. Derived controls can override GetNavigationButtons()
    /// if they want to provide a different set/order.
    /// </summary>
    private void UpdateButtonStyles()
    {
        if (DataContext is not INavigationViewModel viewModel)
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
    /// Returns the collection of navigation buttons for this control. Default implementation finds
    /// all Button controls in the visual tree rooted at this control. Override to provide a custom set/order.
    /// </summary>
    protected virtual IEnumerable<Button> GetNavigationButtons()
    {
        return FindDescendantButtons(this);
    }

    /// <summary>
    /// Maps a navigation tag to the corresponding Button wrapper. Default implementation checks each
    /// navigation button wrapper's CommandParameter and Tag for either a delegate matcher (Func<string,bool>/Predicate<string>)
    /// or a string value. Delegates are checked first to allow custom matching logic per button.
    /// </summary>
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
    /// Backwards-compatible adapter that returns the underlying Button for the matched wrapper.
    /// </summary>
    protected virtual Button? GetButtonForTag(string tag)
    {
        IButtonWrapper? wrapper = GetButtonWrapperForTag(tag);
        return wrapper is ButtonWrapper bw ? bw.UnderlyingButton : null;
    }

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
