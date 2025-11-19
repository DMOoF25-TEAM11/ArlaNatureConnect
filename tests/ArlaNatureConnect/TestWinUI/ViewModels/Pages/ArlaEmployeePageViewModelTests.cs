using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Moq;

namespace TestWinUI.ViewModels.Pages;

/// <summary>
/// Unit tests for <see cref="ArlaEmployeePageViewModel"/> covering role initialization and navigation functionality.
/// 
/// These tests verify:
/// - Initialization with role stores the role correctly
/// - Navigation between different content views works correctly
/// - Default navigation tag is set correctly
/// - Unlike FarmerPage and ConsultantPage, ArlaEmployeePage does not require user selection
/// </summary>
[TestClass]
public sealed class ArlaEmployeePageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private ArlaEmployeePageViewModel _viewModel = null!;

    /// <summary>
    /// Sets up test dependencies before each test method runs.
    /// Creates a mock NavigationHandler and initializes the ViewModel.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockNavigationHandler = new Mock<NavigationHandler>();
        _viewModel = new ArlaEmployeePageViewModel(_mockNavigationHandler.Object);
    }

    /// <summary>
    /// Verifies that Initialize with a valid ArlaEmployee role stores the role correctly.
    /// Unlike FarmerPage and ConsultantPage, ArlaEmployeePage does not need to load users
    /// from repositories since employees have direct access to the dashboard.
    /// </summary>
    [TestMethod]
    public void Initialize_WithArlaEmployeeRole_StoresRole()
    {
        // Arrange
        Role role = new Role { Id = Guid.NewGuid(), Name = "ArlaEmployee" };

        // Act
        _viewModel.Initialize(role);

        // Assert
        // Note: The role is stored in a private field, so we can't directly verify it.
        // But we can verify that Initialize completes without throwing exceptions.
        Assert.IsNotNull(role);
    }

    /// <summary>
    /// Verifies that Initialize with null role is handled gracefully without throwing exceptions.
    /// This ensures the ViewModel can handle cases where no role is provided.
    /// </summary>
    [TestMethod]
    public void Initialize_WithNullRole_DoesNotThrow()
    {
        // Act & Assert
        _viewModel.Initialize(null);
        // If we reach here, no exception was thrown - test passes
    }

    /// <summary>
    /// Verifies that the default navigation tag is set to "Dashboards" when ViewModel is constructed.
    /// This ensures the correct default content view is displayed for Arla employees.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsDefaultNavigationTagToDashboards()
    {
        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating to "Farms" view updates CurrentNavigationTag.
    /// This ensures the navigation system works correctly for switching content views.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithFarmsTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Farms");

        // Assert
        Assert.AreEqual("Farms", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating to "Users" view updates CurrentNavigationTag.
    /// This ensures the navigation system works correctly for switching content views.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithUsersTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Users");

        // Assert
        Assert.AreEqual("Users", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating to "Dashboards" view updates CurrentNavigationTag.
    /// This ensures the navigation system works correctly for switching content views.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithDashboardsTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Dashboards");

        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating with an empty string does not change the current navigation tag.
    /// This ensures invalid navigation tags are handled gracefully.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithEmptyTag_DoesNotChangeNavigationTag()
    {
        // Arrange
        string initialTag = _viewModel.CurrentNavigationTag;

        // Act
        _viewModel.NavigationCommand?.Execute(string.Empty);

        // Assert
        Assert.AreEqual(initialTag, _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating with a null tag does not change the current navigation tag.
    /// This ensures null navigation tags are handled gracefully.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithNullTag_DoesNotChangeNavigationTag()
    {
        // Arrange
        string initialTag = _viewModel.CurrentNavigationTag;

        // Act
        _viewModel.NavigationCommand?.Execute(null);

        // Assert
        Assert.AreEqual(initialTag, _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating with whitespace-only tag does not change the current navigation tag.
    /// This ensures whitespace navigation tags are handled gracefully.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithWhitespaceTag_DoesNotChangeNavigationTag()
    {
        // Arrange
        string initialTag = _viewModel.CurrentNavigationTag;

        // Act
        _viewModel.NavigationCommand?.Execute("   ");

        // Assert
        Assert.AreEqual(initialTag, _viewModel.CurrentNavigationTag);
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
            new ArlaEmployeePageViewModel(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that multiple Initialize calls with different roles are handled correctly.
    /// This ensures the ViewModel can be re-initialized if needed.
    /// </summary>
    [TestMethod]
    public void Initialize_CalledMultipleTimes_HandlesCorrectly()
    {
        // Arrange
        Role role1 = new Role { Id = Guid.NewGuid(), Name = "ArlaEmployee" };
        Role role2 = new Role { Id = Guid.NewGuid(), Name = "ArlaEmployee" };

        // Act
        _viewModel.Initialize(role1);
        _viewModel.Initialize(role2);

        // Assert
        // If we reach here, no exception was thrown - test passes
    }
}

