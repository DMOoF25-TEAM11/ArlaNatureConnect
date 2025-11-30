using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace TestInfrastructure.Repositories;

[TestClass]
public class PersonRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Persons_WhenRoleExists()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed data
        var roleId = Guid.NewGuid();
        var addrId = Guid.NewGuid();

        using (var seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "P1", LastName = "L1", Email = "p1@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync();
        }

        // use IDbContextFactory but backed by in-memory options
        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new PersonRepository(factoryMock.Object);

        var result = (await repo.GetPersonsByRoleAsync("Farmer")).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("p1@x.com", result[0].Email);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Empty_WhenRoleMissing()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        using (var seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Admin" });
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new PersonRepository(factoryMock.Object);

        var result = (await repo.GetPersonsByRoleAsync("Farmer")).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        var roleId = Guid.NewGuid();
        var addrId = Guid.NewGuid();

        using (var seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            for (int i = 0; i < 10; i++)
                seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = $"P{i}", LastName = "L", Email = $"p{i}@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new PersonRepository(factoryMock.Object);

        // call concurrently
        IEnumerable<Task<List<Person>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetPersonsByRoleAsync("Farmer")).ToList()));
        List<Person>[] results = await Task.WhenAll(tasks);

        // all should return 10 items
        foreach (List<Person>? r in results)
        {
            Assert.AreEqual(10, r.Count);
        }
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Empty_On_Exception()
    {
        // Create factory that throws when creating context to simulate COM/WinRT error or other errors
        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new System.Runtime.InteropServices.COMException("COM error"));

        var repo = new PersonRepository(factoryMock.Object);

        IEnumerable<Person> result = await repo.GetPersonsByRoleAsync("Farmer");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }
}
