using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Consultant;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// NatureAreaCoordinate consultant navigation and listens to side-menu user selections.
/// </summary>
public sealed class ConsultantPageViewModel : NavigationViewModelBase
{
    private readonly INatureCheckCaseService? _natureCheckCaseService;

    public ConsultantNatureCheckViewModel? NatureCheckViewModel { get; }

    public Person? SelectedConsultant
    {
        get; private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();

            // Update notification view model when consultant changes
            if (NatureCheckViewModel != null)
            {
                NatureCheckViewModel.SelectedConsultant = value;
            }
        }
    }

    public string CurrentSection
    {
        get; private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = "Dashboards";

    public ConsultantPageViewModel(NavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        SideMenuControlType = typeof(ConsultantPageSideMenuUC);
        SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);

        NavigationCommand = new RelayCommand<object>(OnNavigate, CanNavigate);
        ChooseUserCommand = new RelayCommand<Person>(OnConsultantSelected);

        SetContent("Dashboards");
    }

    public ConsultantPageViewModel(INavigationHandler navigationHandler, INatureCheckCaseService natureCheckCaseService)
        : base(navigationHandler)
    {
        _natureCheckCaseService = natureCheckCaseService ?? throw new ArgumentNullException(nameof(natureCheckCaseService));
        SideMenuControlType = typeof(ConsultantPageSideMenuUC);
        SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);

        // Initialize nature check view model
        NatureCheckViewModel = new ConsultantNatureCheckViewModel(_natureCheckCaseService);

        NavigationCommand = new RelayCommand<object>(OnNavigate, CanNavigate);
        ChooseUserCommand = new RelayCommand<Person>(OnConsultantSelected);

        SetContent("Dashboards");
    }

    public override Task InitializeAsync(Role? role) => Task.CompletedTask;

    private bool CanNavigate(object? parameter) => parameter is string s && !string.IsNullOrWhiteSpace(s);

    private void OnNavigate(object? parameter)
    {
        if (parameter is not string tag)
        {
            return;
        }

        SetContent(tag);
    }

    private void SetContent(string tag)
    {
        CurrentSection = tag;

        UserControl? newContent = tag switch
        {
            "Dashboards" => new ConsultantDashboards(),
            "FarmsWhoHaveNatureArea" => new ConsultantNatureCheck(),
            "Tasks" => new ConsultantTasks(),
            _ => new ConsultantDashboards(),
        };

        if (newContent == null)
        {
            return;
        }

        // Use dedicated ViewModel for ConsultantNatureCheck if available
        if (newContent is ConsultantNatureCheck && NatureCheckViewModel != null)
        {
            newContent.DataContext = NatureCheckViewModel;
            // Ensure notifications are loaded when switching to this view
            _ = NatureCheckViewModel.LoadNotificationsAsync();
        }
        else
        {
            newContent.DataContext = this;
        }

        CurrentContent = newContent;
    }

    private void OnConsultantSelected(Person? person)
    {
        SelectedConsultant = person;
        // Future: trigger dashboard refresh based on selected consultant.
    }
}
