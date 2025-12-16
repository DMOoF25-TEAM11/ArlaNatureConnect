using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Controls.SharedUC;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public class CRUDPersonUCViewModelTests
{
    private sealed class FakePersonRepo : IPersonRepository
    {
        public List<Person> Store { get; } = [];

        public Task<Person> AddAsync(Person entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            Store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<Person>> AddRangeAsync(IEnumerable<Person> entities, CancellationToken cancellationToken = default)
        {
            Store.AddRange(entities);
            return Task.FromResult<IEnumerable<Person>>(entities);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Store.RemoveAll(p => p.Id == id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Person>>([.. Store]);
        }

        public Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Store.FirstOrDefault(p => p.Id == id));
        }

        public Task UpdateAsync(Person entity, CancellationToken cancellationToken = default)
        {
            int idx = Store.FindIndex(p => p.Id == entity.Id);
            if (idx >= 0) Store[idx] = entity;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Person>> GetPersonsByRoleAsync(string role, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Person>>([]);

        public Task<Person?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeAddressRepo : IAddressRepository
    {
        public List<Address> Store { get; } = [];
        public Task<Address> AddAsync(Address entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            Store.Add(entity);
            return Task.FromResult(entity);
        }
        public Task<IEnumerable<Address>> AddRangeAsync(IEnumerable<Address> entities, CancellationToken cancellationToken = default)
        {
            Store.AddRange(entities);
            return Task.FromResult<IEnumerable<Address>>(entities);
        }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Store.RemoveAll(a => a.Id == id);
            return Task.CompletedTask;
        }
        public Task<IEnumerable<Address>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Address>>([.. Store]);
        public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Store.FirstOrDefault(a => a.Id == id));
        public Task UpdateAsync(Address entity, CancellationToken cancellationToken = default)
        {
            int idx = Store.FindIndex(a => a.Id == entity.Id);
            if (idx >= 0) Store[idx] = entity;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRoleRepo : IRoleRepository
    {
        public List<Role> Store { get; } = [];
        public Task<Role> AddAsync(Role entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            Store.Add(entity);
            return Task.FromResult(entity);
        }
        public Task<IEnumerable<Role>> AddRangeAsync(IEnumerable<Role> entities, CancellationToken cancellationToken = default)
        {
            Store.AddRange(entities);
            return Task.FromResult<IEnumerable<Role>>(entities);
        }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Store.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }
        public Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Role>>([.. Store]);
        public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Store.FirstOrDefault(r => r.Id == id));
        public Task UpdateAsync(Role entity, CancellationToken cancellationToken = default)
        {
            int idx = Store.FindIndex(r => r.Id == entity.Id);
            if (idx >= 0) Store[idx] = entity;
            return Task.CompletedTask;
        }
        public Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default) => Task.FromResult(Store.FirstOrDefault(r => r.Name == roleName));
    }

    private static async Task<bool> WaitForAsync(Func<bool> predicate, int timeoutMs = 1000)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (predicate()) return true;
            await Task.Delay(5);
        }
        return false;
    }

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void PopulateFormFromPerson_Sets_Fields_And_SelectedRole_When_Role_Matches()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        Role r = new() { Id = Guid.NewGuid(), Name = "Farmer" };
        roleRepo.Store.Add(r);

        Person p = new()
        {
            Id = Guid.NewGuid(),
            RoleId = r.Id,
            AddressId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "a@b.com",
            IsActive = true,
            Role = r,
            Address = new Address { Id = Guid.NewGuid(), City = "C", PostalCode = "P", Street = "S", Country = "DK" }
        };

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

        vm.PopulateFormFromPerson(p);

        Assert.AreEqual(p.FirstName, vm.FirstName);
        Assert.AreEqual(p.LastName, vm.LastName);
        Assert.AreEqual(p.Email, vm.Email);
        Assert.AreEqual(p.IsActive, vm.IsActive);
        Assert.AreEqual(p.Address.City, vm.AddressCity);
        Assert.IsNotNull(vm.SelectedRole);
        Assert.AreEqual(r.Id, vm.SelectedRole!.Id);
    }

    [TestMethod]
    public async Task LoadAsync_Sets_SelectedItem_And_Populates_Form()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        Person p = new() { Id = Guid.NewGuid(), FirstName = "L", LastName = "A", Email = "x@x" };
        personRepo.Store.Add(p);

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

        await vm.LoadAsync(p.Id);

        Assert.IsTrue(vm.IsEditMode);
        Assert.AreEqual(p.Id, vm.SelectedItem!.Id);
        Assert.AreEqual(p.FirstName, vm.FirstName);
    }

    [TestMethod]
    public async Task AddCommand_Creates_Address_And_Person_And_Resets_Form()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        // prepare role to satisfy validation
        Role r = new() { Id = Guid.NewGuid(), Name = "Employee" };
        roleRepo.Store.Add(r);

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo)
        {
            // Fill fields for add
            FirstName = "F1",
            LastName = "L1",
            Email = "e@e.com",
            IsActive = true,
            AddressCity = "City",
            AddressStreet = "Street",
            AddressPostalCode = "P",
            RoleId = r.Id,
            SelectedRole = r
        };

        Assert.IsTrue(vm.IsAddMode);
        Assert.IsTrue(vm.AddCommand.CanExecute(null));

        vm.AddCommand.Execute(null);

        bool added = await WaitForAsync(() => personRepo.Store.Count > 0, 1000);
        Assert.IsTrue(added, "Person repository should have an added person");
        Assert.IsNotEmpty(addrRepo.Store, "Address repository should have an added address");

        // form reset should clear important fields
        bool reset = await WaitForAsync(() => vm.FirstName == string.Empty && vm.IsAddMode, 1000);
        Assert.IsTrue(reset, "Form should be reset after add completes");
    }

    //[TestMethod]
    //public async Task SaveCommand_In_EditMode_Updates_Existing_Person()
    //{
    //    StatusInfoService status = new();
    //    AppMessageService appMsg = new();

    //    FakePersonRepo personRepo = new();
    //    FakeAddressRepo addrRepo = new();
    //    FakeRoleRepo roleRepo = new();

    //    Role r = new() { Id = Guid.NewGuid(), Name = "Employee" };
    //    roleRepo.Store.Add(r);

    //    Person p = new() { Id = Guid.NewGuid(), FirstName = "Before", LastName = "X", Email = "a@a.com", IsActive = false };
    //    personRepo.Store.Add(p);

    //    CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

    //    // Load into edit mode
    //    await vm.LoadAsync(p.Id);

    //    vm.FirstName = "After";
    //    vm.SelectedRole = r;
    //    vm.RoleId = r.Id;

    //    Assert.IsTrue(vm.SaveCommand.CanExecute(null));
    //    vm.SaveCommand.Execute(null);

    //    bool updated = await WaitForAsync(() => personRepo.Store.Any(x => x.FirstName == "After"), 1000);
    //    Assert.IsTrue(updated, "Person repository should have been updated");
    //}

    [TestMethod]
    public void ApplySearchFilter_Filters_By_Name_Or_Email()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        Person p1 = new() { Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Smith", Email = "anna@x" };
        Person p2 = new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Email = "b@x" };

        personRepo.Store.Add(p1);
        personRepo.Store.Add(p2);

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo)
        {
            // Ensure initial load happened (constructor triggers load)
            //
            SearchText = "anna"
        };
        Assert.HasCount(1, vm.FilteredItems);
        Assert.AreEqual(p1.Id, vm.FilteredItems.First().Id);

        vm.SearchText = "@x";
        Assert.HasCount(2, vm.FilteredItems);
    }

    [TestMethod]
    public void OnSortExecuted_Sorts_By_Nested_Property()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        Person p1 = new() { Id = Guid.NewGuid(), FirstName = "C" };
        Person p2 = new() { Id = Guid.NewGuid(), FirstName = "A" };
        Person p3 = new() { Id = Guid.NewGuid(), FirstName = "B" };

        personRepo.Store.Add(p1);
        personRepo.Store.Add(p2);
        personRepo.Store.Add(p3);

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

        // sort ascending by FirstName
        vm.SortCommand!.Execute("FirstName");
        Assert.AreEqual("A", vm.Items[0].FirstName);
        Assert.AreEqual("B", vm.Items[1].FirstName);
        Assert.AreEqual("C", vm.Items[2].FirstName);

        // sort descending by invoking again
        vm.SortCommand.Execute("FirstName");
        Assert.AreEqual("C", vm.Items[0].FirstName);
    }

    [TestMethod]
    public void UpdateSelectedPersonFarms_Populates_SelectedPersonFarms()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        Person p = new() { Id = Guid.NewGuid(), FirstName = "F" };
        p.Farms.Add(new Farm { Id = Guid.NewGuid(), Name = "Farm1" });
        personRepo.Store.Add(p);

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo)
        {
            // select item
            SelectedItem = p
        };

        // trigger selection changed handler indirectly
        // the viewmodel subscribes to SelectedEntityChanged; setting SelectedItem should have updated farms
        Assert.HasCount(1, vm.SelectedPersonFarms);
        Assert.AreEqual("Farm1", vm.SelectedPersonFarms.First().Name);
    }

    [TestMethod]
    public void RefreshCommandStates_HandlerThrows_COMException_IsSwallowed_In_PersonVM()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

        int called = 0;
        (vm.AddCommand as global::ArlaNatureConnect.WinUI.Commands.RelayCommand)!.CanExecuteChanged += (s, e) => throw new COMException("UI COM error");
        (vm.AddCommand as global::ArlaNatureConnect.WinUI.Commands.RelayCommand)!.CanExecuteChanged += (s, e) => called++;

        vm.RefreshCommand.Execute(null);

        Assert.AreEqual(1, called);
    }

    [TestMethod]
    public void RefreshCommandStates_MultiThreaded_Invokes_All_Handlers_ThreadSafe_In_PersonVM()
    {
        StatusInfoService status = new();
        AppMessageService appMsg = new();

        FakePersonRepo personRepo = new();
        FakeAddressRepo addrRepo = new();
        FakeRoleRepo roleRepo = new();

        CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo);

        const int handlerCount = 40;
        const int threads = 6;
        const int callsPerThread = 100;

        int totalCalls = 0;

        for (int i = 0; i < handlerCount; i++)
        {
            (vm.AddCommand as global::ArlaNatureConnect.WinUI.Commands.RelayCommand)!.CanExecuteChanged += (s, e) => System.Threading.Interlocked.Increment(ref totalCalls);
        }

        Task[] tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int c = 0; c < callsPerThread; c++)
                {
                    vm.RefreshCommand.Execute(null);
                }
            }, TestContext.CancellationToken);
        }

        Task.WaitAll(tasks, TestContext.CancellationToken);

        int expected = handlerCount * threads * callsPerThread;
        Assert.AreEqual(expected, totalCalls);
    }
}
