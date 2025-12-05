using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class AddressRepositoryTest
{
    private IDbContextFactory<AppDbContext> GetCreateFactory()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PooledDbContextFactory<AppDbContext>(options);
    }

    [TestMethod]
    public async Task Add_And_Get_Address_Async()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        Address addr = new Address
        {
            Id = Guid.NewGuid(),
            Street = "Main St1",
            City = "TestCity",
            PostalCode = "12345",
            Country = "Denmark"
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            Address? fetched = await repo.GetByIdAsync(addr.Id);

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
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        Address[] addresses = new[]
        {
            new Address { Id = Guid.NewGuid(), Street = "S1", City = "C1", PostalCode = "P1", Country = "DK" },
            new Address { Id = Guid.NewGuid(), Street = "S2", City = "C2", PostalCode = "P2", Country = "DK" }
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            await repo.AddRangeAsync(addresses);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            List<Address> all = (await repo.GetAllAsync()).ToList();

            Assert.HasCount(2, all);
            CollectionAssert.AreEquivalent(addresses.Select(a => a.Street).ToList(), all.Select(a => a.Street).ToList());
        }
    }

    [TestMethod]
    public async Task Update_Address_Persists_Changes()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        Address addr = new Address { Id = Guid.NewGuid(), Street = "Before", City = "City", PostalCode = "000", Country = "DK" };
        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            addr.Street = "After";
            await repo.UpdateAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            Address? fetched = await repo.GetByIdAsync(addr.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("After", fetched!.Street);
        }
    }

    [TestMethod]
    public async Task Delete_Address_Removes_Entity()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        Address addr = new Address { Id = Guid.NewGuid(), Street = "ToDelete", City = "City", PostalCode = "X", Country = "DK" };
        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            await repo.AddAsync(addr);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            await repo.DeleteAsync(addr.Id);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            AddressRepository repo = new AddressRepository(factory);
            Address? fetched = await repo.GetByIdAsync(addr.Id);
            Assert.IsNull(fetched);
        }
    }

    // New tests: thread-safety and COM exception

    [TestMethod]
    public async Task GetAllAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        // use explicit in-memory options so multiple contexts share the same in-memory DB
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (AppDbContext seed = new AppDbContext(options))
        {
            for (int i = 0; i < 10; i++)
            {
                seed.Address.Add(new Address { Id = Guid.NewGuid(), Street = $"S{i}", City = "C", PostalCode = "P", Country = "DK" });
            }
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        AddressRepository repo = new AddressRepository(factoryMock.Object);

        IEnumerable<Task<List<Address>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<Address>[] results = await Task.WhenAll(tasks);

        foreach (List<Address>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        AddressRepository repo = new AddressRepository(factoryMock.Object);

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
