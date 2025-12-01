using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed partial class CRUDViewModelBaseTests
{
    private sealed class TestEntity { public Guid Id { get; set; } }

    private sealed class FakeRepo : IRepository<TestEntity>
    {
        public Task AddAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<TestEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<TestEntity>>([]);
        public Task<TestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TestEntity?>(null);
        public Task UpdateAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed partial class TestCrudViewModel(
        IStatusInfoServices status,
        IAppMessageService appMsg,
        FakeRepo repo,
        bool autoLoad = false)
        : CRUDViewModelBase<FakeRepo, TestEntity>(status, appMsg, repo, autoLoad)
    {
        public bool AddInvoked { get; private set; }
        public bool SaveInvoked { get; private set; }
        public bool DeleteInvoked { get; private set; }
        public bool ResetFormInvoked { get; private set; }

        protected override Task OnResetFormAsync()
        {
            ResetFormInvoked = true;
            return Task.CompletedTask;
        }
        protected override Task<TestEntity> OnAddFormAsync() => Task.FromResult(new TestEntity());
        protected override Task OnSaveFormAsync() => Task.CompletedTask;
        protected override Task OnLoadFormAsync(TestEntity entity) => Task.CompletedTask;

        protected override async Task OnAddAsync()
        {
            await base.OnAddAsync();
            AddInvoked = true;
        }

        protected override async Task OnSaveAsync()
        {
            // mark that save was invoked
            SaveInvoked = true;
            await Task.CompletedTask;
        }

        protected override async Task OnDeleteAsync()
        {
            DeleteInvoked = true;
            await Task.CompletedTask;
        }

        // Expose protected RefreshCommandStates to tests
        public void ExposedRefreshCommandStates() => RefreshCommandStates();

        // Expose protected CanXXX methods for testing
        public bool ExposedCanSubmitCore() => CanSubmitCore();
        public bool ExposedCanAdd() => CanAdd();
        public bool ExposedCanSave() => CanSave();
        public bool ExposedCanDelete() => CanDelete();

        // Allow tests to set protected state
        public void ExposedSetIsSaving(bool val) => IsSaving = val;
        public void ExposedSetIsEditMode(bool val) => IsEditMode = val;
    }

    private static async Task<bool> WaitForAsync(Func<bool> predicate, int timeoutMs = 1000)
    {
        Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (predicate()) return true;
            await Task.Delay(5);
        }
        return false;
    }

    [TestMethod]
    public void RefreshCommandStates_HandlerThrows_COMException_IsSwallowed()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        int called = 0;

        // First handler throws COMException
        (vm.AddCommand as RelayCommand)!.CanExecuteChanged += (s, e) => throw new COMException("UI COM error");
        // Second handler should still be invoked
        (vm.AddCommand as RelayCommand)!.CanExecuteChanged += (s, e) => called++;

        // Act
        vm.ExposedRefreshCommandStates();

        // Assert
        Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void RefreshCommandStates_MultiThreaded_InvokesAllHandlers_ThreadSafe()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        const int handlerCount = 50;
        const int threads = 8;
        const int callsPerThread = 200;

        int totalCalls = 0;

        // register handlers that increment a shared counter in a thread-safe way
        for (int i = 0; i < handlerCount; i++)
        {
            (vm.AddCommand as RelayCommand)!.CanExecuteChanged += (s, e) => System.Threading.Interlocked.Increment(ref totalCalls);
        }

        // Fire RefreshCommandStates from multiple threads concurrently
        Task[] tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int c = 0; c < callsPerThread; c++)
                {
                    vm.ExposedRefreshCommandStates();
                }
            }, TestContext.CancellationToken);
        }

        Task.WaitAll(tasks, TestContext.CancellationToken);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls, $"Expected {expected} handler invocations, got {totalCalls}");
    }

    [TestMethod]
    public async Task AddCommand_Executes_OnAddAsync_And_Sets_IsSaving()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        // Ensure we are in Add mode
        Assert.IsTrue(vm.IsAddMode);
        Assert.IsFalse(vm.IsSaving);

        // Act
        Assert.IsTrue(vm.AddCommand.CanExecute(null));
        vm.AddCommand.Execute(null);

        // Wait until OnAddAsync set the flag or timeout
        bool invoked = await WaitForAsync(() => vm.AddInvoked, 500);

        Assert.IsTrue(invoked, "OnAddAsync should have been invoked by AddCommand");
        Assert.IsTrue(vm.IsSaving, "IsSaving should be true after AddCommand starts.");
    }

    [TestMethod]
    public async Task SaveCommand_Executes_OnSaveAsync_When_In_EditMode()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        // Put into edit mode so Save is allowed
        vm.ExposedSetIsEditMode(true);
        Assert.IsTrue(vm.SaveCommand.CanExecute(null));

        vm.SaveCommand.Execute(null);

        bool invoked = await WaitForAsync(() => vm.SaveInvoked, 500);
        Assert.IsTrue(invoked, "OnSaveAsync should have been invoked by SaveCommand");
    }

    [TestMethod]
    public async Task DeleteCommand_Executes_OnDeleteAsync_When_In_EditMode()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        vm.ExposedSetIsEditMode(true);
        Assert.IsTrue(vm.DeleteCommand.CanExecute(null));

        vm.DeleteCommand.Execute(null);

        bool invoked = await WaitForAsync(() => vm.DeleteInvoked, 500);
        Assert.IsTrue(invoked, "OnDeleteAsync should have been invoked by DeleteCommand");
    }

    [TestMethod]
    public async Task CancelCommand_Executes_OnResetFormAsync_And_Clears_Errors_And_Selection()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false)
        {
            // Simulate an existing selection and an error message
            SelectedItem = new TestEntity()
        };
        appMsg.AddErrorMessage("err");
        vm.ExposedSetIsEditMode(true);

        Assert.IsTrue(vm.CancelCommand.CanExecute(null));

        vm.CancelCommand.Execute(null);

        bool resetInvoked = await WaitForAsync(() => vm.ResetFormInvoked, 500);
        Assert.IsTrue(resetInvoked, "OnResetFormAsync should have been invoked by CancelCommand");

        // SelectedItem should be cleared and edit mode disabled
        Assert.IsNull(vm.SelectedItem);
        Assert.IsFalse(vm.IsEditMode);

        // Error messages should be cleared
        Assert.IsFalse(appMsg.HasErrorMessages);
    }

    [TestMethod]
    public void RefreshCommand_Invokes_RefreshCommandStates_And_Raises_CanExecuteChanged_On_Commands()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();

        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        int called = 0;
        (vm.AddCommand as RelayCommand)!.CanExecuteChanged += (s, e) => called++;

        // Execute the refresh command which should call RefreshCommandStates
        Assert.IsTrue(vm.RefreshCommand.CanExecute(null));
        vm.RefreshCommand.Execute(null);

        // Handler should have been invoked at least once
        Assert.IsGreaterThan(0, called, "RefreshCommand should cause CanExecuteChanged to be raised on child commands.");
    }

    [TestMethod]
    public void CanSubmitCore_Returns_True_When_NotSaving_And_NoErrors()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        Assert.IsFalse(vm.IsSaving);
        Assert.IsFalse(appMsg.HasErrorMessages);
        Assert.IsTrue(vm.ExposedCanSubmitCore());
    }

    [TestMethod]
    public void CanSubmitCore_Returns_False_When_HasErrors()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        appMsg.AddErrorMessage("err");
        Assert.IsFalse(vm.ExposedCanSubmitCore());
    }

    [TestMethod]
    public void CanSubmitCore_Returns_False_When_Saving()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        vm.ExposedSetIsSaving(true);
        Assert.IsFalse(vm.ExposedCanSubmitCore());
    }

    [TestMethod]
    public void CanAdd_Respects_AddMode_And_Errors_And_Saving()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        // Default is Add mode
        vm.ExposedSetIsEditMode(false);
        vm.ExposedSetIsSaving(false);
        Assert.IsTrue(vm.ExposedCanAdd());

        // When in edit mode, cannot add
        vm.ExposedSetIsEditMode(true);
        Assert.IsFalse(vm.ExposedCanAdd());

        // When errors exist, cannot add
        vm.ExposedSetIsEditMode(false);
        appMsg.AddErrorMessage("err");
        Assert.IsFalse(vm.ExposedCanAdd());
    }

    [TestMethod]
    public void CanSave_Respects_EditMode_And_Errors()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        // Not in edit mode -> cannot save
        vm.ExposedSetIsEditMode(false);
        Assert.IsFalse(vm.ExposedCanSave());

        // In edit mode and no errors -> can save
        vm.ExposedSetIsEditMode(true);
        Assert.IsTrue(vm.ExposedCanSave());

        // When errors exist -> cannot save
        appMsg.AddErrorMessage("err");
        Assert.IsFalse(vm.ExposedCanSave());
    }

    [TestMethod]
    public void CanDelete_Respects_EditMode_And_Saving()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();
        FakeRepo repo = new();
        TestCrudViewModel vm = new(status, appMsg, repo, autoLoad: false);

        // Not in edit mode -> cannot delete
        vm.ExposedSetIsEditMode(false);
        Assert.IsFalse(vm.ExposedCanDelete());

        // In edit mode and not saving -> can delete
        vm.ExposedSetIsEditMode(true);
        vm.ExposedSetIsSaving(false);
        Assert.IsTrue(vm.ExposedCanDelete());

        // When saving -> cannot delete
        vm.ExposedSetIsSaving(true);
        Assert.IsFalse(vm.ExposedCanDelete());
    }

    public TestContext TestContext { get; set; }
}
