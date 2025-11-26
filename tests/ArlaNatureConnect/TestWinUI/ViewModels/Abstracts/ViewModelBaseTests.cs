using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class ViewModelBaseTests
{
    private sealed class TestViewModel : ViewModelBase
    {
        public string? NameProp { get; set; }

        public void SetNameProp(string? value)
        {
            NameProp = value;
            // CallerMemberName should supply "SetNameProp" when called from here if no arg is provided
            OnPropertyChanged();
        }

        public void RaiseWithoutName()
        {
            // CallerMemberName will provide "RaiseWithoutName"
            OnPropertyChanged();
        }

        public void RaiseWithExplicit(string? name) => OnPropertyChanged(name);
    }

    [TestMethod]
    public async Task OnPropertyChanged_WithExplicitName_RaisesEventWithThatName()
    {
        TestViewModel vm = new TestViewModel();
        List<string?> received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e.PropertyName);

        vm.RaiseWithExplicit("ExplicitName");

        Assert.HasCount(1, received);
        Assert.AreEqual("ExplicitName", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task OnPropertyChanged_CalledFromMethod_RaisesEventWithCallerName()
    {
        TestViewModel vm = new TestViewModel();
        List<string?> received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e.PropertyName);

        vm.RaiseWithoutName();

        Assert.HasCount(1, received);
        Assert.AreEqual("RaiseWithoutName", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task OnPropertyChanged_CalledFromSetter_RaisesEventWithPropertyName()
    {
        TestViewModel vm = new TestViewModel();
        List<string?> received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e.PropertyName);

        vm.SetNameProp("value");

        Assert.HasCount(1, received);
        // When called from SetNameProp the CallerMemberName will be "SetNameProp"
        Assert.AreEqual("SetNameProp", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task OnPropertyChanged_MultipleHandlers_AllHandlersInvoked()
    {
        TestViewModel vm = new TestViewModel();
        int calls = 0;
        void Handler1(object? s, System.ComponentModel.PropertyChangedEventArgs e) => calls++;
        void Handler2(object? s, System.ComponentModel.PropertyChangedEventArgs e) => calls++;

        vm.PropertyChanged += Handler1;
        vm.PropertyChanged += Handler2;

        vm.RaiseWithExplicit("X");

        Assert.AreEqual(2, calls);

        vm.PropertyChanged -= Handler1;
        vm.PropertyChanged -= Handler2;

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task OnPropertyChanged_NoHandlers_DoesNotThrow()
    {
        TestViewModel vm = new TestViewModel();

        // Should not throw even if there are no subscribers
        vm.RaiseWithExplicit("NoOneListening");
        vm.RaiseWithoutName();

        await Task.CompletedTask;
    }
}
