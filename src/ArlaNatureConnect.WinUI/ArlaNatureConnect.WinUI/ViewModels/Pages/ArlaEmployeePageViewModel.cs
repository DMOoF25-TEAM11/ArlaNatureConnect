using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ArlaEmployeePage - håndterer dashboard-visning for Arla medarbejdere.
/// 
/// Ansvar:
/// - Modtager Role objekt fra LoginPage via Initialize()
/// - Gemmer den valgte rolle for fremtidig brug
/// - Forbereder dashboard-visning for Arla medarbejdere
/// 
/// Brug:
/// Denne ViewModel bruges af ArlaEmployeePage.xaml til at vise dashboardet for Arla medarbejdere.
/// I modsætning til FarmerPage og ConsultantPage, behøver Arla medarbejdere ikke vælge en specifik
/// bruger, da de har direkte adgang til dashboardet. ViewModel'en er derfor simplere og håndterer
/// kun initialisering med rollen.
/// </summary>
public class ArlaEmployeePageViewModel : ViewModelBase
{
    #region Fields

    private readonly NavigationHandler _navigationHandler;
    private Role? _currentRole;

    #endregion

    #region Constructor

    public ArlaEmployeePageViewModel(NavigationHandler navigationHandler)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
    }

    #endregion

    #region Load Handler

    /// <summary>
    /// Initializes the page with the selected role.
    /// </summary>
    /// <param name="role">The role that was selected (should be ArlaEmployee).</param>
    public void Initialize(Role? role)
    {
        _currentRole = role;
    }

    #endregion
}
