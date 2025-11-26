using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TestInfrastructure.Persistence;

[TestClass]
public class AppDbContextTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task CanCreateContextAndDbSetsNotNull()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        using var ctx = new AppDbContext(options);

        Assert.IsNotNull(ctx.Farms);
        Assert.IsNotNull(ctx.Persons);
        Assert.IsNotNull(ctx.Roles);
        Assert.IsNotNull(ctx.Addresses);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task CanAddAndRetrieveFarm()
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

        // Add entity
        using (var ctx = new AppDbContext(options))
        {
            ctx.Farms.Add(farm);
            await ctx.SaveChangesAsync();
        }

        // Retrieve in new context to ensure data persisted in the in-memory DB
        using (var ctx = new AppDbContext(options))
        {
            Farm? dbFarm = await ctx.Farms.FirstOrDefaultAsync(f => f.Id == farm.Id);
            Assert.IsNotNull(dbFarm);
            Assert.AreEqual(farm.Name, dbFarm!.Name);
            Assert.AreEqual(farm.CVR, dbFarm.CVR);
        }
    }
}
