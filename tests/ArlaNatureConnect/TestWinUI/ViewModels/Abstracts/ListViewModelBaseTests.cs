using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
public sealed class ListViewModelBaseTests
{
    private sealed class TestEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class FakeRepo : IRepository<TestEntity>
    {
        public Func<Guid, Task<TestEntity?>>? GetByIdAsyncImpl { get; set; }

        public Task AddAsync(TestEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TestEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (GetByIdAsyncImpl is null) return Task.FromResult<TestEntity?>(null);
            // If implementation throws, let exception propagate to simulate repo exception
            return GetByIdAsyncImpl(id);
        }

        public Task UpdateAsync(TestEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeStatusInfoServices : IStatusInfoServices
    {
        public bool IsLoading { get; set; }

        public bool HasDbConnection { get; set; }

        public int BeginLoadingCalls { get; private set; }

        // event explicitly implemented to match interface
        event EventHandler? IStatusInfoServices.StatusInfoChanged
        {
            add { }
            remove { }
        }

        public IDisposable BeginLoading()
        {
            BeginLoadingCalls++;
            IsLoading = true;
            return new DisposableAction(() => IsLoading = false);
        }

        private sealed class DisposableAction : IDisposable
        {
            private readonly Action _onDispose;
            public DisposableAction(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }
    }

    private sealed class FakeAppMessageService : IAppMessageService
    {
        private readonly List<string> _info = new();
        private readonly List<string> _errors = new();

        public string? EntityName { get; set; }

        public bool HasStatusMessages => _info.Any();

        public bool HasErrorMessages => _errors.Any();

        public IEnumerable<string> StatusMessages => _info.ToArray();

        public IEnumerable<string> ErrorMessages => _errors.ToArray();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void AddInfoMessage(string message)
        {
            _info.Add(message);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessages)));
        }

        public void AddErrorMessage(string message)
        {
            _errors.Add(message);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessages)));
        }

        public void ClearErrorMessages()
        {
            _errors.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessages)));
        }
    }

    private sealed class TestListViewModel : ListViewModelBase<FakeRepo, TestEntity>
    {
        public TestListViewModel(IStatusInfoServices status, IAppMessageService msg, FakeRepo repo)
            : base(status, msg, repo)
        {
        }

        // expose protected members for assertions in tests
        public new FakeRepo Repository => base.Repository;
    }

    [TestMethod]
    public void Entity_Set_RaisesPropertyChanged()
    {
        var status = new FakeStatusInfoServices();
        var msg = new FakeAppMessageService();
        var repo = new FakeRepo();
        var vm = new TestListViewModel(status, msg, repo);

        var received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e.PropertyName);

        var ent = new TestEntity { Id = Guid.NewGuid(), Name = "X" };
        vm.Entity = ent;

        Assert.AreEqual(1, received.Count);
        Assert.AreEqual("Entity", received[0]);
        Assert.AreSame(ent, vm.Entity);
    }

    [TestMethod]
    public void Repository_Returns_Injected_Instance()
    {
        var status = new FakeStatusInfoServices();
        var msg = new FakeAppMessageService();
        var repo = new FakeRepo();
        var vm = new TestListViewModel(status, msg, repo);

        Assert.AreSame(repo, vm.Repository);
    }

    [TestMethod]
    public async Task LoadAsync_WhenEntityFound_SetsEntity_And_UsesStatus()
    {
        var status = new FakeStatusInfoServices();
        var msg = new FakeAppMessageService();
        var repo = new FakeRepo();

        var found = new TestEntity { Id = Guid.NewGuid(), Name = "Found" };
        repo.GetByIdAsyncImpl = id => Task.FromResult<TestEntity?>(found);

        var vm = new TestListViewModel(status, msg, repo);

        await vm.LoadAsync(found.Id);

        Assert.AreSame(found, vm.Entity);
        Assert.IsFalse(status.IsLoading, "BeginLoading should be disposed after LoadAsync completes");
        Assert.AreEqual(1, status.BeginLoadingCalls);
        Assert.IsFalse(msg.HasErrorMessages);
    }

    [TestMethod]
    public async Task LoadAsync_WhenEntityNotFound_AddsErrorMessage()
    {
        var status = new FakeStatusInfoServices();
        var msg = new FakeAppMessageService();
        var repo = new FakeRepo();

        repo.GetByIdAsyncImpl = id => Task.FromResult<TestEntity?>(null);

        var vm = new TestListViewModel(status, msg, repo);

        await vm.LoadAsync(Guid.NewGuid());

        Assert.IsNull(vm.Entity);
        Assert.IsTrue(msg.HasErrorMessages);
        var errors = msg.ErrorMessages.ToList();
        Assert.AreEqual(1, errors.Count);
        StringAssert.StartsWith(errors[0], "Failed to load entity:");
        StringAssert.Contains(errors[0], "Entity Entity not found");
        Assert.IsFalse(status.IsLoading);
    }

    [TestMethod]
    public async Task LoadAsync_WhenRepositoryThrows_AddsErrorMessage()
    {
        var status = new FakeStatusInfoServices();
        var msg = new FakeAppMessageService();
        var repo = new FakeRepo();

        repo.GetByIdAsyncImpl = id => throw new InvalidOperationException("boom");

        var vm = new TestListViewModel(status, msg, repo);

        await vm.LoadAsync(Guid.NewGuid());

        Assert.IsNull(vm.Entity);
        Assert.IsTrue(msg.HasErrorMessages);
        var errors = msg.ErrorMessages.ToList();
        Assert.AreEqual(1, errors.Count);
        StringAssert.StartsWith(errors[0], "Failed to load entity:");
        StringAssert.Contains(errors[0], "boom");
        Assert.IsFalse(status.IsLoading);
    }
}
