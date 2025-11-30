using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml.Controls;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class NavigationViewModelBaseTests
{
    private sealed class TestNavigationViewModel : NavigationViewModelBase
    {
        public TestNavigationViewModel() : base(new NavigationHandler()) { }

        public void SetLoading(bool v) => IsLoading = v;

        public void SetCurrent(UserControl? uc) => CurrentContent = uc;
    }

    [TestMethod]
    public void PropertyChanged_HandlerThrows_COMException_IsSwallowed()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        int called = 0;

        // First handler throws COMException to simulate UI interop error
        vm.PropertyChanged += (s, e) => throw new COMException("UI COM error");
        // Second handler increments counter
        vm.PropertyChanged += (s, e) => called++;

        // Act: should not throw despite handler throwing
        vm.SetLoading(true);

        // Assert the second handler was invoked
        Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void PropertyChanged_MultiThreaded_InvokesAllHandlers_ThreadSafe()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        const int handlerCount = 40;
        const int threads = 6;
        const int callsPerThread = 150;

        int totalCalls = 0;

        // register handlers that increment a shared counter in a thread-safe way
        for (int i = 0; i < handlerCount; i++)
        {
            vm.PropertyChanged += (s, e) => System.Threading.Interlocked.Increment(ref totalCalls);
        }

        // Fire property changed from multiple threads concurrently by toggling IsLoading
        Task[] tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int c = 0; c < callsPerThread; c++)
                {
                    vm.SetLoading(c % 2 == 0);
                }
            });
        }

        Task.WaitAll(tasks);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls, $"Expected {expected} handler invocations, got {totalCalls}");
    }
}
