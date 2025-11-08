using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for the LoginPage, handling role selection.
/// </summary>
public class LoginPageViewModel : ViewModelBase
{
    private readonly NavigationHandler _navigationHandler;
    private Role? _selectedRole;

    public LoginPageViewModel(NavigationHandler navigationHandler)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        SelectRoleCommand = new RelayCommand<string>(SelectRole);
    }

    /// <summary>
    /// Command to select a role and navigate to the appropriate page.
    /// </summary>
    public RelayCommand<string> SelectRoleCommand { get; }

    /// <summary>
    /// The currently selected role.
    /// </summary>
    public Role? SelectedRole
    {
        get => _selectedRole;
        private set
        {
            _selectedRole = value;
            OnPropertyChanged();
        }
    }

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

        // Create a role object for the selected role
        SelectedRole = new Role { Name = roleName };

        // Navigate based on role
        switch (roleName.ToLowerInvariant())
        {
            case "farmer":
            case "landmand":
                _navigationHandler.Navigate(typeof(ArlaNatureConnect.WinUI.View.Pages.FarmerPage), SelectedRole);
                break;

            case "consultant":
            case "konsulent":
                _navigationHandler.Navigate(typeof(ArlaNatureConnect.WinUI.View.Pages.ConsultantPage), SelectedRole);
                break;

            case "arlaemployee":
            case "arla medarbejder":
            case "arlamedarbejder":
                _navigationHandler.Navigate(typeof(ArlaNatureConnect.WinUI.View.Pages.ArlaEmployeePage), SelectedRole);
                break;

            default:
                // Unknown role, do nothing
                break;
        }
    }
}

