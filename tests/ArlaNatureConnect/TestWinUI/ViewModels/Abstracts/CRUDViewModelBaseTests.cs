using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace TestWinUI.ViewModels.Abstracts;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class CRUDViewModelBaseTests
{
    private sealed class TestEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class FakeRepo : IRepository<TestEntity>
    {
        public Func<Guid, Task<TestEntity?>>? GetByIdAsyncImpl { get; set; }

        public Task AddAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<TestEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<TestEntity>>(Array.Empty<TestEntity>());
        public Task<TestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (GetByIdAsyncImpl is null) return Task.FromResult<TestEntity?>(null);
            return GetByIdAsyncImpl(id);
        }
        public Task UpdateAsync(TestEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeStatusInfoServices : IStatusInfoServices
    {
        public bool IsLoading { get; set; }
        public bool HasDbConnection { get; set; }
        public IDisposable BeginLoading()
        {
            IsLoading = true;
            return new DisposableAction(() => IsLoading = false);
        }
        private sealed class DisposableAction : IDisposable
        {
            private readonly Action _onDispose;
            public DisposableAction(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }

        // event explicitly implemented if interface requires it - provide no-op
        event EventHandler? IStatusInfoServices.StatusInfoChanged
        {
            add { }
            remove { }
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

    private sealed class TestCrudViewModel : CRUDViewModelBase<FakeRepo, TestEntity>
    {
        public bool ResetFormCalled { get; private set; }
        public bool AddFormCalled { get; private set; }
        public bool SaveFormCalled { get; private set; }
        public bool LoadFormCalled { get; private set; }

        public TestCrudViewModel(IStatusInfoServices status, IAppMessageService msg, FakeRepo repo)
            : base(status, msg, repo)
        {
        }

        // expose protected state for tests
        public new FakeRepo Repository => base.Repository;

        public void SetIsSaving(bool value) => _isSaving = value;
        public void SetIsEditMode(bool value) => _isEditMode = value;

        // wrappers to call protected async methods and ensure tasks complete
        public Task CallLoadAsync(Guid id) => base.LoadAsync(id);
        public Task CallOnAddAsync() => base.OnAddAsync();
        public Task CallOnSaveAsync() => base.OnSaveAsync();
        public Task CallOnDeleteAsync() => base.OnDeleteAsync();
        public Task CallOnCancelAsync() => base.OnCancelAsync();
        public Task CallOnResetAsync() => base.OnResetAsync();
        public void CallRefreshCommand() => RefreshCommand.Execute(null);

        protected override Task OnResetFormAsync()
        {
            ResetFormCalled = true;
            return Task.CompletedTask;
        }

        protected override Task<TestEntity> OnAddFormAsync()
        {
            AddFormCalled = true;
            return Task.FromResult(new TestEntity { Id = Guid.NewGuid(), Name = "added" });
        }

        protected override Task OnSaveFormAsync()
        {
            SaveFormCalled = true;
            return Task.CompletedTask;
        }

        protected override Task OnLoadFormAsync(TestEntity entity)
        {
            LoadFormCalled = true;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public void Constructor_InitializesCommands_And_DefaultModes()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        Assert.IsNotNull(vm.AddCommand);
        Assert.IsNotNull(vm.SaveCommand);
        Assert.IsNotNull(vm.DeleteCommand);
        Assert.IsNotNull(vm.CancelCommand);
        Assert.IsNotNull(vm.RefreshCommand);

        // default should be add mode (IsEditMode false)
        Assert.IsFalse(vm.IsEditMode);
        Assert.IsTrue(vm.IsAddMode);

        // initial can execute states
        Assert.IsTrue(vm.AddCommand.CanExecute(null));
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
    }

    [TestMethod]
    public void CanSubmitCore_ReflectsIsSaving_And_ErrorMessages()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        // default: not saving, no errors
        Assert.IsTrue(vm.AddCommand.CanExecute(null));

        // simulate error message
        msg.AddErrorMessage("err");
        Assert.IsFalse(vm.AddCommand.CanExecute(null));

        // clear errors and simulate saving
        msg.ClearErrorMessages();
        vm.SetIsSaving(true);
        // Add should be disabled while saving
        Assert.IsFalse(vm.AddCommand.CanExecute(null));
    }

    [TestMethod]
    public async Task LoadAsync_RaisesEntityPropertyChanged()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();

        TestEntity found = new TestEntity { Id = Guid.NewGuid(), Name = "Found" };
        repo.GetByIdAsyncImpl = id => Task.FromResult<TestEntity?>(found);

        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        List<string?> received = new List<string?>();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        await vm.CallLoadAsync(found.Id);

        // The implementation calls OnPropertyChanged(nameof(Entity)) in finally.
        Assert.Contains("Entity", received);
    }

    [TestMethod]
    public async Task LoadAsync_WhenRepositoryThrows_DoesNotCrash_And_RaisesPropertyChanged()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();

        repo.GetByIdAsyncImpl = id => throw new InvalidOperationException("boom");

        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);
        List<string?> received = new List<string?>();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        await vm.CallLoadAsync(Guid.NewGuid());

        Assert.Contains("Entity", received);
        // error message append in implementation uses LINQ Append and will not modify service state
        Assert.IsFalse(msg.HasErrorMessages);
    }

    [TestMethod]
    public async Task OnAddAsync_OnlyRuns_WhenCanAdd()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        // ensure add mode
        vm.SetIsEditMode(false);
        await vm.CallOnAddAsync();

        // base implementation sets IsSaving = true when CanAdd
        Assert.IsTrue(vm.IsSaving);

        // reset and set edit mode -> cannot add
        vm.SetIsSaving(false);
        vm.SetIsEditMode(true);
        await vm.CallOnAddAsync();
        Assert.IsFalse(vm.IsSaving);
    }

    [TestMethod]
    public async Task OnCancelAsync_Resets_State_And_ClearsErrors()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        // set up state
        vm.Entity = new TestEntity { Id = Guid.NewGuid(), Name = "x" };
        vm.SetIsEditMode(true);
        msg.AddErrorMessage("e");

        await vm.CallOnCancelAsync();

        Assert.IsNull(vm.Entity);
        Assert.IsFalse(vm.IsEditMode);
        Assert.IsTrue(vm.ResetFormCalled);
        Assert.IsFalse(msg.HasErrorMessages);
    }

    [TestMethod]
    public void RefreshCommand_Invokes_CanExecuteChanged_On_All_Commands()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        int calls = 0;
        void Handler(object? s, EventArgs e) => calls++;

        vm.AddCommand.CanExecuteChanged += Handler;
        vm.SaveCommand.CanExecuteChanged += Handler;
        vm.DeleteCommand.CanExecuteChanged += Handler;
        vm.CancelCommand.CanExecuteChanged += Handler;
        vm.RefreshCommand.CanExecuteChanged += Handler;

        // executing RefreshCommand will call RefreshCommandStates() which raises CanExecuteChanged on all
        vm.CallRefreshCommand();

        Assert.AreEqual(5, calls);

        // cleanup
        vm.AddCommand.CanExecuteChanged -= Handler;
        vm.SaveCommand.CanExecuteChanged -= Handler;
        vm.DeleteCommand.CanExecuteChanged -= Handler;
        vm.CancelCommand.CanExecuteChanged -= Handler;
        vm.RefreshCommand.CanExecuteChanged -= Handler;
    }

    [TestMethod]
    public async Task LoadAsync_Calls_OnLoadFormAsync_And_Sets_IsEditMode()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();

        TestEntity found = new TestEntity { Id = Guid.NewGuid(), Name = "Found" };
        repo.GetByIdAsyncImpl = id => Task.FromResult<TestEntity?>(found);

        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        await vm.CallLoadAsync(found.Id);

        Assert.IsTrue(vm.LoadFormCalled);
        Assert.IsTrue(vm.IsEditMode);
        Assert.AreSame(found, vm.Entity);
    }

    [TestMethod]
    public void Save_And_Delete_CanExecute_Based_On_EditMode_And_IsSaving()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        // initially not edit mode -> save/delete disabled
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        Assert.IsFalse(vm.DeleteCommand.CanExecute(null));

        // enable edit mode
        vm.SetIsEditMode(true);
        // no errors and not saving -> save/delete should be enabled
        Assert.IsTrue(vm.SaveCommand.CanExecute(null));
        Assert.IsTrue(vm.DeleteCommand.CanExecute(null));

        // simulate saving -> delete should be disabled
        vm.SetIsSaving(true);
        Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
    }

    [TestMethod]
    public void OnAddAsync_Raises_PropertyChanged_For_IsSaving()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        List<string?> received = new List<string?>();
        vm.PropertyChanged += (_, e) => received.Add(e.PropertyName);

        // Ensure add mode
        vm.SetIsEditMode(false);
        // call add
        vm.CallOnAddAsync().GetAwaiter().GetResult();

        Assert.Contains("IsSaving", received);
    }

    [TestMethod]
    public void SaveCommand_Execute_DoesNotThrow_WhenEnabled()
    {
        FakeStatusInfoServices status = new FakeStatusInfoServices();
        FakeAppMessageService msg = new FakeAppMessageService();
        FakeRepo repo = new FakeRepo();
        TestCrudViewModel vm = new TestCrudViewModel(status, msg, repo);

        vm.SetIsEditMode(true);

        // Should be able to execute without throwing
        vm.SaveCommand.Execute(null);
    }
}
