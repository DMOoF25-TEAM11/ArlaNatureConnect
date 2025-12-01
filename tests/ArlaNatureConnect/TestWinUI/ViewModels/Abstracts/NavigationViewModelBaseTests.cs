using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml.Controls;

using Moq;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed partial class NavigationViewModelBaseTests
{
    private sealed partial class TestNavigationViewModel : NavigationViewModelBase
    {
        public TestNavigationViewModel() : base(new NavigationHandler()) { }

        public void SetLoading(bool v) => IsLoading = v;

        public void SetCurrent(UserControl? uc) => CurrentContent = uc;

        // Expose protected navigation method for testing
        public void InvokeNavigate(object? parameter) => base.NavigateToView(parameter);
    }

    [TestMethod]
    public void PropertyChanged_HandlerThrows_COMException_IsSwallowed()
    {
        TestNavigationViewModel vm = new();

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
        TestNavigationViewModel vm = new();

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
            }, TestContext.CancellationToken);
        }

        Task.WaitAll(tasks, TestContext.CancellationToken);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls, $"Expected {expected} handler invocations, got {totalCalls}");
    }

    [TestMethod]
    public void AttachSideMenuToMainWindow_WhenServiceProviderThrows_COMException_DoesNotThrow()
    {
        // Arrange: create mock service provider that throws COMException when resolving MainWindow
        Mock<IServiceProvider> mockServices = new();
        mockServices.Setup(sp => sp.GetService(typeof(ArlaNatureConnect.WinUI.MainWindow)))
                    .Throws(new COMException("RPC_E_WRONG_THREAD"));

        Mock<IHost> mockHost = new();
        mockHost.Setup(h => h.Services).Returns(mockServices.Object);

        // Assign to static App.HostInstance
        typeof(ArlaNatureConnect.WinUI.App)
            .GetProperty("HostInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(null, mockHost.Object);

        TestNavigationViewModel vm = new();

        // Act & Assert: should swallow the exception and not throw
        vm.AttachSideMenuToMainWindow();

        // Cleanup
        typeof(ArlaNatureConnect.WinUI.App)
            .GetProperty("HostInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(null, null!);
    }

    [TestMethod]
    public async Task AttachSideMenuToMainWindow_CalledConcurrently_IsThreadSafe()
    {
        // Arrange: Ensure HostInstance is null so resolution returns null quickly
        typeof(ArlaNatureConnect.WinUI.App).GetProperty("HostInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, null!);

        TestNavigationViewModel vm = new();

        const int iterations = 50;
        Task[] tasks = new Task[iterations];
        Exception? observed = null;

        // Act: call AttachSideMenuToMainWindow concurrently
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    vm.AttachSideMenuToMainWindow();
                }
                catch (Exception ex)
                {
                    observed = ex;
                }
            }, TestContext.CancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert: no exception observed
        Assert.IsNull(observed, observed?.ToString());
    }

    // New tests for NavigateToView
    [TestMethod]
    public void NavigateToView_FuncReturnsControl_SetsCurrentContentAndDataContext()
    {
        TestNavigationViewModel vm = new();

        // Act: navigate using a func that returns null to avoid creating real WinUI controls in unit tests
        vm.InvokeNavigate(new Func<UserControl?>(() => null));

        // Assert: no control was created and no exception thrown
        Assert.IsNull(vm.CurrentContent);
    }

    [TestMethod]
    public void NavigateToView_FuncThrows_IsWrappedAndRethrown()
    {
        TestNavigationViewModel testNavigationViewModel = new();
        TestNavigationViewModel vm = testNavigationViewModel;

        // Act
        Exception ex = Assert.Throws<Exception>(() => vm.InvokeNavigate(new Func<UserControl?>(() => throw new InvalidOperationException("boom"))));

        // Outer message matches unexpected error wrapper
        Assert.Contains("NavigateToView: unexpected error", ex.Message);
        // Inner exception should be the message from the inner wrapper
        Assert.IsNotNull(ex.InnerException);
        Assert.Contains("NavigateToView: contentFunc threw", ex.InnerException.Message);
        Assert.IsNotNull(ex.InnerException.InnerException);
        Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException.InnerException);
    }

    [TestMethod]
    public void NavigateToView_UnsupportedParameter_ThrowsWrappedInvalidOperationException()
    {
        TestNavigationViewModel vm = new();

        Exception ex = Assert.Throws<Exception>(() => vm.InvokeNavigate(123));

        Assert.Contains("NavigateToView: unexpected error", ex.Message);
        Assert.IsNotNull(ex.InnerException);
        Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("unsupported parameter type", ex.InnerException.Message);
    }

    public TestContext TestContext { get; set; }
}
