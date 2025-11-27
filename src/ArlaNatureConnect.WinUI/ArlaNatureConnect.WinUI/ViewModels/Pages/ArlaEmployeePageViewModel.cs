using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.ArlaEmployee;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// Hosts navigation for Arla employees and wires the assign-nature-check workflow into the shared page shell.
/// </summary>
public sealed class ArlaEmployeePageViewModel : NavigationViewModelBase
{
    private readonly ArlaEmployeeAssignNatureCheckViewModel _assignNatureCheckViewModel;

    public ArlaEmployeeAssignNatureCheckViewModel AssignNatureCheckViewModel => _assignNatureCheckViewModel;

    public ArlaEmployeePageViewModel(
        NavigationHandler navigationHandler,
        ArlaEmployeeAssignNatureCheckViewModel assignNatureCheckViewModel)
        : base(navigationHandler)
    {
        _assignNatureCheckViewModel = assignNatureCheckViewModel ?? throw new ArgumentNullException(nameof(assignNatureCheckViewModel));

        SideMenuControlType = typeof(ArlaEmployeePageSideMenuUC);
        SideMenuViewModelType = typeof(ArlaEmployeePageSideMenuUCViewModel);

        ChooseUserCommand = new RelayCommand<Person>(OnEmployeeSelected);

        // Set default content to Farms view
        NavigateToView(new Func<UserControl?>(() =>
        {
            UserControl control = new ArlaEmployeeAssignNatureCheck();
            control.DataContext = _assignNatureCheckViewModel;
            return control;
        }));
    }

    public override async Task InitializeAsync(Role? role)
    {
        await _assignNatureCheckViewModel.InitializeAsync();
    }

    /// <summary>
    /// Override NavigateToView to handle DataContext assignment for Farms view.
    /// This is primarily used for default content in constructor, as side menu ViewModel handles navigation.
    /// </summary>
    protected override void NavigateToView(object? parameter)
    {
        if (parameter is Func<UserControl?> contentFunc)
        {
            try
            {
                UserControl? ctrl = contentFunc();
                if (ctrl != null)
                {
                    // Set DataContext to AssignNatureCheckViewModel for Farms view, otherwise use this
                    ctrl.DataContext = ctrl is ArlaEmployeeAssignNatureCheck
                        ? _assignNatureCheckViewModel
                        : this;
                    CurrentContent = ctrl;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("NavigateToView: contentFunc threw", ex);
            }
            return;
        }

        // Fall back to base implementation for other parameter types
        base.NavigateToView(parameter);
    }

    private void OnEmployeeSelected(Person? person)
    {
        _assignNatureCheckViewModel.AssignedByPersonId = person?.Id;
    }
}
