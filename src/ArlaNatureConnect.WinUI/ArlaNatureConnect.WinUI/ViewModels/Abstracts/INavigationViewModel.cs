using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;

using Microsoft.UI.Xaml.Controls;

using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Interface for view models that support application navigation.
/// Exposes the navigation command and the currently selected navigation tag.
/// </summary>
public interface INavigationViewModel
{

    #region Commands
    RelayCommand<Person>? ChooseUserCommand { get; }
    ICommand? NavigationCommand { get; }
    #endregion
    #region Properties
    List<Person> AvailablePersons { get; }
    Person? SelectedPerson { get; }
    bool IsLoading { get; }
    bool IsUserSelected { get; }
    UserControl? CurrentContent { get; }
    #endregion

    /// <summary>
    /// The currently selected navigation tag.
    /// </summary>
    string CurrentNavigationTag { get; }

    #region SideMenu Handling
    void AttachToView(Page? page);
    void AttachSideMenuToMainWindow();
    void RestoreMainWindowSideMenu();
    #endregion
}
