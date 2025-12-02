using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Moq;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Input;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class SideMenuViewModelBaseTests
{
    private sealed class TestSideMenuViewModel : SideMenuViewModelBase
    {
        public TestSideMenuViewModel(
            IStatusInfoServices status,
            IAppMessageService msg,
            IPersonRepository repo,
            INavigationHandler navigationHandler)
            : base(status, msg, repo, navigationHandler)
        {
        }

        // expose repository for tests
        public IPersonRepository Repository => this.GetRepository();

        private IPersonRepository GetRepository()
        {
            return (IPersonRepository)typeof(SideMenuViewModelBase)
                .GetField("_repository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(this)!;
        }
    }

    // New tests for NavItem
    private sealed class TestCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
        public event System.EventHandler? CanExecuteChanged;
    }

    [TestMethod]
    public void NavItem_Constructor_Sets_Label_And_Command_And_Defaults()
    {
        TestCommand cmd = new TestCommand();
        SideMenuViewModelBase.NavItem nav = new SideMenuViewModelBase.NavItem("Label1", cmd);

        Assert.AreEqual("Label1", nav.Label);
        Assert.AreEqual(cmd, nav.Command);
        Assert.IsFalse(nav.IsSelected, "Default IsSelected should be false.");
    }

    [TestMethod]
    public void NavItem_IsSelected_Raises_PropertyChanged_Only_On_Actual_Change()
    {
        SideMenuViewModelBase.NavItem nav = new SideMenuViewModelBase.NavItem("L", null);
        string? receivedName = null;
        int callCount = 0;

        nav.PropertyChanged += (sender, e) =>
        {
            receivedName = e.PropertyName;
            callCount++;
        };

        // change from default false to true -> should raise
        nav.IsSelected = true;
        Assert.AreEqual(1, callCount, "PropertyChanged should have been raised once after changing value.");
        Assert.AreEqual("IsSelected", receivedName);

        // setting same value again -> should not raise
        nav.IsSelected = true;
        Assert.AreEqual(1, callCount, "PropertyChanged should not be raised when setting same value.");

        // change back to false -> should raise again
        nav.IsSelected = false;
        Assert.AreEqual(2, callCount, "PropertyChanged should be raised when changing value back.");
    }

    [TestMethod]
    public async Task LoadAvailablePersonsAsync_WithEmptyRole_ClearsAvailablePersons()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        mockRepo.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Person> { new Person { Id = Guid.NewGuid(), FirstName = "X" } });

        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object, new NavigationHandler());

        // pre-populate to ensure it gets cleared
        vm.AvailablePersons.Add(new Person { Id = Guid.NewGuid(), FirstName = "pre" });

        await vm.LoadAvailablePersonsAsync("  ");

        Assert.IsNotNull(vm.AvailablePersons);
        Assert.IsEmpty(vm.AvailablePersons);
    }

    [TestMethod]
    public async Task LoadAvailablePersonsAsync_WithRole_PopulatesAvailablePersons()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        List<Person> list = new List<Person>
        {
            new Person { Id = Guid.NewGuid(), FirstName = "A" },
            new Person { Id = Guid.NewGuid(), FirstName = "B" }
        };
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        mockRepo.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);

        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object, new NavigationHandler());

        await vm.LoadAvailablePersonsAsync("Farmer");

        Assert.HasCount(2, vm.AvailablePersons);
        Assert.AreEqual("A", vm.AvailablePersons[0].FirstName);
        Assert.AreEqual("B", vm.AvailablePersons[1].FirstName);
    }

    [TestMethod]
    public async Task SelectedPerson_Set_Raises_PropertyChanged()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object, new NavigationHandler());

        List<string?> received = new();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        Person p = new Person { Id = Guid.NewGuid(), FirstName = "S" };
        vm.SelectedPerson = p;

        Assert.HasCount(1, received);
        Assert.AreEqual("SelectedPerson", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_CanExecute_Behavior()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object, new NavigationHandler());

        Assert.IsFalse(vm.NavigationCommand!.CanExecute(null));
        Assert.IsTrue(vm.NavigationCommand.CanExecute("Tag"));
        Assert.IsFalse(vm.NavigationCommand.CanExecute("   "));

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task LogoutCommand_CanExecute_ReturnsTrue()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object, new NavigationHandler());

        Assert.IsTrue(vm.LogoutCommand!.CanExecute(null));
        await Task.CompletedTask;
    }
    [TestMethod]
    public async Task LoadAvailablePersonsAsync_Propagates_COMException_From_Repository()
    {
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        repoMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new COMException("COM error"));

        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> appMsgMock = new Mock<IAppMessageService>();
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        TestSideMenuViewModel vm = new TestSideMenuViewModel(statusMock.Object, appMsgMock.Object, repoMock.Object, navMock.Object);

        try
        {
            await vm.LoadAvailablePersonsAsync("Farmer");
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }

}
