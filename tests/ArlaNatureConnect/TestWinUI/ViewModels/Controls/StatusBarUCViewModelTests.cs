using System.Runtime.Versioning;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Controls;

namespace TestWinUI.ViewModels.Controls;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class StatusBarUCViewModelTests
{
    private static string _busySymbol_WhenIsBusyTrue => "⏳";
    private static string _busySymbol_WhenIsBusyFalse => string.Empty;
    private static string _dbConnectionSymbol_WhenHasDbConnectionTrue => "✅";
    private static string _dbConnectionSymbol_WhenHasDbConnectionFalse => "❌";


    [TestMethod]
    public void ParameterlessCtor_DoesNotThrow_AndDefaultsAreFalse()
    {
        StatusBarUCViewModel vm = new();

        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasDbConnection);
        Assert.AreEqual(_busySymbol_WhenIsBusyFalse, vm.BusySymbol);
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionFalse, vm.DbConnectionSymbol);
    }

    [TestMethod]
    public void Ctor_WithService_InitializesFromService()
    {
        StatusInfoService svc = new()
        {
            HasDbConnection = true,
            IsLoading = true
        };

        StatusBarUCViewModel vm = new(svc);

        // constructor reads initial values from service
        Assert.IsTrue(vm.HasDbConnection);
        Assert.IsTrue(vm.IsBusy);
        Assert.AreEqual(_busySymbol_WhenIsBusyTrue, vm.BusySymbol);
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionTrue, vm.DbConnectionSymbol);
    }

    [TestMethod]
    public void StatusInfoServices_StatusInfoChanged_Updates_ViewModel()
    {
        StatusInfoService svc = new();
        StatusBarUCViewModel vm = new(svc);

        // Initially both false
        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasDbConnection);

        // Change service state; StatusInfoService will raise StatusInfoChanged
        svc.IsLoading = true;
        svc.HasDbConnection = true;

        Assert.IsTrue(vm.IsBusy);
        Assert.IsTrue(vm.HasDbConnection);
        Assert.AreEqual(_busySymbol_WhenIsBusyTrue, vm.BusySymbol);
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionTrue, vm.DbConnectionSymbol);

        // Revert
        svc.IsLoading = false;
        svc.HasDbConnection = false;

        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasDbConnection);
        Assert.AreEqual(_busySymbol_WhenIsBusyFalse, vm.BusySymbol);
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionFalse, vm.DbConnectionSymbol);
    }

    [TestMethod]
    public async Task InitializeAsync_UsesBeginLoading_And_EndsNotBusy()
    {
        StatusInfoService svc = new StatusInfoService();
        StatusBarUCViewModel vm = new StatusBarUCViewModel(svc);

        // Call InitializeAsync and wait for it to complete
        await vm.InitializeAsync();

        // After initialization completes the service should not be loading
        Assert.IsFalse(svc.IsLoading);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public void BusySymbol_And_DbConnectionSymbol_Reflect_Properties()
    {
        StatusInfoService svc = new StatusInfoService();
        StatusBarUCViewModel vm = new StatusBarUCViewModel(svc);

        svc.IsLoading = true;
        Assert.AreEqual(_busySymbol_WhenIsBusyTrue, vm.BusySymbol);

        svc.IsLoading = false;
        Assert.AreEqual(_busySymbol_WhenIsBusyFalse, vm.BusySymbol);

        svc.HasDbConnection = true;
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionTrue, vm.DbConnectionSymbol);

        svc.HasDbConnection = false;
        Assert.AreEqual(_dbConnectionSymbol_WhenHasDbConnectionFalse, vm.DbConnectionSymbol);
    }
}
