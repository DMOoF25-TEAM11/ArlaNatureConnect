using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

    private class TestFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;
        public TestFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            // return a new context instance each time to mimic real factory behavior
            return new AppDbContext(_options);
        }
    }

    private static AppDbContext GetInternalContext(PersonRepository repo)
    {
        // Repository has a protected field named `_context` on the base class. Use reflection to access it for
        // calling SaveChanges in tests.
        FieldInfo? field = repo.GetType().BaseType?.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (field == null) throw new InvalidOperationException("Could not find _context field on repository base type");
        return (AppDbContext)field.GetValue(repo)!;
    }

    [TestMethod]
    public async Task Add_And_Get_Person_Async()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        TestFactory factory = new TestFactory(options);

        Person person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            RoleId = Guid.Empty,
            AddressId = Guid.Empty,
            IsActive = true
        };

        PersonRepository repo = new PersonRepository(factory);
        await repo.AddAsync(person);

        // save using the repository internal context
        AppDbContext internalCtx = GetInternalContext(repo);
        await internalCtx.SaveChangesAsync();

        // verify in a fresh context
        using AppDbContext verifyCtx = new AppDbContext(options);
        Person? fetched = await verifyCtx.Persons.FirstOrDefaultAsync(p => p.Id == person.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("John", fetched!.FirstName);
        Assert.AreEqual("Doe", fetched.LastName);
    }

    [TestMethod]
    public async Task GetAll_Returns_All_Persons()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        TestFactory factory = new TestFactory(options);

        Person[] persons = new Person[]
        {
            new Person { Id = Guid.NewGuid(), FirstName = "A", LastName = "A", Email = "a@a.com", RoleId = Guid.Empty, AddressId = Guid.Empty, IsActive = true },
            new Person { Id = Guid.NewGuid(), FirstName = "B", LastName = "B", Email = "b@b.com", RoleId = Guid.Empty, AddressId = Guid.Empty, IsActive = true }
        };

        PersonRepository repo = new PersonRepository(factory);
        await repo.AddRangeAsync(persons);
        AppDbContext internalCtx = GetInternalContext(repo);
        await internalCtx.SaveChangesAsync();

        List<Person> all = (await repo.GetAllAsync()).ToList();
        Assert.HasCount(2, all);
        CollectionAssert.AreEquivalent(persons.Select(p => p.FirstName).ToList(), all.Select(p => p.FirstName).ToList());
    }

    [TestMethod]
    public async Task Update_Person_Persists_Changes()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        TestFactory factory = new TestFactory(options);

        Person person = new Person { Id = Guid.NewGuid(), FirstName = "Before", LastName = "X", Email = "x@x.com", RoleId = Guid.Empty, AddressId = Guid.Empty, IsActive = true };
        PersonRepository repo = new PersonRepository(factory);
        await repo.AddAsync(person);
        AppDbContext internalCtx = GetInternalContext(repo);
        await internalCtx.SaveChangesAsync();

        // modify and update
        person.FirstName = "After";
        await repo.UpdateAsync(person);
        await internalCtx.SaveChangesAsync();

        using AppDbContext verifyCtx = new AppDbContext(options);
        Person? fetched = await verifyCtx.Persons.FirstOrDefaultAsync(p => p.Id == person.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("After", fetched!.FirstName);
    }

    [TestMethod]
    public async Task Delete_Person_Removes_Entity()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        TestFactory factory = new TestFactory(options);

        Person person = new Person { Id = Guid.NewGuid(), FirstName = "ToDelete", LastName = "X", Email = "d@d.com", RoleId = Guid.Empty, AddressId = Guid.Empty, IsActive = true };
        PersonRepository repo = new PersonRepository(factory);
        await repo.AddAsync(person);
        AppDbContext internalCtx = GetInternalContext(repo);
        await internalCtx.SaveChangesAsync();

        await repo.DeleteAsync(person.Id);
        await internalCtx.SaveChangesAsync();

        using AppDbContext verifyCtx = new AppDbContext(options);
        Person? fetched = await verifyCtx.Persons.FirstOrDefaultAsync(p => p.Id == person.Id);
        Assert.IsNull(fetched);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Persons_When_Role_Exists()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed role and persons using a direct context so the factory-created contexts can read the data
        Guid roleId = Guid.NewGuid();
        using (AppDbContext seedCtx = new AppDbContext(options))
        {
            seedCtx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            seedCtx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "Farm", LastName = "One", Email = "f1@x.com", RoleId = roleId, AddressId = Guid.Empty, IsActive = true });
            seedCtx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "Farm", LastName = "Two", Email = "f2@x.com", RoleId = roleId, AddressId = Guid.Empty, IsActive = true });
            await seedCtx.SaveChangesAsync();
        }

        TestFactory factory = new TestFactory(options);
        PersonRepository repo = new PersonRepository(factory);

        List<Person> result = await repo.GetPersonsByRoleAsync("Farmer");
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Is_Case_And_Whitespace_Insensitive()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Guid roleId = Guid.NewGuid();
        using (AppDbContext seedCtx = new AppDbContext(options))
        {
            seedCtx.Roles.Add(new Role { Id = roleId, Name = "Admin" });
            seedCtx.Persons.Add(new Person { Id = Guid.NewGuid(), FirstName = "A", LastName = "A", Email = "a@x.com", RoleId = roleId, AddressId = Guid.Empty, IsActive = true });
            await seedCtx.SaveChangesAsync();
        }

        TestFactory factory = new TestFactory(options);
        PersonRepository repo = new PersonRepository(factory);

        List<Person> result = await repo.GetPersonsByRoleAsync("  admin  ");
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetPersonsByRoleAsync_Returns_Empty_When_Role_Missing_Or_Invalid_Input()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        TestFactory factory = new TestFactory(options);
        PersonRepository repo = new PersonRepository(factory);

        // empty role
        List<Person> resEmpty = await repo.GetPersonsByRoleAsync("  ");
        Assert.IsNotNull(resEmpty);
        Assert.IsEmpty(resEmpty);

        // non-existing role
        List<Person> resMissing = await repo.GetPersonsByRoleAsync("DoesNotExist");
        Assert.IsNotNull(resMissing);
        Assert.IsEmpty(resMissing);
    }
}
