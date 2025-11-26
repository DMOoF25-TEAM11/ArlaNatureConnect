using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class NavigationViewModelBaseTests
{
    private sealed class TestNavigationViewModel : NavigationViewModelBase
    {
        public TestNavigationViewModel(string defaultTag = "") : base()
        {
            InitializeNavigation(defaultTag);
        }

        // Expose protected RegisterContent for tests
        public void AddContent(string tag, Func<UserControl> factory) => RegisterContent(tag, factory);

        protected override void SwitchContentView(string? navigationTag)
        {
            string tag = string.IsNullOrWhiteSpace(navigationTag) ? CurrentNavigationTag : navigationTag!;
            if (_contentFactories.TryGetValue(tag, out Func<object?>? factory))
            {
                // enqueue UI creation to the dispatcher
                DispatcherQueue dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                if (dq != null)
                {
                    dq.TryEnqueue(() =>
                    {
                        object? newContent = factory();
                        if (newContent is FrameworkElement fe) fe.DataContext = this;
                        CurrentContent = newContent as UserControl;
                    });
                    return;
                }

                // fallback synchronous creation
                object? syncContent = factory();
                if (syncContent is FrameworkElement sfe) sfe.DataContext = this;
                CurrentContent = syncContent as UserControl;
            }
        }
    }

    [TestMethod]
    public async Task InitializeNavigation_SetsCommand_And_DefaultTag()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel("start");

        Assert.IsNotNull(vm.NavigationCommand);
        Assert.AreEqual("start", vm.CurrentNavigationTag);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_StringTag_UpdatesCurrentNavigationTag()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel("start");

        vm.NavigationCommand!.Execute("MyTag");

        Assert.AreEqual("MyTag", vm.CurrentNavigationTag);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_FuncString_UpdatesTag()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        vm.NavigationCommand!.Execute(new Func<string>(() => "fromFunc"));

        Assert.AreEqual("fromFunc", vm.CurrentNavigationTag);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_FuncTaskString_UpdatesTag_Asynchronously()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        vm.NavigationCommand!.Execute(new Func<Task<string>>(async () => { await Task.Delay(5); return "asyncTag"; }));

        // wait for ContinueWith to run and update the tag
        int attempts = 0;
        while (vm.CurrentNavigationTag != "asyncTag" && attempts < 50)
        {
            await Task.Delay(10);
            attempts++;
        }

        Assert.AreEqual("asyncTag", vm.CurrentNavigationTag);
    }

    [TestMethod]
    public async Task NavigationCommand_Action_IsInvoked()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();
        bool called = false;

        vm.NavigationCommand!.Execute(new Action(() => called = true));

        Assert.IsTrue(called);
        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_FuncUserControl_SetsCurrentContent_And_DataContext()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        vm.NavigationCommand!.Execute(new Func<UserControl?>(() => new UserControl()));

        Assert.IsNotNull(vm.CurrentContent);
        Assert.AreSame(vm, ((UserControl)vm.CurrentContent!).DataContext);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_FuncVmToString_UpdatesTag()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        vm.NavigationCommand!.Execute(new Func<NavigationViewModelBase, string>(v => "vmTag"));

        Assert.AreEqual("vmTag", vm.CurrentNavigationTag);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task RegisterContent_Factory_IsUsed_By_SwitchingTag()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        vm.AddContent("page1", () => new UserControl());

        vm.NavigationCommand!.Execute("page1");

        Assert.IsNotNull(vm.CurrentContent);
        Assert.AreSame(vm, ((UserControl)vm.CurrentContent!).DataContext);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task CanNavigate_Behavior_ForVariousParameters()
    {
        TestNavigationViewModel vm = new TestNavigationViewModel();

        Assert.IsFalse(vm.NavigationCommand!.CanExecute(null));
        Assert.IsTrue(vm.NavigationCommand.CanExecute("Tag"));
        Assert.IsFalse(vm.NavigationCommand.CanExecute("   "));
        Assert.IsTrue(vm.NavigationCommand.CanExecute(new Func<string>(() => "x")));

        await Task.CompletedTask;
    }
}
