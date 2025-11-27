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
/// Coordinates consultant navigation and listens to side-menu user selections.
/// </summary>
public sealed class ConsultantPageViewModel : NavigationViewModelBase
{
    private readonly INatureCheckCaseService? _natureCheckCaseService;
    private Person? _selectedConsultant;
    private string _currentSection = "Dashboards";
    private ConsultantNatureCheckViewModel? _natureCheckViewModel;

    public ConsultantNatureCheckViewModel? NatureCheckViewModel => _natureCheckViewModel;

    public Person? SelectedConsultant
    {
        get => _selectedConsultant;
        private set
        {
            if (_selectedConsultant == value)
            {
                return;
            }

            _selectedConsultant = value;
            OnPropertyChanged();
            
            // Update notification view model when consultant changes
            if (_natureCheckViewModel != null)
            {
                _natureCheckViewModel.SelectedConsultant = value;
            }
        }
    }

    public string CurrentSection
    {
        get => _currentSection;
        private set
        {
            if (_currentSection == value)
            {
                return;
            }

            _currentSection = value;
            OnPropertyChanged();
        }
    }

    public ConsultantPageViewModel(NavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        SideMenuControlType = typeof(ConsultantPageSideMenuUC);
        SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);

        NavigationCommand = new RelayCommand<object>(OnNavigate, CanNavigate);
        ChooseUserCommand = new RelayCommand<Person>(OnConsultantSelected);

        SetContent("Dashboards");
    }

    public ConsultantPageViewModel(NavigationHandler navigationHandler, INatureCheckCaseService natureCheckCaseService)
        : base(navigationHandler)
    {
        _natureCheckCaseService = natureCheckCaseService ?? throw new ArgumentNullException(nameof(natureCheckCaseService));
        SideMenuControlType = typeof(ConsultantPageSideMenuUC);
        SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);

        // Initialize nature check view model
        _natureCheckViewModel = new ConsultantNatureCheckViewModel(_natureCheckCaseService);

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
            "Farms" => new ConsultantNatureCheck(),
            "Tasks" => new ConsultantTasks(),
            _ => new ConsultantDashboards(),
        };

        if (newContent == null)
        {
            return;
        }

        // Use dedicated ViewModel for ConsultantNatureCheck if available
        if (newContent is ConsultantNatureCheck && _natureCheckViewModel != null)
        {
            newContent.DataContext = _natureCheckViewModel;
            // Ensure notifications are loaded when switching to this view
            _ = _natureCheckViewModel.LoadNotificationsAsync();
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
