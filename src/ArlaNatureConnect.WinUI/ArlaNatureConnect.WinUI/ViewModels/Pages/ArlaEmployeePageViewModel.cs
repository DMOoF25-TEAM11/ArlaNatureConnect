using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.ArlaEmployee;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// Hosts navigation for Arla employees and wires the assign-nature-check workflow into the shared page shell.
/// </summary>
public sealed class ArlaEmployeePageViewModel : NavigationViewModelBase
{
    public ArlaEmployeeAssignNatureCheckViewModel AssignNatureCheckViewModel { get; }

    public ArlaEmployeePageViewModel(
        INavigationHandler navigationHandler,
        ArlaEmployeeAssignNatureCheckViewModel assignNatureCheckViewModel)
        : base(navigationHandler)
    {
        AssignNatureCheckViewModel = assignNatureCheckViewModel ?? throw new ArgumentNullException(nameof(assignNatureCheckViewModel));

        SideMenuControlType = typeof(ArlaEmployeePageSideMenuUC);
        SideMenuViewModelType = typeof(ArlaEmployeePageSideMenuUCViewModel);

        ChooseUserCommand = new RelayCommand<Person>(OnEmployeeSelected);

        // Set default content to Farms view
        NavigateToView(new Func<UserControl?>(() =>
        {
            UserControl control = new ArlaEmployeeAssignNatureCheck();
            control.DataContext = AssignNatureCheckViewModel;
            return control;
        }));
    }

    public override async Task InitializeAsync(Role? role)
    {
        await AssignNatureCheckViewModel.InitializeAsync();
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
                        ? AssignNatureCheckViewModel
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
        AssignNatureCheckViewModel.AssignedByPersonId = person?.Id;
    }
}
