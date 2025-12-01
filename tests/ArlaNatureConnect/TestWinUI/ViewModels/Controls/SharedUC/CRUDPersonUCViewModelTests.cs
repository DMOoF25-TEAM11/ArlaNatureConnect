using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

using Moq;

using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Threading;

namespace TestWinUI.ViewModels.Controls.SharedUC;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class CRUDPersonUCViewModelTests
{
    private sealed class DisposableAction : IDisposable { private readonly Action _on; public DisposableAction(Action on) => _on = on; public void Dispose() => _on(); }

    private sealed class TestVM : CRUDPersonUCViewModel
    {
        public TestVM(IStatusInfoServices status, IAppMessageService msg, IPersonRepository repo)
            : base(status, msg, repo)
        {
        }

        // Expose protected hooks for testing
        public new Task<Person> OnAddFormAsync() => base.OnAddFormAsync();
        public new Task OnLoadFormAsync(Person entity) => base.OnLoadFormAsync(entity);
        public new Task OnResetFormAsync() => base.OnResetFormAsync();
        public new Task OnSaveFormAsync() => base.OnSaveFormAsync();

        // Expose protected refresh helper so tests can exercise exception handling and concurrency
        public void CallRefreshCommandStates() => base.RefreshCommandStates();
    }

    [TestMethod]
    public async Task OnAddFormAsync_Creates_Person_From_Fields()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        // set fields
        vm.Id = Guid.Empty; // should become new id
        vm.RoleId = Guid.NewGuid();
        vm.AddressId = Guid.NewGuid();
        vm.FirstName = "FN";
        vm.LastName = "LN";
        vm.Email = "e@x.com";
        vm.IsActive = true;

        Person p = await vm.OnAddFormAsync();

        Assert.IsNotNull(p);
        Assert.AreEqual("FN", p.FirstName);
        Assert.AreEqual("LN", p.LastName);
        Assert.AreEqual("e@x.com", p.Email);
        Assert.AreEqual(true, p.IsActive);
        Assert.AreEqual(vm.RoleId, p.RoleId);
        Assert.AreEqual(vm.AddressId, p.AddressId);
        Assert.AreNotEqual(Guid.Empty, p.Id);
    }

    [TestMethod]
    public async Task OnLoadFormAsync_Populates_ViewModel_Fields()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        Person person = new Person { Id = Guid.NewGuid(), RoleId = Guid.NewGuid(), AddressId = Guid.NewGuid(), FirstName = "A", LastName = "B", Email = "x@y", IsActive = true };

        await vm.OnLoadFormAsync(person);

        Assert.AreEqual(person.Id, vm.Id);
        Assert.AreEqual(person.RoleId, vm.RoleId);
        Assert.AreEqual(person.AddressId, vm.AddressId);
        Assert.AreEqual(person.FirstName, vm.FirstName);
        Assert.AreEqual(person.LastName, vm.LastName);
        Assert.AreEqual(person.Email, vm.Email);
        Assert.AreEqual(person.IsActive, vm.IsActive);
        Assert.AreSame(person, vm.SelectedItem);
    }

    [TestMethod]
    public async Task OnResetFormAsync_Clears_Fields_And_SelectedItem()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        vm.Id = Guid.NewGuid();
        vm.FirstName = "X";
        vm.SelectedItem = new Person { Id = Guid.NewGuid() };

        await vm.OnResetFormAsync();

        Assert.AreEqual(Guid.Empty, vm.Id);
        Assert.AreEqual(string.Empty, vm.FirstName);
        Assert.IsNull(vm.SelectedItem);
    }

    [TestMethod]
    public async Task OnSaveFormAsync_Adds_When_In_AddMode_And_Updates_When_EditMode()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        // --- Add mode (IsEditMode false)
        vm.Id = Guid.Empty;
        vm.FirstName = "New";
        vm.RoleId = Guid.NewGuid();
        vm.AddressId = Guid.NewGuid();
        vm.IsActive = true;

        // Call save form directly (protected) which should call repo.AddAsync
        await vm.OnSaveFormAsync();

        mockRepo.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsNotNull(vm.SelectedItem);

        // --- Update mode
        mockRepo.Invocations.Clear();

        Person existing = new Person { Id = Guid.NewGuid(), FirstName = "Old", LastName = "L", Email = "o@x", RoleId = Guid.Empty, AddressId = Guid.Empty, IsActive = false };
        vm.SelectedItem = existing;

        // mark edit mode by loading the entity first (sets SelectedItem and IsEditMode)
        await vm.OnLoadFormAsync(existing);

        // set edit fields AFTER loading
        vm.RoleId = Guid.NewGuid();
        vm.AddressId = Guid.NewGuid();
        vm.FirstName = "Updated";
        vm.LastName = "UpdatedL";
        vm.Email = "u@x";
        vm.IsActive = true;

        await vm.OnSaveFormAsync();

        mockRepo.Verify(r => r.UpdateAsync(
            It.Is<Person>(p => p.Id == existing.Id &&
                               p.FirstName == "Updated" &&
                               p.LastName == "UpdatedL" &&
                               p.Email == "u@x"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void RefreshCommandStates_Swallows_COMException_And_Continues()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        bool saveInvoked = false;

        // Add handler that throws COMException
        vm.AddCommand.CanExecuteChanged += (s, e) => throw new COMException();
        // Save handler should still be invoked
        vm.SaveCommand.CanExecuteChanged += (s, e) => saveInvoked = true;

        // Should not throw even if one handler throws COMException
        vm.CallRefreshCommandStates();

        Assert.IsTrue(saveInvoked, "Save command's CanExecuteChanged handler should be invoked even when another handler throws COMException.");
    }

    [TestMethod]
    public void RefreshCommandStates_Is_ThreadSafe_Under_Concurrent_Calls()
    {
        Mock<IStatusInfoServices> mockStatus = new Mock<IStatusInfoServices>();
        mockStatus.Setup(s => s.BeginLoading()).Returns(new DisposableAction(() => { }));
        Mock<IAppMessageService> mockMsg = new Mock<IAppMessageService>();
        Mock<IPersonRepository> mockRepo = new Mock<IPersonRepository>();

        TestVM vm = new TestVM(mockStatus.Object, mockMsg.Object, mockRepo.Object);

        int invokeCount = 0;
        vm.AddCommand.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invokeCount);
        vm.SaveCommand.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invokeCount);
        vm.DeleteCommand.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invokeCount);
        vm.CancelCommand.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invokeCount);
        vm.RefreshCommand.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invokeCount);

        // Run multiple concurrent callers to stress potential race conditions
        const int threads = 8;
        const int iterations = 50;
        var tasks = new List<Task>();
        for (int t = 0; t < threads; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    vm.CallRefreshCommandStates();
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Expect that handlers have been invoked at least once
        Assert.IsTrue(invokeCount > 0, "Handlers should have been invoked during concurrent RefreshCommandStates calls.");
    }
}
