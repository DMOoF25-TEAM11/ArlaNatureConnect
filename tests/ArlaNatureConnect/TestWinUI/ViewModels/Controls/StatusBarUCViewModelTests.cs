using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Controls;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public class StatusBarUCViewModelTests
{
    [TestMethod]
    public async Task IsBusy_True_BusySymbolIsHourglass()
    {
        // Arrange
        Mock<IStatusInfoServices> statusInfoMock = new Mock<IStatusInfoServices>();
        statusInfoMock.SetupGet(s => s.IsLoading).Returns(true);
        statusInfoMock.SetupGet(s => s.HasDbConnection).Returns(true);
        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusInfoMock.Object);

        // Act
        // Simulate status change
        statusInfoMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // Assert
        Assert.IsTrue(vm.IsBusy);
        Assert.AreEqual("⏳", vm.BusySymbol);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task IsBusy_False_BusySymbolIsCheckmark()
    {
        // Arrange
        Mock<IStatusInfoServices> statusInfoMock = new Mock<IStatusInfoServices>();
        statusInfoMock.SetupGet(s => s.IsLoading).Returns(false);
        statusInfoMock.SetupGet(s => s.HasDbConnection).Returns(true);
        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusInfoMock.Object);

        // Act
        // Simulate status change
        statusInfoMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // Assert
        Assert.IsFalse(vm.IsBusy);
        Assert.AreEqual("✔️", vm.BusySymbol);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task HasDbConnection_Change_Should_Update_ViewModel()
    {
        // Arrange
        bool hasDb = false;
        Mock<IStatusInfoServices> statusInfoMock = new Mock<IStatusInfoServices>();
        statusInfoMock.SetupGet(s => s.IsLoading).Returns(false);
        statusInfoMock.SetupGet(s => s.HasDbConnection).Returns(() => hasDb);
        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusInfoMock.Object);

        // Act/Assert initial state
        statusInfoMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);
        Assert.IsFalse(vm.HasDbConnection);
        Assert.AreEqual("❌", vm.DbConnectionSymbol);

        // Change service state and raise event
        hasDb = true;
        statusInfoMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // Assert updated
        Assert.IsTrue(vm.HasDbConnection);
        Assert.AreEqual("✅", vm.DbConnectionSymbol);

        await Task.CompletedTask;
    }
}
