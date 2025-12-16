using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class FarmRepositoryTest
{
    private static DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static Mock<IDbContextFactory<AppDbContext>> CreateFactory(DbContextOptions<AppDbContext> options)
    {
        Mock<IDbContextFactory<AppDbContext>> mock = new();
        mock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));
        return mock;
    }

    private async Task SeedRolesAndPersons(DbContextOptions<AppDbContext> options, int count = 2)
    {
        Guid roleId = Guid.NewGuid();
        using AppDbContext ctx = new(options);
        ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
        for (int i = 0; i < count; i++)
        {
            Guid addressId = Guid.NewGuid();
            Guid ownerId = Guid.NewGuid();
            ctx.Addresses.Add(new Address { Id = addressId, Street = $"Street{i}", City = $"City{i}", PostalCode = $"{1000 + i}", Country = "DK" });
            ctx.Persons.Add(new Person { Id = ownerId, FirstName = $"Owner{i}", LastName = "Test", Email = $"owner{i}@test.com", RoleId = roleId, AddressId = addressId, IsActive = true });
        }
        await ctx.SaveChangesAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task AddAsync_And_GetByIdAsync_Work()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 1);

        Mock<IDbContextFactory<AppDbContext>> factory = CreateFactory(options);
        FarmRepository repo = new(factory.Object);

        Guid ownerId;
        Guid addressId;
        using (AppDbContext ctx = new(options))
        {
            ownerId = ctx.Persons.First().Id;
            addressId = ctx.Addresses.First().Id;
        }

        Farm farm = new() { Id = Guid.NewGuid(), Name = "RepoFarm", CVR = "98765432", OwnerId = ownerId, AddressId = addressId };
        Farm added = await repo.AddAsync(farm, TestContext.CancellationToken);

        Assert.IsNotNull(added);

        Farm? fetched = await repo.GetByIdAsync(farm.Id, TestContext.CancellationToken);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("RepoFarm", fetched!.Name);
        Assert.AreEqual("98765432", fetched.CVR);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task AddRangeAsync_GetAllAsync_Work()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 2);

        Mock<IDbContextFactory<AppDbContext>> factory = CreateFactory(options);
        FarmRepository repo = new(factory.Object);

        Guid owner1, owner2, addr1, addr2;
        using (AppDbContext ctx = new(options))
        {
            owner1 = ctx.Persons.Skip(0).First().Id;
            owner2 = ctx.Persons.Skip(1).First().Id;
            addr1 = ctx.Addresses.Skip(0).First().Id;
            addr2 = ctx.Addresses.Skip(1).First().Id;
        }

        Farm[] farms =
        [
            new Farm { Id = Guid.NewGuid(), Name = "A", CVR = "A1", OwnerId = owner1, AddressId = addr1 },
            new Farm { Id = Guid.NewGuid(), Name = "B", CVR = "B1", OwnerId = owner2, AddressId = addr2 }
        ];

        await repo.AddRangeAsync(farms, TestContext.CancellationToken);

        List<Farm> all = [.. (await repo.GetAllAsync(TestContext.CancellationToken))];
        Assert.HasCount(2, all);
        CollectionAssert.AreEquivalent(farms.Select(f => f.Name).ToList(), all.Select(f => f.Name).ToList());
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task UpdateAsync_Persists_Changes()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 1);

        Mock<IDbContextFactory<AppDbContext>> factory = CreateFactory(options);
        FarmRepository repo = new(factory.Object);

        Guid ownerId, addrId;
        using (AppDbContext ctx = new(options))
        {
            ownerId = ctx.Persons.First().Id;
            addrId = ctx.Addresses.First().Id;
        }

        Farm farm = new() { Id = Guid.NewGuid(), Name = "Before", CVR = "000", OwnerId = ownerId, AddressId = addrId };
        await repo.AddAsync(farm, TestContext.CancellationToken);

        farm.Name = "After";
        await repo.UpdateAsync(farm, TestContext.CancellationToken);

        Farm? fetched = await repo.GetByIdAsync(farm.Id, TestContext.CancellationToken);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("After", fetched!.Name);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task DeleteAsync_Removes_Entity()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 1);

        Mock<IDbContextFactory<AppDbContext>> factory = CreateFactory(options);
        FarmRepository repo = new(factory.Object);

        Guid ownerId, addrId;
        using (AppDbContext ctx = new(options))
        {
            ownerId = ctx.Persons.First().Id;
            addrId = ctx.Addresses.First().Id;
        }

        Farm farm = new() { Id = Guid.NewGuid(), Name = "ToDelete", CVR = "X", OwnerId = ownerId, AddressId = addrId };
        await repo.AddAsync(farm, TestContext.CancellationToken);

        await repo.DeleteAsync(farm.Id, TestContext.CancellationToken);

        Farm? fetched = await repo.GetByIdAsync(farm.Id, TestContext.CancellationToken);
        Assert.IsNull(fetched);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByCvrAsync_Returns_Farm_And_Trims_Input()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 1);

        Mock<IDbContextFactory<AppDbContext>> factory = CreateFactory(options);
        FarmRepository repo = new(factory.Object);

        Guid ownerId, addrId;
        using (AppDbContext ctx = new(options))
        {
            ownerId = ctx.Persons.First().Id;
            addrId = ctx.Addresses.First().Id;
        }

        Farm farm = new() { Id = Guid.NewGuid(), Name = "FindMe", CVR = "  12345  ", OwnerId = ownerId, AddressId = addrId };
        // Persist CVR without surrounding spaces
        farm.CVR = farm.CVR.Trim();
        await repo.AddAsync(farm, TestContext.CancellationToken);

        Farm? fetched = await repo.GetByCvrAsync(" 12345 ", TestContext.CancellationToken);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("FindMe", fetched!.Name);

        Farm? notFound = await repo.GetByCvrAsync("doesnotexist", TestContext.CancellationToken);
        Assert.IsNull(notFound);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByCvrAsync_Returns_Null_On_Exception_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        FarmRepository repo = new(factoryMock.Object);

        Farm? result = await repo.GetByCvrAsync("123", TestContext.CancellationToken);
        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        FarmRepository repo = new(factoryMock.Object);

        // Methods from base repository should propagate exception
        try
        {
            await repo.GetAllAsync(TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.AddAsync(new Farm { Id = Guid.NewGuid() }, TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.AddRangeAsync([new Farm { Id = Guid.NewGuid() }], TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.GetByIdAsync(Guid.NewGuid(), TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.UpdateAsync(new Farm { Id = Guid.NewGuid() }, TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.DeleteAsync(Guid.NewGuid(), TestContext.CancellationToken);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task GetAllAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 10);

        using (AppDbContext ctx = new(options))
        {
            for (int i = 0; i < 10; i++)
            {
                Person owner = ctx.Persons.Skip(i).First();
                Address addr = ctx.Addresses.Skip(i).First();
                ctx.Farms.Add(new Farm { Id = Guid.NewGuid(), Name = $"F{i}", CVR = i.ToString(), OwnerId = owner.Id, AddressId = addr.Id });
            }
            await ctx.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);
        FarmRepository repo = new(factoryMock.Object);

        IEnumerable<Task<List<Farm>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync(TestContext.CancellationToken)).ToList()));
        List<Farm>[] results = await Task.WhenAll(tasks);

        foreach (List<Farm>? list in results)
        {
            Assert.HasCount(10, list);
        }
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task MultiSession_Concurrent_Adds_Are_Persisted()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 5);

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);

        List<Task> tasks = [];
        int addCount = 20;

        for (int i = 0; i < addCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                FarmRepository repo = new(factoryMock.Object);
                Guid ownerId, addrId;
                using (AppDbContext ctx = new(options))
                {
                    ownerId = ctx.Persons.First().Id;
                    addrId = ctx.Addresses.First().Id;
                }

                Farm f = new() { Id = Guid.NewGuid(), Name = $"Multi{i}", CVR = i.ToString(), OwnerId = ownerId, AddressId = addrId };
                await repo.AddAsync(f, TestContext.CancellationToken);
            }, TestContext.CancellationToken));
        }

        await Task.WhenAll(tasks);

        FarmRepository repoFinal = new(factoryMock.Object);
        List<Farm> all = [.. (await repoFinal.GetAllAsync(TestContext.CancellationToken))];
        Assert.IsGreaterThanOrEqualTo(addCount, all.Count);
    }

    [TestMethod]
    [TestCategory("Benchmark")]
    public async Task Performance_Benchmark_Adds_And_Gets()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedRolesAndPersons(options, 5);

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);
        FarmRepository repo = new(factoryMock.Object);

        int ops = 100;
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < ops; i++)
        {
            Guid ownerId, addrId;
            using (AppDbContext ctx = new(options))
            {
                ownerId = ctx.Persons.First().Id;
                addrId = ctx.Addresses.First().Id;
            }

            Farm f = new() { Id = Guid.NewGuid(), Name = $"P{i}", CVR = i.ToString(), OwnerId = ownerId, AddressId = addrId };
            await repo.AddAsync(f, TestContext.CancellationToken);
            Farm? _ = await repo.GetByIdAsync(f.Id, TestContext.CancellationToken);
        }

        sw.Stop();
        // Ensure operations completed within a reasonable time (example threshold: 5s)
        Assert.IsLessThan(5000, sw.ElapsedMilliseconds, $"Benchmark exceeded: {sw.ElapsedMilliseconds}ms");
    }

    public TestContext TestContext { get; set; }
}
