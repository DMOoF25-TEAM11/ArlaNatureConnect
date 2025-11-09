using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace ArlaNatureConnect.WinUI.View.Pages.Consultant;

/// <summary>
/// UserControl for the Consultant sidebar navigation menu.
/// Handles user selection dropdown and navigation buttons.
/// </summary>
public sealed partial class ConsultantSidebar : UserControl
{
    private ViewModels.Pages.ConsultantPageViewModel? _previousViewModel;

    public ConsultantSidebar()
    {
        InitializeComponent();
        Loaded += ConsultantSidebar_Loaded;
        DataContextChanged += ConsultantSidebar_DataContextChanged;
    }

    private void ConsultantSidebar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    private void ConsultantSidebar_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from previous ViewModel if it exists
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Subscribe to CurrentNavigationTag property changes to update button styles
        if (args.NewValue is ViewModels.Pages.ConsultantPageViewModel viewModel)
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

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.Pages.ConsultantPageViewModel.CurrentNavigationTag))
        {
            UpdateButtonStyles();
        }
    }

    /// <summary>
    /// Updates the button styles based on the current navigation tag in the ViewModel.
    /// </summary>
    private void UpdateButtonStyles()
    {
        if (DataContext is not ViewModels.Pages.ConsultantPageViewModel viewModel)
        {
            return;
        }

        if (Application.Current.Resources.TryGetValue("ArlaNavButton", out var navStyle) &&
            Application.Current.Resources.TryGetValue("ArlaNavButtonActive", out var activeStyle))
        {
            var navStyleTyped = navStyle as Microsoft.UI.Xaml.Style;
            var activeStyleTyped = activeStyle as Microsoft.UI.Xaml.Style;

            // Reset all buttons to normal navigation style
            DashboardsButton.Style = navStyleTyped;
            FarmsButton.Style = navStyleTyped;
            TasksButton.Style = navStyleTyped;

            // Set active button based on CurrentNavigationTag
            switch (viewModel.CurrentNavigationTag)
            {
                case "Dashboards":
                    DashboardsButton.Style = activeStyleTyped;
                    break;
                case "Farms":
                    FarmsButton.Style = activeStyleTyped;
                    break;
                case "Tasks":
                    TasksButton.Style = activeStyleTyped;
                    break;
            }
        }
    }
}

