using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for the ArlaEmployeePage, displaying the employee dashboard directly.
/// </summary>
public class ArlaEmployeePageViewModel : ViewModelBase
{
    private readonly NavigationHandler _navigationHandler;
    private Role? _currentRole;

    public ArlaEmployeePageViewModel(NavigationHandler navigationHandler)
    {
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
    }

    /// <summary>
    /// Initializes the page with the selected role.
    /// </summary>
    /// <param name="role">The role that was selected (should be ArlaEmployee).</param>
    public void Initialize(Role? role)
    {
        _currentRole = role;
    }
}


