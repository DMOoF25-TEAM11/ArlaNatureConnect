using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

using System.Runtime.InteropServices;

namespace TestInfrastructure.Persistence;

[TestClass]
public class AppDbContextTest
{
    // Use a dedicated in-memory database root per test instance so multiple contexts
    // created from the same options share the same in-memory store reliably.
    private readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName, _dbRoot)
            .Options;
    }

    [TestMethod]
    public async Task CanCreateContextAndDbSetsNotNull()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        using AppDbContext ctx = new AppDbContext(options);

        Assert.IsNotNull(ctx.Farms);
        Assert.IsNotNull(ctx.Persons);
        Assert.IsNotNull(ctx.Roles);
        Assert.IsNotNull(ctx.Addresses);

        await Task.CompletedTask;
    }

    [TestMethod]
    public void OnModelCreating_Configures_AutoInclude()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        using AppDbContext ctx = new(options);

        IModel model = ctx.Model;
        IEntityType? personEntity = model.FindEntityType(typeof(Person));
        Assert.IsNotNull(personEntity, "Person entity should be in the model");

        INavigation? pRole = personEntity!.FindNavigation(nameof(Person.Role));
        INavigation? pAddress = personEntity.FindNavigation(nameof(Person.Address));
        INavigation? pFarms = personEntity.FindNavigation(nameof(Person.Farms));

        Assert.IsNotNull(pRole, "Person.Role navigation should exist");
        Assert.IsNotNull(pAddress, "Person.Address navigation should exist");
        Assert.IsNotNull(pFarms, "Person.Farms navigation should exist");

        // AutoInclude sets navigation to eager-loaded in the EF model
        Assert.IsTrue(pRole!.IsEagerLoaded, "Person.Role should be configured for AutoInclude");
        Assert.IsTrue(pAddress!.IsEagerLoaded, "Person.Address should be configured for AutoInclude");
        Assert.IsTrue(pFarms!.IsEagerLoaded, "Person.Farms should be configured for AutoInclude");

        IEntityType? farmEntity = model.FindEntityType(typeof(Farm));
        Assert.IsNotNull(farmEntity, "Farm entity should be in the model");

        INavigation? fAddress = farmEntity!.FindNavigation(nameof(Farm.Address));
        INavigation? fPerson = farmEntity.FindNavigation(nameof(Farm.Person));

        Assert.IsNotNull(fAddress, "Farm.Address navigation should exist");
        Assert.IsNotNull(fPerson, "Farm.Person navigation should exist");

        Assert.IsTrue(fAddress!.IsEagerLoaded, "Farm.Address should be configured for AutoInclude");
        Assert.IsTrue(fPerson!.IsEagerLoaded, "Farm.Person should be configured for AutoInclude");
    }

    [TestMethod]
    public async Task ConcurrentContexts_CanAddConcurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        const int threads = 8;
        Task[] tasks = new Task[threads];

        for (int i = 0; i < threads; i++)
        {
            int idx = i;
            tasks[i] = Task.Run(async () =>
            {
                using AppDbContext ctx = new AppDbContext(options);
                ctx.Farms.Add(new Farm
                {
                    Id = Guid.NewGuid(),
                    Name = $"Farm_{idx}",
                    CVR = idx.ToString("D8"),
                    PersonId = Guid.Empty,
                    AddressId = Guid.Empty
                });
                await ctx.SaveChangesAsync();
            });
        }

        await Task.WhenAll(tasks);

        using AppDbContext verify = new AppDbContext(options);
        int count = await verify.Farms.CountAsync();
        Assert.AreEqual(threads, count, "All concurrent inserts should be visible in the shared in-memory DB");
    }

    [TestMethod]
    public void FactoryCreate_Throws_COMException()
    {
        ThrowingFactory factory = new ThrowingFactory();

        Assert.Throws<COMException>(() => factory.CreateDbContext());
    }

    private sealed class ThrowingFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            throw new COMException("Simulated COM error while creating DbContext");
        }
    }
}
