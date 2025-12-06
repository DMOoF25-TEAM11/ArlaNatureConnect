using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

using Microsoft.UI.Xaml.Controls;

using Moq;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Controls.SideMenu;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class AdministratorPageSideMenuUCViewModelTests
{
    private sealed class DummyDisposable : IDisposable { public void Dispose() { } }

    private sealed class TestNavigationHost : NavigationViewModelBase
    {
        public TestNavigationHost(INavigationHandler navigationHandler) : base(navigationHandler)
        {
        }

        public void SetCurrentContent(UserControl content)
        {
            this.CurrentContent = content;
        }

        protected override void NavigateToView(object? parameter)
        {
            if (parameter is Func<UserControl?> contentFunc)
            {
                UserControl? ctrl = contentFunc();
                if (ctrl != null)
                    this.CurrentContent = ctrl;
            }
            else
            {
                base.NavigateToView(parameter);
            }
        }
    }

    [TestMethod]
    public void Constructor_Initializes_NavItems_And_Commands()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);

        Assert.IsNotNull(vm.NavItems);
        Assert.HasCount(2, vm.NavItems);
        Assert.AreEqual("Dashboards", vm.NavItems[0].Label);
        Assert.AreEqual("Administrere personer", vm.NavItems[1].Label);
        Assert.IsNotNull(vm.DashboardsCommand);
        Assert.IsNotNull(vm.AdministratePersonsCommand);
        Assert.AreEqual(vm.DashboardsCommand, vm.NavItems[0].Command);
        Assert.AreEqual(vm.AdministratePersonsCommand, vm.NavItems[1].Command);
    }

    [TestMethod]
    public async Task InitializeAsync_Populates_AvailablePersons()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        List<Person> persons = new List<Person>
        {
            new Person { Id = Guid.NewGuid(), FirstName = "Admin1" },
            new Person { Id = Guid.NewGuid(), FirstName = "Admin2" }
        };
        repoMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(persons);
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);

        await vm.InitializeAsync();

        Assert.IsNotNull(vm.AvailablePersons);
        Assert.HasCount(2, vm.AvailablePersons);
        Assert.AreEqual("Admin1", vm.AvailablePersons[0].FirstName);
    }

    [TestMethod]
    public void DashboardsCommand_Executes_SetsSelected_And_Navigates()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        Mock<INavigationHandler> navMock = new();

        TestNavigationHost host = new TestNavigationHost(navMock.Object);
        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);
        vm.SetHostPageViewModel(host);

        // execute dashboards
        vm.DashboardsCommand.Execute(null);

        // the first nav item should be selected
        Assert.IsTrue(vm.NavItems[0].IsSelected);
        Assert.IsFalse(vm.NavItems[1].IsSelected);

        // CurrentContent on host should be set to a UserControl (AdministratorDashboardUC)
        // TODO fix this so host.CurrentContent is actually set to AdministratorDashboardUC
        Assert.IsInstanceOfType<UserControl>(host.CurrentContent);
    }

    [TestMethod]
    public void AdministratePersonsCommand_Executes_SetsSelected_And_Navigates()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);

        TestNavigationHost host = new TestNavigationHost(navMock.Object);
        vm.SetHostPageViewModel(host);

        // execute administrate persons
        vm.AdministratePersonsCommand.Execute(null);

        // the second nav item should be selected
        Assert.IsFalse(vm.NavItems[0].IsSelected);
        Assert.IsTrue(vm.NavItems[1].IsSelected);

        Assert.IsInstanceOfType(host.CurrentContent, typeof(UserControl));
    }

    [TestMethod]
    public async Task InitializeAsync_Propagates_COMException_From_Repository()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        repoMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new COMException("COM error"));
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);

        try
        {
            await vm.InitializeAsync();
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }

    [TestMethod]
    public void IsLoading_PropertyChanged_Raises_Command_CanExecuteChanged()
    {
        Mock<IStatusInfoServices> statusMock = new Mock<IStatusInfoServices>();
        statusMock.Setup(s => s.BeginLoadingOrSaving()).Returns(new DummyDisposable());
        Mock<IAppMessageService> msgMock = new Mock<IAppMessageService>();
        Mock<IPersonRepository> repoMock = new Mock<IPersonRepository>();
        Mock<INavigationHandler> navMock = new Mock<INavigationHandler>();

        AdministratorPageSideMenuUCViewModel vm = new AdministratorPageSideMenuUCViewModel(statusMock.Object, msgMock.Object, repoMock.Object, navMock.Object);

        int dashboardsChanged = 0;
        int administrateChanged = 0;

        vm.DashboardsCommand.CanExecuteChanged += (_, _) => dashboardsChanged++;
        vm.AdministratePersonsCommand.CanExecuteChanged += (_, _) => administrateChanged++;

        // toggle loading
        vm.IsLoading = true;
        Assert.AreEqual(1, dashboardsChanged);
        Assert.AreEqual(1, administrateChanged);

        vm.IsLoading = false;
        Assert.AreEqual(2, dashboardsChanged);
        Assert.AreEqual(2, administrateChanged);
    }
}
