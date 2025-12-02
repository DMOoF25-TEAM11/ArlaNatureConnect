using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Abstract; // Add this using
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Diagnostics;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
public class ListViewModelBaseTests
{
    private sealed class DummyEntity : IEntity // Implement IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
    }

    private sealed class InMemoryRepository : IRepository<DummyEntity>
    {
        private readonly IEnumerable<DummyEntity> _items;
        public InMemoryRepository(IEnumerable<DummyEntity> items) => _items = items;

        public Task AddAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<DummyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<DummyEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_items);
        public Task<DummyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
        public Task UpdateAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ThrowingRepository : IRepository<DummyEntity>
    {
        public Task AddAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<DummyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<DummyEntity>> GetAllAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("repo-failure");
        public Task<DummyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<DummyEntity?>(null);
        public Task UpdateAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DelayedRepository : IRepository<DummyEntity>
    {
        private readonly IEnumerable<DummyEntity> _items;
        private readonly int _delayMs;
        public DelayedRepository(IEnumerable<DummyEntity> items, int delayMs)
        {
            _items = items;
            _delayMs = delayMs;
        }

        public Task AddAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<DummyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public async Task<IEnumerable<DummyEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delayMs, cancellationToken);
            return _items;
        }
        public Task<DummyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
        public Task UpdateAsync(DummyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ConcreteListViewModel : ListViewModelBase<IRepository<DummyEntity>, DummyEntity>
    {
        public ConcreteListViewModel(IStatusInfoServices status, ArlaNatureConnect.Core.Services.IAppMessageService appMsg, IRepository<DummyEntity> repo, bool autoLoad = false)
            : base(status, appMsg, repo, autoLoad)
        {
        }
    }

    [TestMethod]
    public async Task LoadAllAsync_Populates_Items()
    {
        DummyEntity[] items = new[] { new DummyEntity { Name = "a" }, new DummyEntity { Name = "b" } };
        InMemoryRepository repo = new InMemoryRepository(items);
        StatusInfoService status = new StatusInfoService();
        AppMessageService appMsg = new ArlaNatureConnect.Core.Services.AppMessageService();

        ConcreteListViewModel vm = new ConcreteListViewModel(status, appMsg, repo, autoLoad: false);

        await vm.LoadAllAsync();

        Assert.HasCount(2, vm.Items);
        Assert.IsTrue(vm.Items.Any(i => i.Name == "a"));
        Assert.IsTrue(vm.Items.Any(i => i.Name == "b"));
    }

    [TestMethod]
    public async Task LoadAllAsync_WhenRepositoryThrows_ReportsErrorAndDoesNotThrow()
    {
        ThrowingRepository repo = new ThrowingRepository();
        StatusInfoService status = new StatusInfoService();
        AppMessageService appMsg = new ArlaNatureConnect.Core.Services.AppMessageService();

        ConcreteListViewModel vm = new ConcreteListViewModel(status, appMsg, repo, autoLoad: false);

        // Should not throw despite repository throwing; error is reported to AppMessageService
        await vm.LoadAllAsync();

        Assert.IsTrue(appMsg.HasErrorMessages, "AppMessageService should contain an error message when repository fails.");
        StringAssert.Contains(appMsg.ErrorMessages.First(), "Failed to load items");
    }

    [TestMethod]
    public async Task LoadAllAsync_Sets_IsLoading_While_Loading_And_IsThreadSafe()
    {
        DummyEntity[] items = new[] { new DummyEntity { Name = "delayed" } };
        DelayedRepository repo = new DelayedRepository(items, 200);
        StatusInfoService status = new StatusInfoService();
        AppMessageService appMsg = new ArlaNatureConnect.Core.Services.AppMessageService();

        ConcreteListViewModel vm = new ConcreteListViewModel(status, appMsg, repo, autoLoad: false);

        Task loadTask = vm.LoadAllAsync();

        // Wait until the load has started and BeginLoadingOrSaving has set IsLoadingOrSaving = true
        Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        while (!status.IsLoadingOrSaving && sw.ElapsedMilliseconds < 1000)
        {
            await Task.Delay(5);
        }

        Assert.IsTrue(status.IsLoadingOrSaving, "StatusInfoService.IsLoadingOrSaving should be true while the load operation is in progress.");

        await loadTask;

        Assert.IsFalse(status.IsLoadingOrSaving, "StatusInfoService.IsLoadingOrSaving should be false after the load operation completes.");
        Assert.HasCount(1, vm.Items);
        Assert.AreEqual("delayed", vm.Items.First().Name);
    }
}
