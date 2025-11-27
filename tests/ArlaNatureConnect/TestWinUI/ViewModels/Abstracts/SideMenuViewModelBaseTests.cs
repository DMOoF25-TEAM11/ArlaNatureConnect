using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class SideMenuViewModelBaseTests
{
    private sealed class TestSideMenuViewModel : SideMenuViewModelBase
    {
        public TestSideMenuViewModel(IStatusInfoServices status, IAppMessageService msg, IPersonRepository repo)
            : base(status, msg, repo)
        {
        }

        // expose repository for tests
        public IPersonRepository Repository => this.GetRepository();

        private IPersonRepository GetRepository()
        {
            // Use a protected getter in base, or expose _repository via a protected property in SideMenuViewModelBase
            return (IPersonRepository)typeof(SideMenuViewModelBase)
                .GetField("_repository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(this)!;
        }
    }

    [TestMethod]
    public async Task LoadAvailablePersonsAsync_WithEmptyRole_ClearsAvailablePersons()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        mockRepo.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Person> { new Person { Id = Guid.NewGuid(), FirstName = "X" } });

        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        // pre-populate to ensure it gets cleared
        vm.AvailablePersons.Add(new Person { Id = Guid.NewGuid(), FirstName = "pre" });

        await vm.LoadAvailablePersonsAsync("  ");

        Assert.IsNotNull(vm.AvailablePersons);
        Assert.AreEqual(0, vm.AvailablePersons.Count);
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

        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        await vm.LoadAvailablePersonsAsync("Farmer");

        Assert.AreEqual(2, vm.AvailablePersons.Count);
        Assert.AreEqual("A", vm.AvailablePersons[0].FirstName);
        Assert.AreEqual("B", vm.AvailablePersons[1].FirstName);
    }

    [TestMethod]
    public async Task SelectedPerson_Set_Raises_PropertyChanged()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        List<string?> received = new();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        Person p = new Person { Id = Guid.NewGuid(), FirstName = "S" };
        vm.SelectedPerson = p;

        Assert.AreEqual(1, received.Count);
        Assert.AreEqual("SelectedPerson", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task IsLoading_Set_Raises_PropertyChanged()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        List<string?> received = new();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        vm.IsLoading = true;

        Assert.AreEqual(1, received.Count);
        Assert.AreEqual("IsLoading", received[0]);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task NavigationCommand_CanExecute_Behavior()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

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
        TestSideMenuViewModel vm = new TestSideMenuViewModel(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        Assert.IsTrue(vm.LogoutCommand!.CanExecute(null));
        await Task.CompletedTask;
    }
}
