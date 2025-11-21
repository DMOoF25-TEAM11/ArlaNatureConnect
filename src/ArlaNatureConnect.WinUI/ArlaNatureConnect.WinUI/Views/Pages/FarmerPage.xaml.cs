using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
/// This page orchestrates the FarmerSideMenu and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class FarmerPage : Page
{
    public FarmerPageViewModel ViewModel { get; }

    // Keep track of previous SideMenu children so we can restore them when leaving the page
    private UIElement[]? _previousSideMenuChildren;
    private FarmerSideMenuUC? _addedSideMenuControl;

    public FarmerPage()
    {
        InitializeComponent();

        // Get dependencies from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();
        
        ViewModel = new FarmerPageViewModel(navigationHandler, personRepository, roleRepository);
        DataContext = ViewModel;

        // Bind the page's ContentPresenter to the ViewModel's CurrentContent property when it changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Subscribe to Loaded so we can attach the SideMenu once visual tree is ready
        Loaded += FarmerPage_Loaded;
        Unloaded += FarmerPage_Unloaded;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Role role)
        {
            await ViewModel.InitializeAsync(role);
        }
        
        // Set default view to Dashboards in ViewModel
        ViewModel.NavigationCommand?.Execute("Dashboards");

        // Attach SideMenu to main window's SideMenu area
        AttachSideMenuToMainWindow();
    }

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Restore previous SideMenu children when navigating away
        RestoreMainWindowSideMenu();
    }

    private void FarmerPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Ensure the content presenter shows the current ViewModel content
        if (ContentPresenter.Content == null && ViewModel.CurrentContent != null)
        {
            ContentPresenter.Content = ViewModel.CurrentContent;
        }

        // Also ensure SideMenu is attached in case OnNavigatedTo wasn't called
        AttachSideMenuToMainWindow();
    }

    private void FarmerPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // When page is unloaded (e.g. app shutdown), attempt to restore previous SideMenu
        RestoreMainWindowSideMenu();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FarmerPageViewModel.CurrentContent))
        {
            ContentPresenter.Content = ViewModel.CurrentContent;
        }
    }

    /// <summary>
    /// Finds the MainWindow's SideMenu panel and replaces its children with the FarmerSideMenuUC.
    /// Stores previous children so they can be restored later.
    /// Safe no-op when MainWindow or SideMenu cannot be found (e.g. during unit tests).
    /// </summary>
    private void AttachSideMenuToMainWindow()
    {
        try
        {
            MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
            if (mainWindow == null)
                return;

            if (mainWindow.Content is not FrameworkElement root)
                return;

            if (root.FindName("SideMenu") is not Panel SideMenu)
                return;

            // If we've already added our SideMenu control, ensure DataContext is synced and return
            if (_addedSideMenuControl != null && SideMenu.Children.Contains(_addedSideMenuControl))
            {
                _addedSideMenuControl.DataContext = this.DataContext;
                return;
            }

            // Store previous children so they can be restored
            _previousSideMenuChildren = SideMenu.Children.Cast<UIElement>().ToArray();

            SideMenu.Children.Clear();

            _addedSideMenuControl = new FarmerSideMenuUC();
            _addedSideMenuControl.DataContext = this.DataContext;

            SideMenu.Children.Add(_addedSideMenuControl);
        }
        catch
        {
            // Ignore errors during UI attach (useful for test environments)
        }
    }

    /// <summary>
    /// Restores the MainWindow's SideMenu to its previous children if available.
    /// </summary>
    private void RestoreMainWindowSideMenu()
    {
        try
        {
            MainWindow? mainWindow = App.HostInstance?.Services.GetService<MainWindow>();
            if (mainWindow == null)
                return;

            if (mainWindow.Content is not FrameworkElement root)
                return;

            if (root.FindName("SideMenu") is not Panel SideMenu)
                return;

            // Only restore if we previously replaced children
            if (_previousSideMenuChildren == null)
                return;

            SideMenu.Children.Clear();
            foreach (UIElement child in _previousSideMenuChildren)
            {
                SideMenu.Children.Add(child);
            }

            _previousSideMenuChildren = null;
            _addedSideMenuControl = null;
        }
        catch
        {
            // ignore
        }
    }
}

