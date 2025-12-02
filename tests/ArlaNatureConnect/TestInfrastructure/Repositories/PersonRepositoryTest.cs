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
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "P1", LastName = "L1", Email = "p1@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync();
        }

        // use IDbContextFactory but backed by in-memory options
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new PersonRepository(factoryMock.Object);

        List<Person> result = (await repo.GetPersonsByRoleAsync("Farmer")).ToList();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("p1@x.com", result[0].Email);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Empty_WhenRoleMissing()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Admin" });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new PersonRepository(factoryMock.Object);

        List<Person> result = (await repo.GetPersonsByRoleAsync("Farmer")).ToList();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        using (AppDbContext seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            for (int i = 0; i < 10; i++)
                seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = $"P{i}", LastName = "L", Email = $"p{i}@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new PersonRepository(factoryMock.Object);

        // call concurrently
        IEnumerable<Task<List<Person>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetPersonsByRoleAsync("Farmer")).ToList()));
        List<Person>[] results = await Task.WhenAll(tasks);

        // all should return 10 items
        foreach (List<Person>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Empty_On_Exception()
    {
        // Create factory that throws when creating context to simulate COM/WinRT error or other errors
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new System.Runtime.InteropServices.COMException("COM error"));

        PersonRepository repo = new PersonRepository(factoryMock.Object);

        IEnumerable<Person> result = await repo.GetPersonsByRoleAsync("Farmer");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }
}
