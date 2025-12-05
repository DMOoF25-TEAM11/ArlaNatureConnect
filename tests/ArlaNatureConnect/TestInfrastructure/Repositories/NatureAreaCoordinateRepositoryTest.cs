using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestInfrastructure.Repositories;

[TestClass]
public class NatureAreaCoordinateRepositoryTest
{
    private IDbContextFactory<AppDbContext> GetCreateFactory()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PooledDbContextFactory<AppDbContext>(options);
    }

    [TestMethod]
    public async Task Add_And_Get_NatureAreaCoordinate_Async()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea
        {
            Id = Guid.NewGuid(),
            Name = "Area1",
            Description = "Desc",
            FarmId = Guid.NewGuid()
        };

        NatureAreaCoordinate coord = new NatureAreaCoordinate
        {
            Id = Guid.NewGuid(),
            NatureAreaId = area.Id,
            Latitude = 55.0,
            Longitude = 12.0,
            OrderIndex = 1
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.SaveChangesAsync();

            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            await repo.AddAsync(coord);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            NatureAreaCoordinate? fetched = await repo.GetByIdAsync(coord.Id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(coord.Latitude, fetched!.Latitude);
            Assert.AreEqual(coord.Longitude, fetched.Longitude);
            Assert.AreEqual(area.Id, fetched.NatureAreaId);
            Assert.AreEqual(1, fetched.OrderIndex);
        }
    }

    [TestMethod]
    public async Task GetAll_Returns_All_NatureAreaCoordinates()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaA", Description = "D", FarmId = Guid.NewGuid() };

        NatureAreaCoordinate[] coords = new[]
        {
            new NatureAreaCoordinate { Id = Guid.NewGuid(), NatureAreaId = area.Id, Latitude = 1, Longitude = 2, OrderIndex = 0 },
            new NatureAreaCoordinate { Id = Guid.NewGuid(), NatureAreaId = area.Id, Latitude = 3, Longitude = 4, OrderIndex = 1 }
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaCoordinates.AddRangeAsync(coords);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            List<NatureAreaCoordinate> all = (await repo.GetAllAsync()).ToList();

            Assert.HasCount(2, all);
            CollectionAssert.AreEquivalent(coords.Select(c => c.Id).ToList(), all.Select(c => c.Id).ToList());
        }
    }

    [TestMethod]
    public async Task Update_NatureAreaCoordinate_Persists_Changes()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaU", Description = "D", FarmId = Guid.NewGuid() };
        NatureAreaCoordinate coord = new NatureAreaCoordinate { Id = Guid.NewGuid(), NatureAreaId = area.Id, Latitude = 10, Longitude = 20, OrderIndex = 0 };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaCoordinates.AddAsync(coord);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            coord.Latitude = 99.9;
            coord.OrderIndex = 5;
            await repo.UpdateAsync(coord);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            NatureAreaCoordinate? fetched = await repo.GetByIdAsync(coord.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual(99.9, fetched!.Latitude);
            Assert.AreEqual(5, fetched.OrderIndex);
        }
    }

    [TestMethod]
    public async Task Delete_NatureAreaCoordinate_Removes_Entity()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaD", Description = "D", FarmId = Guid.NewGuid() };
        NatureAreaCoordinate coord = new NatureAreaCoordinate { Id = Guid.NewGuid(), NatureAreaId = area.Id, Latitude = 0, Longitude = 0, OrderIndex = 0 };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaCoordinates.AddAsync(coord);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            await repo.DeleteAsync(coord.Id);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factory);
            NatureAreaCoordinate? fetched = await repo.GetByIdAsync(coord.Id);
            Assert.IsNull(fetched);
        }
    }

    [TestMethod]
    public async Task GetAllAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (AppDbContext seed = new AppDbContext(options))
        {
            NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "TArea", Description = "D", FarmId = Guid.NewGuid() };
            seed.NatureAreas.Add(area);
            for (int i = 0; i < 10; i++)
            {
                seed.NatureAreaCoordinates.Add(new NatureAreaCoordinate { Id = Guid.NewGuid(), NatureAreaId = area.Id, Latitude = i, Longitude = i, OrderIndex = i });
            }
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factoryMock.Object);

        IEnumerable<Task<List<NatureAreaCoordinate>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<NatureAreaCoordinate>[] results = await Task.WhenAll(tasks);

        foreach (List<NatureAreaCoordinate>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        NatureAreaCoordinateRepository repo = new NatureAreaCoordinateRepository(factoryMock.Object);

        try
        {
            await repo.GetAllAsync();
            Assert.Fail("Expected COMException to be thrown");
        }
        catch (COMException)
        {
            // expected
        }
    }
}
