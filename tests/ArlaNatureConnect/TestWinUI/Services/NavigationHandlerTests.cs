using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Views.Pages;

using System.Runtime.Versioning;

namespace TestWinUI.Services;

/// <summary>
/// Unit tests for <see cref="NavigationHandler"/> covering navigation functionality and error handling.
/// 
/// These tests verify:
/// - Navigation requires initialization before use
/// - Navigate method validates initialization
/// - GoBack functionality validates initialization
/// - CanGoBack property reflects initialization state
/// - Error handling when not initialized
/// 
/// Note: Frame is a WinUI UI component that cannot be mocked with Moq.
/// These tests focus on NavigationHandler's own logic and validation.
/// </summary>
[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class NavigationHandlerTests
{
    private NavigationHandler _navigationHandler = null!;

    /// <summary>
    /// Sets up test dependencies before each test method runs.
    /// Creates a new NavigationHandler instance.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _navigationHandler = new NavigationHandler();
    }

    /// <summary>
    /// Verifies that Navigate throws InvalidOperationException when NavigationHandler is not initialized.
    /// This ensures proper error handling when Navigate is called before Initialize.
    /// </summary>
    [TestMethod]
    public void Navigate_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        // Act & Assert
        try
        {
            _navigationHandler.Navigate(typeof(LoginPage));
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Contains("NavigationHandler has not been initialized", ex.Message);
        }
    }

    /// <summary>
    /// Verifies that Initialize throws ArgumentNullException when Frame is null.
    /// This ensures proper validation of dependencies.
    /// </summary>
    [TestMethod]
    public void Initialize_WithNullFrame_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            _navigationHandler.Initialize(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that GoBack returns false when Frame is not initialized.
    /// This ensures graceful handling when navigation history is not available.
    /// </summary>
    [TestMethod]
    public void GoBack_WhenNotInitialized_ReturnsFalse()
    {
        // Act
        bool result = _navigationHandler.GoBack();

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that CanGoBack returns false when Frame is not initialized.
    /// This ensures the property reflects the initialization state.
    /// </summary>
    [TestMethod]
    public void CanGoBack_WhenFrameNotInitialized_ReturnsFalse()
    {
        // Act
        bool result = _navigationHandler.CanGoBack;

        // Assert
        Assert.IsFalse(result);
    }

}

