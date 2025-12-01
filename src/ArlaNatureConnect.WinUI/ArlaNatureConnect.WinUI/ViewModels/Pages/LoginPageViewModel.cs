using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Pages;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for LoginPage - handles role selection and navigation to different dashboards.
/// 
/// Responsibilities:
/// - Receives role selection from UI (Farmer, Consultant, Arla Employee)
/// - Creates Role object based on the selection
/// - Navigates to the correct Page (FarmerPage, ConsultantPage, or ArlaEmployeePage)
/// - Passes the Role object to the next Page via NavigationHandler
/// 
/// Usage:
/// This ViewModel is used by LoginPage.xaml to handle the user's role selection.
/// When the user clicks a role button, the role name is sent as a parameter to SelectRoleCommand,
/// which then navigates to the correct Page with the selected Role object.
/// </summary>
public class LoginPageViewModel : NavigationViewModelBase
{
    #region Fields

    private readonly INavigationHandler _navigationHandler;

    #endregion

    #region Commands

    /// <summary>
    /// Command to select a role and navigate to the appropriate page.
    /// Receives role name as string parameter (e.g., "Farmer", "Consultant", "ArlaEmployee").
    /// </summary>
    public RelayCommand<string> SelectRoleCommand { get; }

    #endregion

    #region Properties

    /// <summary>
    /// The currently selected role.
    /// </summary>
    public Role? SelectedRole
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor

    public LoginPageViewModel(INavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        SelectRoleCommand = new RelayCommand<string>(SelectRole);
    }

    #endregion

    #region OnSelectRole Command

    /// <summary>
    /// Selects a role and navigates to the appropriate page.
    /// </summary>
    /// <param name="roleName">The name of the role to select (Farmer, Consultant, or ArlaEmployee).</param>
    private void SelectRole(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return;
        }

        // Navigate based on role
        switch (roleName.ToLowerInvariant())
        {
            case "farmer":
            case "landmand":
                // Create a role object for the selected role
                SelectedRole = new Role { Name = roleName };
                _navigationHandler.Navigate(typeof(FarmerPage), SelectedRole);
                break;

            case "consultant":
            case "konsulent":
                // Create a role object for the selected role
                SelectedRole = new Role { Name = roleName };
                _navigationHandler.Navigate(typeof(ConsultantPage), SelectedRole);
                break;

            case "arlaemployee":
            case "arla medarbejder":
            case "arlamedarbejder":
                // Create a role object for the selected role
                SelectedRole = new Role { Name = roleName };
                _navigationHandler.Navigate(typeof(ArlaEmployeePage), SelectedRole);
                break;

            case "administrator":
                // Create a role object for the selected role
                SelectedRole = new Role { Name = roleName };
                _navigationHandler.Navigate(typeof(AdministratorPage), SelectedRole);
                break;

            default:
                // Unknown role, do nothing - don't set SelectedRole
                break;
        }
    }

    #endregion
}
