using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Repositories;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

public class FarmerSideMenuUCViewModel : ListViewModelBase<PersonRepository, Person>
{
    #region Fields
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    // Parameterless constructor for XAML/design-time or frameworks that require it.
    // It forwards nulls to the parameterized ctor; adjust to provide real services/repository
    // when constructed via DI in production.
    public FarmerSideMenuUCViewModel() : this(null!, null!, null!)
    {
    }

    public FarmerSideMenuUCViewModel(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, PersonRepository repository) 
        : base(statusInfoServices, appMessageService, repository)
    {
        _navigationCommand = new RelayCommand<object>(OnNavigation);
    }

    private RelayCommand<object> _navigationCommand;
    public ICommand NavigationCommand => _navigationCommand ??= new RelayCommand<object>(param => OnNavigation(param));


    #region Properties
    #endregion
    #region Observables Properties
    #endregion
    #region Load handler
    #endregion
    #region Commands
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    private void OnNavigation(object? commandParameter)
    {
    }
    #endregion
    #region Helpers
    #endregion
}
