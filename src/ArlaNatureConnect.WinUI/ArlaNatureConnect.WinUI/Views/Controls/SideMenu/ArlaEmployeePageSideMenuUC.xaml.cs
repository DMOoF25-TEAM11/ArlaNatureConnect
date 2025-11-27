using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Arla Employee sidebar navigation menu.
/// Handles navigation buttons.
/// </summary>
public sealed partial class ArlaEmployeePageSideMenuUC : SideMenuBaseUC
{
    //private ViewModels.Pages.ArlaEmployeePageViewModel? _previousViewModel;

    public ArlaEmployeePageSideMenuUC()
    {
        InitializeComponent();
        Loaded += ArlaEmployeePageSideMenuUC_Loaded;
        //DataContextChanged += ArlaEmployeePageSideMenuUC_DataContextChanged;
    }

    private void ArlaEmployeePageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    //private void ArlaEmployeePageSideMenuUC_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    //{
    //    // Unsubscribe from previous ViewModel if it exists
    //    if (_previousViewModel != null)
    //    {
    //        _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    //    }

    //    // Subscribe to CurrentNavigationTag property changes to update button styles
    //    if (args.NewValue is ViewModels.Pages.ArlaEmployeePageViewModel viewModel)
    //    {
    //        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    //        _previousViewModel = viewModel;
    //        UpdateButtonStyles();
    //    }
    //    else
    //    {
    //        _previousViewModel = null;
    //    }
    //}

    //private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(ViewModels.Pages.ArlaEmployeePageViewModel.CurrentNavigationTag))
    //    {
    //        UpdateButtonStyles();
    //    }
    //}

    ///// <summary>
    ///// Updates the button styles based on the current navigation tag in the ViewModel.
    ///// </summary>
    //private void UpdateButtonStyles()
    //{
    //    if (DataContext is not ViewModels.Pages.ArlaEmployeePageViewModel viewModel)
    //    {
    //        return;
    //    }

    //    if (Application.Current.Resources.TryGetValue("ArlaNavButton", out object navStyle) &&
    //        Application.Current.Resources.TryGetValue("ArlaNavButtonActive", out object activeStyle))
    //    {
    //        Microsoft.UI.Xaml.Style? navStyleTyped = navStyle as Microsoft.UI.Xaml.Style;
    //        Microsoft.UI.Xaml.Style? activeStyleTyped = activeStyle as Microsoft.UI.Xaml.Style;

    //        // Reset all buttons to normal navigation style
    //        DashboardsButton.Style = navStyleTyped;
    //        FarmsButton.Style = navStyleTyped;
    //        UsersButton.Style = navStyleTyped;

    //        // Set active button based on CurrentNavigationTag
    //        switch (viewModel.CurrentNavigationTag)
    //        {
    //            case "Dashboards":
    //                DashboardsButton.Style = activeStyleTyped;
    //                break;
    //            case "Farms":
    //                FarmsButton.Style = activeStyleTyped;
    //                break;
    //            case "Users":
    //                UsersButton.Style = activeStyleTyped;
    //                break;
    //        }
    //    }
    //}
}

