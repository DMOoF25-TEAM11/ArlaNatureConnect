using Microsoft.EntityFrameworkCore;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;
using ArlaNatureConnect.Domain.Entities;

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

        var farm = new Farm
        {
            Id = Guid.NewGuid(),
            Name = "TestFarm",
            CVR = "12345678",
            PersonId = Guid.Empty,
            AddressId = Guid.Empty
        };

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            await repo.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            Farm? fetched = await repo.GetByIdAsync(farm.Id);

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

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            await repo.AddRangeAsync(farms);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            var all = (await repo.GetAllAsync()).ToList();

            Assert.AreEqual(2, all.Count);
            CollectionAssert.AreEquivalent(farms.Select(f => f.Name).ToList(), all.Select(f => f.Name).ToList());
        }
    }

    [TestMethod]
    public async Task Update_Farm_Persists_Changes()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        var farm = new Farm { Id = Guid.NewGuid(), Name = "Before", CVR = "000", PersonId = Guid.Empty, AddressId = Guid.Empty };
        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            await repo.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            farm.Name = "After";
            await repo.UpdateAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            Farm? fetched = await repo.GetByIdAsync(farm.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("After", fetched!.Name);
        }
    }

    [TestMethod]
    public async Task Delete_Farm_Removes_Entity()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        var farm = new Farm { Id = Guid.NewGuid(), Name = "ToDelete", CVR = "X", PersonId = Guid.Empty, AddressId = Guid.Empty };
        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            await repo.AddAsync(farm);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            await repo.DeleteAsync(farm.Id);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new FarmRepository(ctx);
            Farm? fetched = await repo.GetByIdAsync(farm.Id);
            Assert.IsNull(fetched);
        }
    }
}
