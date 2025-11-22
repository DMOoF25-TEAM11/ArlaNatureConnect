using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

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

        // Arrange - add in one context
        Role role = new Role { Id = Guid.NewGuid(), Name = "Tester" };
        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            await repo.AddAsync(role);
            await ctx.SaveChangesAsync();
        }

        // Act - read in a new context to simulate real persistence
        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            Role? fetched = await repo.GetByIdAsync(role.Id);

            // Assert
            Assert.IsNotNull(fetched);
            Assert.AreEqual("Tester", fetched!.Name);
        }
    }

    [TestMethod]
    public async Task GetAll_Returns_All_Roles()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Role[] roles = new[]
        {
            new Role { Id = Guid.NewGuid(), Name = "R1" },
            new Role { Id = Guid.NewGuid(), Name = "R2" }
        };

        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            await repo.AddRangeAsync(roles);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            List<Role> all = (await repo.GetAllAsync()).ToList();

            Assert.HasCount(2, all);
            CollectionAssert.AreEquivalent(roles.Select(r => r.Name).ToList(), all.Select(r => r.Name).ToList());
        }
    }

    [TestMethod]
    public async Task Update_Role_Persists_Changes()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Role role = new Role { Id = Guid.NewGuid(), Name = "Before" };
        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            await repo.AddAsync(role);
            await ctx.SaveChangesAsync();
        }

        // update
        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            role.Name = "After";
            await repo.UpdateAsync(role);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            Role? fetched = await repo.GetByIdAsync(role.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("After", fetched!.Name);
        }
    }

    [TestMethod]
    public async Task Delete_Role_Removes_Entity()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        Role role = new Role { Id = Guid.NewGuid(), Name = "ToDelete" };
        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            await repo.AddAsync(role);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            await repo.DeleteAsync(role.Id);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = new AppDbContext(options))
        {
            RoleRepository repo = new RoleRepository(ctx);
            Role? fetched = await repo.GetByIdAsync(role.Id);
            Assert.IsNull(fetched);
        }
    }
}
