using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using System.Runtime.InteropServices;
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

    [TestMethod]
    public void OnPropertyChanged_HandlerThrows_COMException_IsSwallowed()
    {
        TestViewModel vm = new TestViewModel();

        int called = 0;

        // First handler throws COMException to simulate UI interop error
        vm.PropertyChanged += (s, e) => throw new COMException("UI COM error");
        // Second handler increments counter
        vm.PropertyChanged += (s, e) => called++;

        // Act: should not throw despite handler throwing
        vm.RaiseWithExplicit("Test");

        // Assert the second handler was invoked
        Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void OnPropertyChanged_MultiThreaded_InvokesAllHandlers_ThreadSafe()
    {
        TestViewModel vm = new TestViewModel();

        const int handlerCount = 50;
        const int threads = 8;
        const int callsPerThread = 200;

        int totalCalls = 0;

        // register handlers that increment a shared counter in a thread-safe way
        for (int i = 0; i < handlerCount; i++)
        {
            vm.PropertyChanged += (s, e) => System.Threading.Interlocked.Increment(ref totalCalls);
        }

        // Fire property changed from multiple threads concurrently
        Task[] tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int c = 0; c < callsPerThread; c++)
                {
                    vm.RaiseWithExplicit("Concurrent");
                }
            });
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls, $"Expected {expected} handler invocations, got {totalCalls}");
    }
}
