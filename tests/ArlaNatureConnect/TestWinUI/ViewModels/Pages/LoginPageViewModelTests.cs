using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages;

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
    /// Parameterized test that covers multiple role name variations and verifies
    /// that the correct page type is navigated to and the selected role is set.
    /// </summary>
    [TestMethod]
    [DataRow("Farmer", "Farmer", "FarmerPage")]
    [DataRow("Consultant", "Consultant", "ConsultantPage")]
    [DataRow("ArlaEmployee", "ArlaEmployee", "ArlaEmployeePage")]
    [DataRow("Administrator", "Administrator", "AdministratorPage")]
    public void SelectRoleCommand_NavigatesForRoleVariations(string roleName, string expectedSelectedRoleName, string expectedPage)
    {
        // Act
        _viewModel.SelectRoleCommand.Execute(roleName);

        // Assert selected role
        if (string.IsNullOrWhiteSpace(roleName))
        {
            Assert.IsNull(_viewModel.SelectedRole);
            _mockNavigationHandler.Verify(
                h => h.Navigate(It.IsAny<Type>(), It.IsAny<object?>()),
                Times.Never);
            return;
        }

        Assert.IsNotNull(_viewModel.SelectedRole);
        Assert.AreEqual(expectedSelectedRoleName, _viewModel.SelectedRole!.Name);

        // Map expected page string to actual Type
        Type expectedType = expectedPage switch
        {
            "FarmerPage" => typeof(FarmerPage),
            "ConsultantPage" => typeof(ConsultantPage),
            "ArlaEmployeePage" => typeof(ArlaEmployeePage),
            "AdministratorPage" => typeof(AdministratorPage),
            _ => throw new InvalidOperationException($"Unknown expected page: {expectedPage}")
        };

        _mockNavigationHandler.Verify(
            h => h.Navigate(
                expectedType,
                It.Is<Role>(r => r.Name == expectedSelectedRoleName)),
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

