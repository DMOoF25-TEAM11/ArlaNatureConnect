using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class PersonRepositoryTest
{
    private static DbContextOptions<AppDbContext> CreateOptions() => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetPersonsByRoleAsync_Returns_Persons_WhenRoleExists()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed data
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "P1", LastName = "L1", Email = "p1@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        // use IDbContextFactory but backed by in-memory options
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        List<Person> result = (await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken)).ToList();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("p1@x.com", result[0].Email);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetPersonsByRoleAsync_Returns_Empty_WhenRoleMissing()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        using (AppDbContext seed = new(options))
        {
            seed.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Admin" });
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        List<Person> result = (await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken)).ToList();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task GetPersonsByRoleAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            for (int i = 0; i < 10; i++)
                seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = $"P{i}", LastName = "L", Email = $"p{i}@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        // call concurrently
        IEnumerable<Task<List<Person>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken)).ToList()));
        List<Person>[] results = await Task.WhenAll(tasks);

        // all should return 10 items
        foreach (List<Person>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetPersonsByRoleAsync_Returns_Empty_On_Exception()
    {
        // Create factory that throws when creating context to simulate COM/WinRT error or other errors
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new System.Runtime.InteropServices.COMException("COM error"));

        PersonRepository repo = new(factoryMock.Object);

        IEnumerable<Person> result = await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByEmailAsync_Returns_Person_WhenExists()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid addrId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john.doe@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        Person? p = await repo.GetByEmailAsync("john.doe@x.com", TestContext.CancellationToken);

        Assert.IsNotNull(p);
        Assert.AreEqual("john.doe@x.com", p!.Email);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByEmailAsync_Returns_Null_WhenMissing()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        Person? p = await repo.GetByEmailAsync("noone@x.com", TestContext.CancellationToken);

        Assert.IsNull(p);
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task GetByEmailAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid addrId = Guid.NewGuid();
        string email = "concurrent@x.com";
        Guid roleId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "C", LastName = "U", Email = email, RoleId = roleId, AddressId = addrId, IsActive = true });
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        const int calls = 30;
        Task<Person?>[] tasks = Enumerable.Range(0, calls).Select(_ => Task.Run(() => repo.GetByEmailAsync(email, TestContext.CancellationToken))).ToArray();

        Person?[] results = await Task.WhenAll(tasks);

        foreach (Person? r in results)
            Assert.IsNotNull(r);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetByEmailAsync_Returns_Null_On_Exception()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        PersonRepository repo = new(factoryMock.Object);

        Person? p = await repo.GetByEmailAsync("any@x.com", TestContext.CancellationToken);

        Assert.IsNull(p);
    }

    [TestMethod]
    [TestCategory("Benchmark")]
    public async Task GetPersonsByRoleAsync_Performance_Benchmark()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        const int count = 1000;
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seed.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            for (int i = 0; i < count; i++)
            {
                seed.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = $"P{i}", LastName = "L", Email = $"p{i}@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
            }
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        PersonRepository repo = new(factoryMock.Object);

        Stopwatch sw = Stopwatch.StartNew();
        List<Person> result = (await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken)).ToList();
        sw.Stop();

        Assert.HasCount(count, result);
        Assert.IsLessThan(5000, sw.ElapsedMilliseconds, $"GetPersonsByRoleAsync took too long: {sw.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task MultiSession_Can_Add_And_Read_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid roleId = Guid.NewGuid();
        Guid addrId = Guid.NewGuid();

        // ensure role exists
        using (AppDbContext ctx = new(options))
        {
            ctx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            ctx.Addresses.Add(new Address { Id = addrId, Street = "S", City = "C", PostalCode = "P", Country = "DK" });
            await ctx.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        const int writers = 10;
        Task[] writerTasks = new Task[writers];

        for (int i = 0; i < writers; i++)
        {
            writerTasks[i] = Task.Run(async () =>
            {
                using AppDbContext ctx = new(options);
                ctx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "W", LastName = "R", Email = $"w{Guid.NewGuid()}@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
                await ctx.SaveChangesAsync(TestContext.CancellationToken);
            }, TestContext.CancellationToken);
        }

        await Task.WhenAll(writerTasks);

        PersonRepository repo = new(factoryMock.Object);
        List<Person> list = (await repo.GetPersonsByRoleAsync("Farmer", TestContext.CancellationToken)).ToList();

        Assert.HasCount(writers, list);
    }

    public TestContext TestContext { get; set; }
}
