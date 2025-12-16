using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TestInfrastructure.Repositories;

[TestClass]
public class NatureAreaRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static Mock<IDbContextFactory<AppDbContext>> CreateFactory(DbContextOptions<AppDbContext> options)
    {
        Mock<IDbContextFactory<AppDbContext>> mock = new();
        mock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));
        mock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => Task.FromResult(new AppDbContext(options)));
        return mock;
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task Add_And_Get_NatureArea_Async()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed a farm so FarmId references are sensible
        Guid farmId = Guid.NewGuid();
        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(new Farm { Id = farmId, Name = "SeedFarm", CVR = "000", OwnerId = Guid.Empty, AddressId = Guid.Empty });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);

        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        NatureArea area = new NatureArea
        {
            Id = Guid.NewGuid(),
            FarmId = farmId,
            Name = "TestArea",
            Description = "Desc"
        };

        NatureArea added = await repo.AddAsync(area);

        Assert.IsNotNull(added);
        Assert.AreEqual(area.Id, added.Id);

        NatureArea? fetched = await repo.GetByIdAsync(area.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("TestArea", fetched!.Name);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task AddRange_GetAll_Update_Delete_Works()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid farmId = Guid.NewGuid();
        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(new Farm { Id = farmId, Name = "SeedFarm", CVR = "000", OwnerId = Guid.Empty, AddressId = Guid.Empty });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);

        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        NatureArea a1 = new NatureArea { Id = Guid.NewGuid(), FarmId = farmId, Name = "A1", Description = "d1" };
        NatureArea a2 = new NatureArea { Id = Guid.NewGuid(), FarmId = farmId, Name = "A2", Description = "d2" };

        await repo.AddRangeAsync(new[] { a1, a2 });

        List<NatureArea> all = (await repo.GetAllAsync()).ToList();
        Assert.HasCount(2, all);

        // Update a1
        a1.Name = "A1-Updated";
        await repo.UpdateAsync(a1);

        NatureArea? fetched = await repo.GetByIdAsync(a1.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("A1-Updated", fetched!.Name);

        // Delete a2
        await repo.DeleteAsync(a2.Id);
        NatureArea? afterDelete = await repo.GetByIdAsync(a2.Id);
        Assert.IsNull(afterDelete);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByFarmIdAsync_Returns_Areas_For_Given_Farm()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid farmId = Guid.NewGuid();
        Farm farmEntity = new() { Id = farmId, Name = "FarmX", CVR = "111", OwnerId = Guid.Empty, AddressId = Guid.Empty };

        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(farmEntity);
            seed.NatureAreas.Add(new NatureArea { Id = Guid.NewGuid(), FarmId = farmId, Name = "Area1", Description = "d" });
            seed.NatureAreas.Add(new NatureArea { Id = Guid.NewGuid(), FarmId = farmId, Name = "Area2", Description = "d" });
            // area for other farm
            seed.NatureAreas.Add(new NatureArea { Id = Guid.NewGuid(), FarmId = Guid.NewGuid(), Name = "Other", Description = "d" });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);
        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        IEnumerable<NatureArea> areas = await repo.GetByFarmIdAsync(farmEntity);
        List<NatureArea> list = areas.ToList();

        Assert.HasCount(2, list);
        CollectionAssert.AllItemsAreNotNull(list);
        CollectionAssert.AllItemsAreUnique(list);
    }

    [TestMethod]
    public async Task GetAll_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid farmId = Guid.NewGuid();

        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(new Farm { Id = farmId, Name = "SeedFarm", CVR = "000", OwnerId = Guid.Empty, AddressId = Guid.Empty });
            for (int i = 0; i < 20; i++)
            {
                seed.NatureAreas.Add(new NatureArea { Id = Guid.NewGuid(), FarmId = farmId, Name = $"N{i}", Description = "d" });
            }
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);

        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        IEnumerable<Task<List<NatureArea>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<NatureArea>[] results = await Task.WhenAll(tasks);

        foreach (List<NatureArea> r in results)
        {
            Assert.HasCount(20, r);
        }
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task MultiSession_Concurrent_Adds_Are_Persisted()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid farmId = Guid.NewGuid();
        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(new Farm { Id = farmId, Name = "SeedFarm", CVR = "000", OwnerId = Guid.Empty, AddressId = Guid.Empty });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);

        List<Task> tasks = new();
        int addCount = 30;

        for (int i = 0; i < addCount; i++)
        {
            int idx = i;
            tasks.Add(Task.Run(async () =>
            {
                NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

                NatureArea a = new() { Id = Guid.NewGuid(), FarmId = farmId, Name = $"Multi{idx}", Description = "d" };
                await repo.AddAsync(a);
            }));
        }

        await Task.WhenAll(tasks);

        NatureAreaRepository repoFinal = new NatureAreaRepository(factoryMock.Object);
        List<NatureArea> all = (await repoFinal.GetAllAsync()).ToList();
        Assert.IsGreaterThanOrEqualTo(addCount, all.Count);
    }

    [TestMethod]
    [TestCategory("Benchmark")]
    public async Task Performance_Benchmark_Adds_And_Gets()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid farmId = Guid.NewGuid();
        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Farms.Add(new Farm { Id = farmId, Name = "SeedFarm", CVR = "000", OwnerId = Guid.Empty, AddressId = Guid.Empty });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = CreateFactory(options);
        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        int ops = 100;
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < ops; i++)
        {
            NatureArea a = new() { Id = Guid.NewGuid(), FarmId = farmId, Name = $"P{i}", Description = "d" };
            await repo.AddAsync(a);
            NatureArea? _ = await repo.GetByIdAsync(a.Id);
        }

        sw.Stop();
        // Ensure operations completed within a reasonable time (example threshold: 5s)
        Assert.IsLessThan(5000, sw.ElapsedMilliseconds, $"Benchmark exceeded: {sw.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).Throws(new COMException("COM error"));

        NatureAreaRepository repo = new NatureAreaRepository(factoryMock.Object);

        NatureArea sample = new NatureArea { Id = Guid.NewGuid(), FarmId = Guid.NewGuid(), Name = "X" };

        // Expect operations to propagate the exception thrown by the factory
        try
        {
            await repo.GetAllAsync();
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.GetByIdAsync(sample.Id);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.AddAsync(sample);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.AddRangeAsync(new[] { sample });
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.UpdateAsync(sample);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            await repo.DeleteAsync(sample.Id);
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }

        try
        {
            // call the specialized method that uses CreateDbContextAsync
            await repo.GetByFarmIdAsync(new Farm { Id = Guid.NewGuid() });
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }

    public TestContext TestContext { get; set; }
}
