using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ArlaEmployeePage - handles dashboard display for Arla employees.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via Initialize()
/// - Stores the selected role for future use
/// - Handles navigation between different sections (Dashboards, Farms, Users)
/// - Prepares dashboard display for Arla employees
/// 
/// Usage:
/// This ViewModel is used by ArlaEmployeePage.xaml to display the dashboard for Arla employees.
/// Unlike FarmerPage and ConsultantPage, Arla employees do not need to select a specific
/// user, as they have direct access to the dashboard. The ViewModel handles navigation between
/// different content views and initialization with the role.
/// </summary>
public class ArlaEmployeePageViewModel : NavigationViewModelBase
{
    #region Fields

    private readonly NavigationHandler? _navigationHandler;
    private Role? _currentRole;

    #endregion

    #region Properties

    #endregion

    #region Constructor

    public ArlaEmployeePageViewModel() : base()
    {
        
    }

    public ArlaEmployeePageViewModel(NavigationHandler navigationHandler) : base()
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        InitializeNavigation("Dashboards"); // Default to "Dashboards"
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
