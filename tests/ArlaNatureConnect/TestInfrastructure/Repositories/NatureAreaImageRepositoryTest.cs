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
public class NatureAreaImageRepositoryTest
{
    private IDbContextFactory<AppDbContext> GetCreateFactory()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PooledDbContextFactory<AppDbContext>(options);
    }

    [TestMethod]
    public async Task Add_And_Get_NatureAreaImage_Async()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea
        {
            Id = Guid.NewGuid(),
            Name = "Area1",
            Description = "Desc",
            FarmId = Guid.NewGuid()
        };

        NatureAreaImage img = new NatureAreaImage
        {
            Id = Guid.NewGuid(),
            NatureAreaId = area.Id,
            ImageUrl = "http://example.com/img1.jpg"
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.SaveChangesAsync();

            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            await repo.AddAsync(img);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            NatureAreaImage? fetched = await repo.GetByIdAsync(img.Id);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(img.ImageUrl, fetched!.ImageUrl);
            Assert.AreEqual(area.Id, fetched.NatureAreaId);
        }
    }

    [TestMethod]
    public async Task GetAll_Returns_All_NatureAreaImages()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaA", Description = "D", FarmId = Guid.NewGuid() };

        NatureAreaImage[] images = new[]
        {
            new NatureAreaImage { Id = Guid.NewGuid(), NatureAreaId = area.Id, ImageUrl = "u1" },
            new NatureAreaImage { Id = Guid.NewGuid(), NatureAreaId = area.Id, ImageUrl = "u2" }
        };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaImages.AddRangeAsync(images);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            List<NatureAreaImage> all = (await repo.GetAllAsync()).ToList();

            Assert.HasCount(2, all);
            CollectionAssert.AreEquivalent(images.Select(i => i.ImageUrl).ToList(), all.Select(i => i.ImageUrl).ToList());
        }
    }

    [TestMethod]
    public async Task Update_NatureAreaImage_Persists_Changes()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaU", Description = "D", FarmId = Guid.NewGuid() };
        NatureAreaImage img = new NatureAreaImage { Id = Guid.NewGuid(), NatureAreaId = area.Id, ImageUrl = "before.jpg" };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaImages.AddAsync(img);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            img.ImageUrl = "after.jpg";
            await repo.UpdateAsync(img);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            NatureAreaImage? fetched = await repo.GetByIdAsync(img.Id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual("after.jpg", fetched!.ImageUrl);
        }
    }

    [TestMethod]
    public async Task Delete_NatureAreaImage_Removes_Entity()
    {
        IDbContextFactory<AppDbContext> factory = GetCreateFactory();

        NatureArea area = new NatureArea { Id = Guid.NewGuid(), Name = "AreaD", Description = "D", FarmId = Guid.NewGuid() };
        NatureAreaImage img = new NatureAreaImage { Id = Guid.NewGuid(), NatureAreaId = area.Id, ImageUrl = "todelete.jpg" };

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            await ctx.NatureAreas.AddAsync(area);
            await ctx.NatureAreaImages.AddAsync(img);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            await repo.DeleteAsync(img.Id);
            await ctx.SaveChangesAsync();
        }

        using (AppDbContext ctx = factory.CreateDbContext())
        {
            NatureAreaImageRepository repo = new NatureAreaImageRepository(factory);
            NatureAreaImage? fetched = await repo.GetByIdAsync(img.Id);
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
                seed.NatureAreaImages.Add(new NatureAreaImage { Id = Guid.NewGuid(), NatureAreaId = area.Id, ImageUrl = $"img{i}" });
            }
            await seed.SaveChangesAsync();
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureAreaImageRepository repo = new NatureAreaImageRepository(factoryMock.Object);

        IEnumerable<Task<List<NatureAreaImage>>> tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => (await repo.GetAllAsync()).ToList()));
        List<NatureAreaImage>[] results = await Task.WhenAll(tasks);

        foreach (List<NatureAreaImage>? r in results)
        {
            Assert.HasCount(10, r);
        }
    }

    [TestMethod]
    public async Task Repository_Methods_Throw_On_COMException_From_Factory()
    {
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        NatureAreaImageRepository repo = new NatureAreaImageRepository(factoryMock.Object);

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
