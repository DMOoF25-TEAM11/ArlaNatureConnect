using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Moq;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Controls;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public class StatusBarUCViewModelTests
{
    [TestMethod]
    public void InitializeAndEvent_IsBusy_True_ShowsHourglassAndBusyLabel()
    {
        // Arrange
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.SetupGet(s => s.IsLoadingOrSaving).Returns(true);
        statusMock.SetupGet(s => s.HasDbConnection).Returns(true);

        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusMock.Object);

        // Act
        vm.InitializeForUi(null);
        statusMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // Assert
        Assert.IsTrue(vm.IsBusy);
        Assert.AreEqual("⏳", vm.BusySymbol);
        Assert.AreEqual("Busy:", vm.BusyLabel);
    }

    [TestMethod]
    public void InitializeAndEvent_IsBusy_False_ShowsCheckmarkAndIdleLabel()
    {
        // Arrange
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.SetupGet(s => s.IsLoadingOrSaving).Returns(false);
        statusMock.SetupGet(s => s.HasDbConnection).Returns(true);

        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusMock.Object);

        // Act
        vm.InitializeForUi(null);
        statusMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // Assert
        Assert.IsFalse(vm.IsBusy);
        Assert.AreEqual("✔️", vm.BusySymbol);
        Assert.AreEqual("Idle:", vm.BusyLabel);
    }

    [TestMethod]
    public void InitializeAndEvent_HasDbConnection_TogglesSymbols()
    {
        bool hasDb = false;
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.SetupGet(s => s.IsLoadingOrSaving).Returns(false);
        statusMock.SetupGet(s => s.HasDbConnection).Returns(() => hasDb);

        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusMock.Object);

        vm.InitializeForUi(null);

        // initial: false
        statusMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);
        Assert.IsFalse(vm.HasDbConnection);
        Assert.AreEqual("❌", vm.DbConnectionSymbol);

        // change to true
        hasDb = true;
        statusMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);
        Assert.IsTrue(vm.HasDbConnection);
        Assert.AreEqual("✅", vm.DbConnectionSymbol);
    }

    [TestMethod]
    public void ServicePropertyAccess_Throws_COMException_IsHandled()
    {
        // Arrange: mock that throws COMException when getters accessed
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.SetupGet(s => s.IsLoadingOrSaving).Throws(new COMException("COM failure"));
        statusMock.SetupGet(s => s.HasDbConnection).Throws(new COMException("COM failure"));

        StatusBarUCViewModel vm = new StatusBarUCViewModel(statusMock.Object);

        // Act & Assert: InitializeForUi and raising event should not throw despite COMException
        vm.InitializeForUi(null);

        // Raising the event should be swallowed by the viewmodel
        statusMock.Raise(s => s.StatusInfoChanged += null, vm, System.EventArgs.Empty);

        // When service access throws, wrappers return false, so vm should show not busy and No DB
        Assert.IsFalse(vm.IsBusy);
        Assert.AreEqual("✔️", vm.BusySymbol);
        Assert.IsFalse(vm.HasDbConnection);
        Assert.AreEqual("❌", vm.DbConnectionSymbol);
    }
}
