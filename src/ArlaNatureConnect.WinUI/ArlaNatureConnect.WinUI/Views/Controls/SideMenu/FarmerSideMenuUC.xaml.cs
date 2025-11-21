using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Farmer sidebar navigation menu.
/// Handles user selection dropdown and navigation buttons.
/// </summary>
public sealed partial class FarmerSideMenuUC : UserControl
{
    private ViewModels.Pages.FarmerPageViewModel? _previousViewModel;

    public FarmerSideMenuUC()
    {
        InitializeComponent();
        // Subscribe to Loaded event to initialize button styles
        Loaded += FarmerSidebar_Loaded;
        // Subscribe to DataContextChanged event to handle ViewModel changes
        DataContextChanged += FarmerSidebar_DataContextChanged;
    }

    private void FarmerSidebar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    private void FarmerSidebar_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from previous ViewModel if it exists
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Subscribe to CurrentNavigationTag property changes to update button styles
        if (args.NewValue is ViewModels.Pages.FarmerPageViewModel viewModel)
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
        if (e.PropertyName == nameof(ViewModels.Pages.FarmerPageViewModel.CurrentNavigationTag))
        {
            UpdateButtonStyles();
        }
    }

    /// <summary>
    /// Updates the button styles based on the current navigation tag in the ViewModel.
    /// </summary>
    private void UpdateButtonStyles()
    {
        if (DataContext is not ViewModels.Pages.FarmerPageViewModel viewModel)
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

