using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class FarmRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task Add_And_Get_Farm_Async()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Farm farm = new Farm
        {
            Id = Guid.NewGuid(),
            Name = "TestFarm",
            CVR = "12345678",
            PersonId = Guid.Empty,
            AddressId = Guid.Empty
        };

        using (AppDbContext ctx = new AppDbContext(options))
        {
            await ctx.Farms.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            Farm? fetched = await ctx.Farms.FindAsync(farm.Id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(farm.Name, fetched!.Name);
            Assert.AreEqual(farm.CVR, fetched.CVR);
        }
    }

    [TestMethod]
    public async Task GetAll_Returns_All_Farms()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Farm[] farms = new[]
        {
            new Farm { Id = Guid.NewGuid(), Name = "F1", CVR = "111", PersonId = Guid.Empty, AddressId = Guid.Empty },
            new Farm { Id = Guid.NewGuid(), Name = "F2", CVR = "222", PersonId = Guid.Empty, AddressId = Guid.Empty }
        };

        using (AppDbContext ctx = new(options))
        {
            await ctx.Farms.AddRangeAsync(farms);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new(options))
        {
            List<Farm> all = await ctx.Farms.ToListAsync();

            Assert.HasCount(2, all);
            CollectionAssert.AreEquivalent(farms.Select(f => f.Name).ToList(), all.Select(f => f.Name).ToList());
        }
    }

    [TestMethod]
    public async Task Update_Farm_Persists_Changes()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Farm farm = new Farm { Id = Guid.NewGuid(), Name = "Before", CVR = "000", PersonId = Guid.Empty, AddressId = Guid.Empty };
        using (AppDbContext ctx = new AppDbContext(options))
        {
            await ctx.Farms.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            farm.Name = "After";
            ctx.Farms.Update(farm);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            Farm? fetched = await ctx.Farms.FindAsync(farm.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("After", fetched!.Name);
        }
    }

    [TestMethod]
    public async Task Delete_Farm_Removes_Entity()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Farm farm = new() { Id = Guid.NewGuid(), Name = "ToDelete", CVR = "X", PersonId = Guid.Empty, AddressId = Guid.Empty };
        using (AppDbContext ctx = new(options))
        {
            await ctx.Farms.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new(options))
        {
            ctx.Farms.Remove(farm);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new(options))
        {
            Farm? fetched = await ctx.Farms.FindAsync(farm.Id);
            Assert.IsNull(fetched);
        }
    }

    // New tests for FarmRepository

    [TestMethod]
    public async Task GetAllAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed
        using (var seed = new AppDbContext(options))
        {
            for (int i = 0; i < 10; i++)
            {
                seed.Farms.Add(new Farm { Id = Guid.NewGuid(), Name = $"F{i}", CVR = i.ToString(), PersonId = Guid.Empty, AddressId = Guid.Empty });
            }
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new FarmRepository(factoryMock.Object);

        IEnumerable<Task<List<Farm>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<Farm>[] results = await Task.WhenAll(tasks);

        foreach (List<Farm>? r in results)
        {
            Assert.AreEqual(10, r.Count);
        }
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        var repo = new FarmRepository(factoryMock.Object);

        try
        {
            await repo.GetAllAsync();
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }
}
