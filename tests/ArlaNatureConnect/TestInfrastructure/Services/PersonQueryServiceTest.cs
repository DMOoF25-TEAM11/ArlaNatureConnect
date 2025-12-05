//using ArlaNatureConnect.Domain.Entities;
//using ArlaNatureConnect.Infrastructure.Persistence;

//using Microsoft.EntityFrameworkCore;

//namespace TestInfrastructure.Services;

//[TestClass]
//public class PersonQueryServiceTest
//{
//    private DbContextOptions<AppDbContext> CreateOptions()
//    {
//        return new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;
//    }

//    private class TestFactory : IDbContextFactory<AppDbContext>
//    {
//        private readonly DbContextOptions<AppDbContext> _options;
//        public TestFactory(DbContextOptions<AppDbContext> options)
//        {
//            _options = options;
//        }

//        public AppDbContext CreateDbContext()
//        {
//            return new AppDbContext(_options);
//        }

//        public ValueTask<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
//        {
//            return new ValueTask<AppDbContext>(new AppDbContext(_options));
//        }
//    }

//    [TestMethod]
//    public async Task GetAllWithRolesAsync_Returns_Persons_With_Role_And_Address()
//    {
//        DbContextOptions<AppDbContext> options = CreateOptions();

//        // seed data using a direct context so factory-created contexts can read it
//        Guid roleId = Guid.NewGuid();
//        Guid roleId2 = Guid.NewGuid();
//        Guid addrId = Guid.NewGuid();
//        Guid addrId2 = Guid.NewGuid();

//        using (AppDbContext seedCtx = new AppDbContext(options))
//        {
//            seedCtx.Roles.Add(new Role { Id = roleId, Name = "Farmer" });
//            seedCtx.Roles.Add(new Role { Id = roleId2, Name = "Admin" });
//            seedCtx.Address.Add(new Address
//            {
//                Id = addrId,
//                Street = "Main",
//                City = "Test",
//                PostalCode = "12345",
//                Country = "Denmark"
//            });
//            seedCtx.Address.Add(new Address
//            {
//                Id = addrId2,
//                Street = "SideRoad",
//                City = "Downtown",
//                PostalCode = "54321",
//                Country = "Denmark"
//            });

//            seedCtx.Persons.Add(new Owner { Id = Guid.NewGuid(), FirstName = "P1", LastName = "One", Email = "p1@x.com", RoleId = roleId, AddressId = addrId, IsActive = true });
//            seedCtx.Persons.Add(new Owner { Id = Guid.NewGuid(), FirstName = "P2", LastName = "Two", Email = "p2@x.com", RoleId = roleId2, AddressId = addrId2, IsActive = true });

//            await seedCtx.SaveChangesAsync();
//        }

//        TestFactory factory = new TestFactory(options);
//        PersonQueryService service = new PersonQueryService(factory);

//        List<Owner> result = (await service.GetAllWithRolesAsync()).ToList();

//        Assert.IsNotNull(result);
//        Assert.AreEqual(2, result.Count);

//        foreach (Owner p in result)
//        {
//            Assert.IsNotNull(p.Role, "Role navigation should be populated");
//            Assert.IsNotNull(p.Address, "Address navigation should be populated");
//        }

//        Owner p1 = result.Single(p => p.Email == "p1@x.com");
//        Assert.AreEqual("Farmer", p1.Role.Name);
//        Assert.AreEqual("Test", p1.Address.City);

//        Owner p2 = result.Single(p => p.Email == "p2@x.com");
//        Assert.AreEqual("Admin", p2.Role.Name);
//        Assert.AreEqual("Downtown", p2.Address.City);
//    }
//}
