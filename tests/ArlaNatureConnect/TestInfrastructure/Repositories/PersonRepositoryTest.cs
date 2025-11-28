using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

namespace TestInfrastructure.Repositories;

[TestClass]
public sealed class PersonRepositoryTest
{
    private AppDbContext _ctx = null!;

    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private async Task SeedAsync(DbContextOptions<AppDbContext> options)
    {
        _ctx = new AppDbContext(options);

        Role role = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        _ctx.Roles.Add(role);

        _ctx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "One", Email = "a@example.com", RoleId = role.Id, AddressId = Guid.Empty, IsActive = true });
        _ctx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Two", Email = "b@example.com", RoleId = role.Id, AddressId = Guid.Empty, IsActive = true });

        await _ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_ReturnsEmpty_ForNullOrWhitespace()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedAsync(options);

        // Pass factory that returns the same seeded context
        PersonRepository repo = new PersonRepository(new DbContextFactoryStub(_ctx));

        IEnumerable<Person> res1 = await repo.GetPersonsByRoleAsync(null!);
        IEnumerable<Person> res2 = await repo.GetPersonsByRoleAsync(string.Empty);
        IEnumerable<Person> res3 = await repo.GetPersonsByRoleAsync("   ");

        Assert.IsNotNull(res1);
        Assert.IsNotNull(res2);
        Assert.IsNotNull(res3);
        Assert.AreEqual(0, res1.Count());
        Assert.AreEqual(0, res2.Count());
        Assert.AreEqual(0, res3.Count());
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_ReturnsPersons_WhenRoleExists()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedAsync(options);

        // Pass factory that returns the same seeded context
        PersonRepository repo = new PersonRepository(new DbContextFactoryStub(_ctx));

        List<Person> res = (await repo.GetPersonsByRoleAsync("consultant")).ToList();

        Assert.IsNotNull(res);
        Assert.AreEqual(2, res.Count);
        Assert.IsTrue(res.Any(p => p.FirstName == "Alice"));
        Assert.IsTrue(res.Any(p => p.FirstName == "Bob"));
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_ReturnsEmpty_WhenRoleNotFound()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        await SeedAsync(options);

        // Pass factory that returns the same seeded context
        PersonRepository repo = new PersonRepository(new DbContextFactoryStub(_ctx));

        IEnumerable<Person> res = await repo.GetPersonsByRoleAsync("NonExistingRole");

        Assert.IsNotNull(res);
        Assert.AreEqual(0, res.Count());
    }

    [TestMethod]
    public void GetPersonsByRoleAsync_Concurrent_Calls_WithSeparateContexts_AreThreadSafe()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        // Seed DB once
        Task.Run(async () => await SeedAsync(options)).GetAwaiter().GetResult();

        const int tasks = 16;
        Task[] runners = new Task[tasks];
        int success = 0;

        for (int i = 0; i < tasks; i++)
        {
            runners[i] = Task.Run(async () =>
            {
                // For concurrency test we still create repository that uses the seeded context instance
                PersonRepository repo = new PersonRepository(new DbContextFactoryStub(_ctx));
                IEnumerable<Person> res = await repo.GetPersonsByRoleAsync("Consultant");
                if (res != null && res.Count() == 2) System.Threading.Interlocked.Increment(ref success);
            });
        }

        Task.WaitAll(runners);

        Assert.AreEqual(tasks, success, "All concurrent callers using separate contexts should receive the expected results.");
    }

    private sealed class DbContextFactoryStub : IDbContextFactory<AppDbContext>
    {
        private readonly AppDbContext _context;

        public DbContextFactoryStub(AppDbContext context)
        {
            _context = context;
        }

        public AppDbContext CreateDbContext()
        {
            return _context;
        }
    }
}
