using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Pages;

/// <summary>
/// Unit tests for <see cref="LoginPageViewModel"/> covering role selection and navigation functionality.
/// 
/// These tests verify:
/// - Role selection creates correct Role objects
/// - Navigation is triggered with correct page types and parameters
/// - Different role name variations (English/Danish) are handled correctly
/// - Invalid or empty role names are handled gracefully
/// </summary>
[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class LoginPageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private LoginPageViewModel _viewModel = null!;

    /// <summary>
    /// Sets up test dependencies before each test method runs.
    /// Creates a mock NavigationHandler and initializes the ViewModel.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockNavigationHandler = new Mock<NavigationHandler>();
        _viewModel = new LoginPageViewModel(_mockNavigationHandler.Object);
    }

    /// <summary>
    /// Verifies that selecting "Farmer" role creates a Role object with correct name
    /// and triggers navigation to FarmerPage with the role as parameter.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithFarmerRole_SetsSelectedRoleAndNavigates()
    {
        // Arrange
        string roleName = "Farmer";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        Assert.AreEqual("Farmer", _viewModel.SelectedRole.Name);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.FarmerPage),
                It.Is<Role>(r => r.Name == "Farmer")),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "Landmand" (Danish for Farmer) role is handled correctly
    /// and navigates to FarmerPage, ensuring Danish role names are supported.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithLandmandRole_NavigatesToFarmerPage()
    {
        // Arrange
        string roleName = "Landmand";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        Assert.AreEqual("Landmand", _viewModel.SelectedRole.Name);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.FarmerPage),
                It.IsAny<Role>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "Consultant" role creates a Role object and navigates
    /// to ConsultantPage with the role as parameter.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithConsultantRole_SetsSelectedRoleAndNavigates()
    {
        // Arrange
        string roleName = "Consultant";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        Assert.AreEqual("Consultant", _viewModel.SelectedRole.Name);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.ConsultantPage),
                It.Is<Role>(r => r.Name == "Consultant")),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "Konsulent" (Danish for Consultant) role is handled correctly
    /// and navigates to ConsultantPage, ensuring Danish role names are supported.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithKonsulentRole_NavigatesToConsultantPage()
    {
        // Arrange
        string roleName = "Konsulent";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.ConsultantPage),
                It.IsAny<Role>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "ArlaEmployee" role creates a Role object and navigates
    /// to ArlaEmployeePage with the role as parameter.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithArlaEmployeeRole_SetsSelectedRoleAndNavigates()
    {
        // Arrange
        string roleName = "ArlaEmployee";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        Assert.AreEqual("ArlaEmployee", _viewModel.SelectedRole.Name);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.ArlaEmployeePage),
                It.Is<Role>(r => r.Name == "ArlaEmployee")),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "Arla Medarbejder" (Danish for Arla Employee) role is handled correctly
    /// and navigates to ArlaEmployeePage, ensuring Danish role names with spaces are supported.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithArlaMedarbejderRole_NavigatesToArlaEmployeePage()
    {
        // Arrange
        string roleName = "Arla Medarbejder";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.ArlaEmployeePage),
                It.IsAny<Role>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that selecting "ArlaMedarbejder" (Danish without space) role is handled correctly
    /// and navigates to ArlaEmployeePage, ensuring different Danish variations are supported.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithArlaMedarbejderNoSpaceRole_NavigatesToArlaEmployeePage()
    {
        // Arrange
        string roleName = "ArlaMedarbejder";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.ArlaEmployeePage),
                It.IsAny<Role>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that role selection is case-insensitive, so "farmer" (lowercase) works
    /// the same as "Farmer" (capitalized).
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithLowercaseRoleName_IsCaseInsensitive()
    {
        // Arrange
        string roleName = "farmer";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(
                typeof(ArlaNatureConnect.WinUI.View.Pages.FarmerPage),
                It.IsAny<Role>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that passing null as role name does not set SelectedRole and does not trigger navigation.
    /// This ensures the ViewModel handles null input gracefully without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithNullRoleName_DoesNotNavigate()
    {
        // Act
        _viewModel.SelectRoleCommand.Execute(null);

        // Assert
        Assert.IsNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(It.IsAny<Type>(), It.IsAny<object?>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that passing empty string as role name does not set SelectedRole and does not trigger navigation.
    /// This ensures the ViewModel handles empty input gracefully.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithEmptyRoleName_DoesNotNavigate()
    {
        // Act
        _viewModel.SelectRoleCommand.Execute(string.Empty);

        // Assert
        Assert.IsNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(It.IsAny<Type>(), It.IsAny<object?>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that passing whitespace-only string as role name does not set SelectedRole and does not trigger navigation.
    /// This ensures the ViewModel handles whitespace input gracefully.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithWhitespaceRoleName_DoesNotNavigate()
    {
        // Act
        _viewModel.SelectRoleCommand.Execute("   ");

        // Assert
        Assert.IsNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(It.IsAny<Type>(), It.IsAny<object?>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that passing an unknown role name does not set SelectedRole and does not trigger navigation.
    /// This ensures the ViewModel handles invalid role names gracefully without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void SelectRoleCommand_WithUnknownRoleName_DoesNotNavigate()
    {
        // Arrange
        string roleName = "UnknownRole";

        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert
        Assert.IsNull(_viewModel.SelectedRole);
        _mockNavigationHandler.Verify(
            h => h.Navigate(It.IsAny<Type>(), It.IsAny<object?>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that the ViewModel constructor throws ArgumentNullException when NavigationHandler is null.
    /// This ensures proper dependency injection validation.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullNavigationHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            new LoginPageViewModel(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }
}

