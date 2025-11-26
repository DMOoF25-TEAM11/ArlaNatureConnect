using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu; // updated namespace to match other side menu controls

public sealed partial class ConsultantPageSideMenuUC : SideMenuBaseUC
{
    public ConsultantPageSideMenuUC()
    {
        InitializeComponent();
        Loaded += ConsultantPageSideMenuUC_Loaded;
        //DataContextChanged += ConsultantPageSideMenuUC_DataContextChanged;
    }

    private void ConsultantPageSideMenuUC_Loaded(object sender, RoutedEventArgs e) => UpdateButtonStyles();

    //private void ConsultantPageSideMenuUC_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    //{
    //    if (_previousViewModel != null)
    //        _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;

    //    if (args.NewValue is ViewModels.Pages.ConsultantPageViewModel viewModel)
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
    //    if (e.PropertyName == nameof(ViewModels.Pages.ConsultantPageViewModel.CurrentNavigationTag))
    //        UpdateButtonStyles();
    //}

    //private void UpdateButtonStyles()
    //{
    //    if (DataContext is not ViewModels.Pages.ConsultantPageViewModel viewModel) return;

    //    if (Application.Current.Resources.TryGetValue("ArlaNavButton", out object navStyle) &&
    //        Application.Current.Resources.TryGetValue("ArlaNavButtonActive", out object activeStyle))
    //    {
    //        Style? navStyleTyped = navStyle as Style;
    //        Style? activeStyleTyped = activeStyle as Style;

    //        DashboardsButton.Style = navStyleTyped;
    //        FarmsButton.Style = navStyleTyped;
    //        TasksButton.Style = navStyleTyped;

    //        switch (viewModel.CurrentNavigationTag)
    //        {
    //            case "Dashboards": DashboardsButton.Style = activeStyleTyped; break;
    //            case "Farms": FarmsButton.Style = activeStyleTyped; break;
    //            case "Tasks": TasksButton.Style = activeStyleTyped; break;
    //        }
    //    }
    //}
}

