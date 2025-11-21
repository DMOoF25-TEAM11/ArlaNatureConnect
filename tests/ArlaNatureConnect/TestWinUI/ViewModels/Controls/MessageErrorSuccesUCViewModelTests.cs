using System.ComponentModel;
using System.Runtime.Versioning;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Controls;

namespace TestWinUI.ViewModels.Controls;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed partial class MessageErrorSuccesUCViewModelTests
{
    private sealed partial class StubAppMessageService : IAppMessageService
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? EntityName { get; set; }
        public bool HasStatusMessages => StatusMessages?.GetEnumerator().MoveNext() ?? false;
        public bool HasErrorMessages => ErrorMessages?.GetEnumerator().MoveNext() ?? false;
        public IEnumerable<string> StatusMessages { get; set; } = [];
        public IEnumerable<string> ErrorMessages { get; set; } = [];

        public void AddInfoMessage(string message) => ((List<string>)StatusMessages).Add(message);
        public void AddErrorMessage(string message) => ((List<string>)ErrorMessages).Add(message);
        public void ClearErrorMessages() => ((List<string>)ErrorMessages).Clear();
    }

    [TestMethod]
    public void DefaultConstructor_DoesNotThrow_And_DefaultsAreEmpty()
    {
        MessageErrorSuccesUCViewModel vm = new();

        Assert.IsNotNull(vm);
        Assert.AreEqual(string.Empty, vm.StatusMessage);
        Assert.AreEqual(string.Empty, vm.ErrorMessage);
        Assert.IsFalse(vm.IsSuccessVisible);
        Assert.IsFalse(vm.IsErrorVisible);
    }

    [TestMethod]
    public void DiConstructor_WithService_DoesNotThrow()
    {
        StubAppMessageService svc = new();
        MessageErrorSuccesUCViewModel vm = new(svc);

        Assert.IsNotNull(vm);
    }

    [TestMethod]
    public void SettingStatusMessage_RaisesPropertyChanged_And_UpdatesVisibility()
    {
        MessageErrorSuccesUCViewModel vm = new();
        List<string?> changed = [];
        vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

        vm.StatusMessage = "All good";

        Assert.AreEqual("All good", vm.StatusMessage);
        Assert.IsTrue(vm.IsSuccessVisible);
        Assert.IsFalse(vm.IsErrorVisible);
        Assert.Contains("StatusMessage", changed);
    }

    [TestMethod]
    public void SettingErrorMessage_RaisesPropertyChanged_And_UpdatesVisibility()
    {
        MessageErrorSuccesUCViewModel vm = new();
        List<string?> changed = [];
        vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

        vm.ErrorMessage = "Something went wrong";

        Assert.AreEqual("Something went wrong", vm.ErrorMessage);
        Assert.IsTrue(vm.IsErrorVisible);
        Assert.IsFalse(vm.IsSuccessVisible);
        Assert.Contains("ErrorMessage", changed);
    }

    [TestMethod]
    public void ClearingMessages_ResultsInVisibilitiesFalse()
    {
        MessageErrorSuccesUCViewModel vm = new()
        {
            StatusMessage = "OK",
            ErrorMessage = "Err"
        };

        Assert.IsTrue(vm.IsSuccessVisible);
        Assert.IsTrue(vm.IsErrorVisible);

        vm.StatusMessage = string.Empty;
        vm.ErrorMessage = string.Empty;

        Assert.IsFalse(vm.IsSuccessVisible);
        Assert.IsFalse(vm.IsErrorVisible);
    }
}
