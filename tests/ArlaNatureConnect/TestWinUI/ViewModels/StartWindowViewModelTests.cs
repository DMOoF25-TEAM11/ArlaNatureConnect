using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels;

[DoNotParallelize]
[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class StartWindowViewModelTests
{
    private Mock<IConnectionStringService> _mockConnService = null!;
    private Mock<IStatusInfoServices> _mockStatusInfoServices = null!;
    private StartWindowViewModel _viewModel = null!;
    private string _assetsDir = null!;
    private string _assetFile = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockConnService = new Mock<IConnectionStringService>();
        _mockStatusInfoServices = new Mock<IStatusInfoServices>();
        _viewModel = new StartWindowViewModel(_mockConnService.Object, _mockStatusInfoServices.Object);

        _assetsDir = Path.Combine(AppContext.BaseDirectory ?? string.Empty, "Assets");
        _assetFile = Path.Combine(_assetsDir, "startUpImage.jpg");

        // Ensure clean state for asset file
        if (Directory.Exists(_assetsDir))
        {
            try { Directory.Delete(_assetsDir, recursive: true); } catch { }
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (File.Exists(_assetFile)) File.Delete(_assetFile);
            if (Directory.Exists(_assetsDir)) Directory.Delete(_assetsDir, recursive: true);
        }
        catch { }
    }

    [TestMethod]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        try
        {
            new StartWindowViewModel(null!, null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }

    [TestMethod]
    public async Task ConnectionMethods_DelegateToService()
    {
        // Arrange
        _mockConnService.Setup(s => s.ExistsAsync()).ReturnsAsync(true);
        _mockConnService.Setup(s => s.ReadAsync()).ReturnsAsync("Data Source=.;Initial Catalog=Test;");
        _mockConnService.Setup(s => s.SaveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act & Assert
        Assert.IsTrue(await _viewModel.ConnectionExistsAsync());
        Assert.AreEqual("Data Source=.;Initial Catalog=Test;", await _viewModel.ReadConnectionStringAsync());
        await _viewModel.SaveConnectionStringAsync("foo");
        _mockConnService.Verify(s => s.SaveAsync("foo"), Times.Once);
    }

    [TestMethod]
    public async Task ValidateConnectionString_Empty_ReturnsFalseAndMessage()
    {
        (bool isValid, string? msg) = await _viewModel.ValidateConnectionStringWithRetryAsync(string.Empty);
        Assert.IsFalse(isValid);
        Assert.IsFalse(string.IsNullOrWhiteSpace(msg));
        Assert.IsTrue(msg!.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task ValidateConnectionString_MissingDataSource_ReturnsFalseAndMessage()
    {
        // connection string without Data Source / Server
        string cs = "Initial Catalog=Test;User Id=sa;Password=pass;";
        (bool isValid, string? msg) = await _viewModel.ValidateConnectionStringWithRetryAsync(cs);
        Assert.IsFalse(isValid);
        Assert.IsFalse(string.IsNullOrWhiteSpace(msg));
        Assert.IsTrue(msg!.Contains("missing a server", StringComparison.OrdinalIgnoreCase) || msg.Contains("data source", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task InitializeAsync_ImageExists_NoConnection_ShowsDialogAndSave()
    {
        // Arrange - create asset file so image lookup succeeds
        Directory.CreateDirectory(_assetsDir);
        await File.WriteAllTextAsync(_assetFile, "dummy");

        bool dialogShown = false;
        Func<Task> showDialog = () => { dialogShown = true; return Task.CompletedTask; };
        Func<string, Task> showError = (m) => { return Task.CompletedTask; };

        _mockConnService.Setup(s => s.ExistsAsync()).ReturnsAsync(false);

        // Act
        await _viewModel.InitializeAsync(showDialog, showError);

        // Assert
        Assert.IsTrue(dialogShown);
    }

    [TestMethod]
    public async Task InitializeAsync_ImageExists_ConnectionExistsButEmptyRead_ShowsDialog()
    {
        // Arrange - create asset file so image lookup succeeds
        Directory.CreateDirectory(_assetsDir);
        await File.WriteAllTextAsync(_assetFile, "dummy");

        bool dialogShown = false;
        Func<Task> showDialog = () => { dialogShown = true; return Task.CompletedTask; };
        Func<string, Task> showError = (m) => { return Task.CompletedTask; };

        _mockConnService.Setup(s => s.ExistsAsync()).ReturnsAsync(true);
        _mockConnService.Setup(s => s.ReadAsync()).ReturnsAsync((string?)"\t");

        // Act
        await _viewModel.InitializeAsync(showDialog, showError);

        // Assert
        Assert.IsTrue(dialogShown);
    }
}
