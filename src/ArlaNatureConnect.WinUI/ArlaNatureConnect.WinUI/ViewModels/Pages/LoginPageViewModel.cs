using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for LoginPage - håndterer rollevalg og navigation til de forskellige dashboards.
/// 
/// Ansvar:
/// - Modtager rollevalg fra UI (Landmand, Konsulent, Arla Medarbejder)
/// - Opretter Role objekt baseret på valget
/// - Navigerer til den korrekte Page (FarmerPage, ConsultantPage, eller ArlaEmployeePage)
/// - Overfører Role objektet til den næste Page via NavigationHandler
/// 
/// Brug:
/// Denne ViewModel bruges af LoginPage.xaml til at håndtere brugerens rollevalg.
/// Når brugeren klikker på en rolle-knap, sendes rolle-navnet som parameter til SelectRoleCommand,
/// som derefter navigerer til den korrekte Page med det valgte Role objekt.
/// </summary>
public class LoginPageViewModel : ViewModelBase
{
    #region Fields

    private readonly NavigationHandler _navigationHandler;
    private Role? _selectedRole;

    #endregion

    #region Commands

    /// <summary>
    /// Command to select a role and navigate to the appropriate page.
    /// Modtager rolle-navn som string parameter (fx "Farmer", "Consultant", "ArlaEmployee").
    /// </summary>
    public RelayCommand<string> SelectRoleCommand { get; }

    #endregion

    #region Properties

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

    #endregion

    #region Constructor

    public LoginPageViewModel(NavigationHandler navigationHandler)
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

    #endregion
}
