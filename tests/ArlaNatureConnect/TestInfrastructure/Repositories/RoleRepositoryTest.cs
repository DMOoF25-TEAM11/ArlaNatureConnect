using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Moq;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class RoleRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task Add_And_Get_Role_Async()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        Role role = new Role { Id = Guid.NewGuid(), Name = "Tester" };

        // Add using repository
        var repo = new RoleRepository(factoryMock.Object);
        await repo.AddAsync(role);

        // Read back in a new context
        var fetched = await repo.GetByNameAsync("Tester");

        Assert.IsNotNull(fetched);
        Assert.AreEqual("Tester", fetched!.Name);
    }

    [TestMethod]
    public async Task GetByName_Returns_Null_On_Exception()
    {
        // Create factory that throws when creating context to simulate COM/WinRT error or other errors
        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        var repo = new RoleRepository(factoryMock.Object);

        Role? result = await repo.GetByNameAsync("Farmer");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByName_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed data
        var roleId = Guid.NewGuid();
        using (var seed = new AppDbContext(options))
        {
            seed.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new RoleRepository(factoryMock.Object);

        // call concurrently
        IEnumerable<Task<Role?>> tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() => repo.GetByNameAsync("Farmer")));
        Role?[] results = (await Task.WhenAll(tasks)).ToArray();

        // all should return non-null with correct name
        foreach (var r in results)
        {
            Assert.IsNotNull(r);
            Assert.AreEqual("Farmer", r!.Name);
        }
    }
}
