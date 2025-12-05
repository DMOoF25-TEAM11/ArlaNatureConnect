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

        // Create required Owner and Address entities first (due to AutoInclude and foreign key constraints)
        Guid OwnerId = Guid.NewGuid();
        Guid addressId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        using (AppDbContext ctx = new AppDbContext(options))
        {
            // Create Role first
            ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });

            // Create Address
            ctx.Addresses.Add(new Address { Id = addressId, Street = "Test Street", City = "Test City", PostalCode = "1234", Country = "DK" });

            // Create Owner
            ctx.Persons.Add(new Person { Id = OwnerId, FirstName = "Test", LastName = "Owner", Email = "test@test.com", RoleId = roleId, AddressId = addressId, IsActive = true });

            await ctx.SaveChangesAsync();
        }

        Farm farm = new Farm
        {
            Id = Guid.NewGuid(),
            Name = "TestFarm",
            CVR = "12345678",
            OwnerId = OwnerId,
            AddressId = addressId
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

        // Create required Owner and Address entities first (due to AutoInclude and foreign key constraints)
        Guid OwnerId1 = Guid.NewGuid();
        Guid addressId1 = Guid.NewGuid();
        Guid OwnerId2 = Guid.NewGuid();
        Guid addressId2 = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        using (AppDbContext ctx = new(options))
        {
            // Create Role first
            ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });

            // Create Addresses
            ctx.Addresses.Add(new Address { Id = addressId1, Street = "Street1", City = "City1", PostalCode = "1234", Country = "DK" });
            ctx.Addresses.Add(new Address { Id = addressId2, Street = "Street2", City = "City2", PostalCode = "5678", Country = "DK" });

            // Create Persons
            ctx.Persons.Add(new Person { Id = OwnerId1, FirstName = "Owner1", LastName = "Test", Email = "owner1@test.com", RoleId = roleId, AddressId = addressId1, IsActive = true });
            ctx.Persons.Add(new Person { Id = OwnerId2, FirstName = "Owner2", LastName = "Test", Email = "owner2@test.com", RoleId = roleId, AddressId = addressId2, IsActive = true });

            await ctx.SaveChangesAsync();
        }

        Farm[] farms = new[]
        {
            new Farm { Id = Guid.NewGuid(), Name = "F1", CVR = "111", OwnerId = OwnerId1, AddressId = addressId1 },
            new Farm { Id = Guid.NewGuid(), Name = "F2", CVR = "222", OwnerId = OwnerId2, AddressId = addressId2 }
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

        // Create required Owner and Address entities first (due to AutoInclude and foreign key constraints)
        Guid OwnerId = Guid.NewGuid();
        Guid addressId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        using (AppDbContext ctx = new AppDbContext(options))
        {
            // Create Role first
            ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });

            // Create Address
            ctx.Addresses.Add(new Address { Id = addressId, Street = "Test Street", City = "Test City", PostalCode = "1234", Country = "DK" });

            // Create Owner
            ctx.Persons.Add(new Person { Id = OwnerId, FirstName = "Test", LastName = "Owner", Email = "test@test.com", RoleId = roleId, AddressId = addressId, IsActive = true });

            await ctx.SaveChangesAsync();
        }

        Farm farm = new Farm { Id = Guid.NewGuid(), Name = "Before", CVR = "000", OwnerId = OwnerId, AddressId = addressId };
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

        // Create required Owner and Address entities first (due to AutoInclude and foreign key constraints)
        Guid OwnerId = Guid.NewGuid();
        Guid addressId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        using (AppDbContext ctx = new(options))
        {
            // Create Role first
            ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });

            // Create Address
            ctx.Addresses.Add(new Address { Id = addressId, Street = "Test Street", City = "Test City", PostalCode = "1234", Country = "DK" });

            // Create Owner
            ctx.Persons.Add(new Person { Id = OwnerId, FirstName = "Test", LastName = "Owner", Email = "test@test.com", RoleId = roleId, AddressId = addressId, IsActive = true });

            await ctx.SaveChangesAsync();
        }

        Farm farm = new() { Id = Guid.NewGuid(), Name = "ToDelete", CVR = "X", OwnerId = OwnerId, AddressId = addressId };
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

        // Create required Owner and Address entities first (due to AutoInclude and foreign key constraints)
        Guid roleId = Guid.NewGuid();
        List<Guid> OwnerIds = new List<Guid>();
        List<Guid> addressIds = new List<Guid>();

        using (AppDbContext seed = new AppDbContext(options))
        {
            // Create Role first
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });

            // Create Addresses and Persons
            for (int i = 0; i < 10; i++)
            {
                Guid addressId = Guid.NewGuid();
                Guid OwnerId = Guid.NewGuid();
                addressIds.Add(addressId);
                OwnerIds.Add(OwnerId);

                seed.Addresses.Add(new Address { Id = addressId, Street = $"Street{i}", City = $"City{i}", PostalCode = $"{i}000", Country = "DK" });
                seed.Persons.Add(new Person { Id = OwnerId, FirstName = $"Owner{i}", LastName = "Test", Email = $"owner{i}@test.com", RoleId = roleId, AddressId = addressId, IsActive = true });
            }

            await seed.SaveChangesAsync();
        }

        // seed
        using (AppDbContext seed = new AppDbContext(options))
        {
            for (int i = 0; i < 10; i++)
            {
                seed.Farms.Add(new Farm { Id = Guid.NewGuid(), Name = $"F{i}", CVR = i.ToString(), OwnerId = OwnerIds[i], AddressId = addressIds[i] });
            }
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        FarmRepository repo = new FarmRepository(factoryMock.Object);

        IEnumerable<Task<List<Farm>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<Farm>[] results = await Task.WhenAll(tasks);

        foreach (List<Farm>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        FarmRepository repo = new FarmRepository(factoryMock.Object);

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
