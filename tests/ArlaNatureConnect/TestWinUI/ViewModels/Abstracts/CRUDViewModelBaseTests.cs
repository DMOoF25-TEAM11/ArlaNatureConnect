using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using System.Runtime.InteropServices;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
public sealed class CRUDViewModelBaseTests
{
    private sealed class TestEntity { public Guid Id { get; set; } }

    private sealed class FakeRepo : IRepository<TestEntity>
    {
        public Task AddAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<TestEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<TestEntity>>(Array.Empty<TestEntity>());
        public Task<TestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TestEntity?>(null);
        public Task UpdateAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestCrudViewModel : CRUDViewModelBase<FakeRepo, TestEntity>
    {
        public TestCrudViewModel(IStatusInfoServices status, IAppMessageService appMsg, FakeRepo repo, bool autoLoad = false)
            : base(status, appMsg, repo, autoLoad)
        { }

        protected override Task OnResetFormAsync() => Task.CompletedTask;
        protected override Task<TestEntity> OnAddFormAsync() => Task.FromResult(new TestEntity());
        protected override Task OnSaveFormAsync() => Task.CompletedTask;
        protected override Task OnLoadFormAsync(TestEntity entity) => Task.CompletedTask;

        // Expose protected RefreshCommandStates to tests
        public void ExposedRefreshCommandStates() => RefreshCommandStates();
    }

    [TestMethod]
    public void RefreshCommandStates_HandlerThrows_COMException_IsSwallowed()
    {
        var status = new StatusInfoService();
        var appMsg = new AppMessageService();
        var repo = new FakeRepo();

        var vm = new TestCrudViewModel(status, appMsg, repo, autoLoad: false);

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
        var status = new StatusInfoService();
        var appMsg = new AppMessageService();
        var repo = new FakeRepo();

        var vm = new TestCrudViewModel(status, appMsg, repo, autoLoad: false);

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
            });
        }

        Task.WaitAll(tasks);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls, $"Expected {expected} handler invocations, got {totalCalls}");
    }
}
