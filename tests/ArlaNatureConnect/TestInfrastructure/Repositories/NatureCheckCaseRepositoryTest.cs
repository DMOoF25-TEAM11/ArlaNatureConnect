using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Moq;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class NatureCheckCaseRepositoryTest
{
    private DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public async Task GetActiveCasesAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed data: 30 active, 20 non-active
        int activeCount = 30;
        using (var seed = new AppDbContext(options))
        {
            for (int i = 0; i < activeCount; i++)
            {
                seed.NatureCheckCases.Add(new NatureCheckCase
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.NewGuid(),
                    ConsultantId = Guid.NewGuid(),
                    AssignedByPersonId = Guid.NewGuid(),
                    Status = i % 2 == 0 ? ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned : ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.InProgress,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            for (int i = 0; i < 20; i++)
            {
                seed.NatureCheckCases.Add(new NatureCheckCase
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.NewGuid(),
                    ConsultantId = Guid.NewGuid(),
                    AssignedByPersonId = Guid.NewGuid(),
                    Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Completed,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new NatureCheckCaseRepository(factoryMock.Object);

        const int threads = 8;
        Task<IReadOnlyList<NatureCheckCase>>[] tasks = new Task<IReadOnlyList<NatureCheckCase>>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => repo.GetActiveCasesAsync());
        }

        IReadOnlyList<NatureCheckCase>[] results = await Task.WhenAll(tasks);

        foreach (var r in results)
        {
            Assert.AreEqual(activeCount, r.Count);
        }
    }

    [TestMethod]
    public async Task FarmHasActiveCaseAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid farmId = Guid.NewGuid();

        using (var seed = new AppDbContext(options))
        {
            seed.NatureCheckCases.Add(new NatureCheckCase
            {
                Id = Guid.NewGuid(),
                FarmId = farmId,
                ConsultantId = Guid.NewGuid(),
                AssignedByPersonId = Guid.NewGuid(),
                Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new NatureCheckCaseRepository(factoryMock.Object);

        const int calls = 50;
        Task<bool>[] tasks = Enumerable.Range(0, calls).Select(_ => Task.Run(() => repo.FarmHasActiveCaseAsync(farmId))).ToArray();

        bool[] results = await Task.WhenAll(tasks);

        foreach (bool r in results)
        {
            Assert.IsTrue(r);
        }
    }

    [TestMethod]
    public async Task GetAssignedCasesForConsultantAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid consultantId = Guid.NewGuid();
        int assignedCount = 12;

        using (var seed = new AppDbContext(options))
        {
            for (int i = 0; i < assignedCount; i++)
            {
                seed.NatureCheckCases.Add(new NatureCheckCase
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.NewGuid(),
                    ConsultantId = consultantId,
                    AssignedByPersonId = Guid.NewGuid(),
                    Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned,
                    CreatedAt = DateTimeOffset.UtcNow,
                    AssignedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
                });
            }
            await seed.SaveChangesAsync();
        }

        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        var repo = new NatureCheckCaseRepository(factoryMock.Object);

        const int threads = 6;
        Task<IReadOnlyList<NatureCheckCase>>[] tasks = new Task<IReadOnlyList<NatureCheckCase>>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => repo.GetAssignedCasesForConsultantAsync(consultantId));
        }

        IReadOnlyList<NatureCheckCase>[] results = await Task.WhenAll(tasks);

        foreach (var r in results)
        {
            Assert.AreEqual(assignedCount, r.Count);
        }
    }

    [TestMethod]
    public async Task Methods_Return_Safe_Defaults_On_COMException()
    {
        // factory that throws COMException
        var factoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        var repo = new NatureCheckCaseRepository(factoryMock.Object);

        var active = await repo.GetActiveCasesAsync();
        Assert.IsNotNull(active);
        Assert.AreEqual(0, active.Count);

        bool has = await repo.FarmHasActiveCaseAsync(Guid.NewGuid());
        Assert.IsFalse(has);

        var assigned = await repo.GetAssignedCasesForConsultantAsync(Guid.NewGuid());
        Assert.IsNotNull(assigned);
        Assert.AreEqual(0, assigned.Count);
    }
}
