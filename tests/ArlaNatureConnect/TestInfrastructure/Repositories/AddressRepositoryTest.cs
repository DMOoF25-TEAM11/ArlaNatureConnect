using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;
using ArlaNatureConnect.Domain.Entities;

namespace TestInfrastructure.Repositories;

[TestClass]
public class AddressRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task Add_And_Get_Address_Async()
    {
        var options = CreateOptions();

        var addr = new Address
        {
            Id = Guid.NewGuid(),
            Street = "Main St1",
            City = "TestCity",
            PostalCode = "12345",
            Country = "Denmark"
        };

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            var fetched = await repo.GetByIdAsync(addr.Id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(addr.Street, fetched!.Street);
            Assert.AreEqual(addr.City, fetched.City);
            Assert.AreEqual(addr.PostalCode, fetched.PostalCode);
            Assert.AreEqual(addr.Country, fetched.Country);
        }
    }

    [TestMethod]
    public async Task GetAll_Returns_All_Addresses()
    {
        var options = CreateOptions();

        var addresses = new[]
        {
            new Address { Id = Guid.NewGuid(), Street = "S1", City = "C1", PostalCode = "P1", Country = "DK" },
            new Address { Id = Guid.NewGuid(), Street = "S2", City = "C2", PostalCode = "P2", Country = "DK" }
        };

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            await repo.AddRangeAsync(addresses);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            var all = (await repo.GetAllAsync()).ToList();

            Assert.AreEqual(2, all.Count);
            CollectionAssert.AreEquivalent(addresses.Select(a => a.Street).ToList(), all.Select(a => a.Street).ToList());
        }
    }

    [TestMethod]
    public async Task Update_Address_Persists_Changes()
    {
        var options = CreateOptions();

        var addr = new Address { Id = Guid.NewGuid(), Street = "Before", City = "City", PostalCode = "000", Country = "DK" };
        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            addr.Street = "After";
            await repo.UpdateAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            var fetched = await repo.GetByIdAsync(addr.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("After", fetched!.Street);
        }
    }

    [TestMethod]
    public async Task Delete_Address_Removes_Entity()
    {
        var options = CreateOptions();

        var addr = new Address { Id = Guid.NewGuid(), Street = "ToDelete", City = "City", PostalCode = "X", Country = "DK" };
        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            await repo.DeleteAsync(addr.Id);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = new AppDbContext(options))
        {
            var repo = new AddressRepository(ctx);
            var fetched = await repo.GetByIdAsync(addr.Id);
            Assert.IsNull(fetched);
        }
    }
}
