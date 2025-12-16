using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestInfrastructure.Repositories;

[TestClass]
public class NatureCheckCaseRepositoryTest
{
    private static DbContextOptions<AppDbContext> CreateOptions() => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task GetActiveCasesAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // seed data: 30 active, 20 non-active
        int activeCount = 30;
        using (AppDbContext seed = new(options))
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
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        const int threads = 8;
        Task<IReadOnlyList<NatureCheckCase>>[] tasks = new Task<IReadOnlyList<NatureCheckCase>>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => repo.GetActiveCasesAsync(TestContext.CancellationToken));
        }

        IReadOnlyList<NatureCheckCase>[] results = await Task.WhenAll(tasks);

        foreach (IReadOnlyList<NatureCheckCase> r in results)
        {
            Assert.HasCount(activeCount, r, "Expected all concurrent calls to return the same active count");
        }
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task FarmHasActiveCaseAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid farmId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
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
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        const int calls = 50;
        Task<bool>[] tasks = [.. Enumerable.Range(0, calls).Select(_ => Task.Run(() => repo.FarmHasActiveCaseAsync(farmId, TestContext.CancellationToken)))];

        bool[] results = await Task.WhenAll(tasks);

        foreach (bool r in results)
        {
            Assert.IsTrue(r);
        }
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task GetAssignedCasesForConsultantAsync_Is_ThreadSafe_When_Called_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid consultantId = Guid.NewGuid();
        int assignedCount = 12;

        using (AppDbContext seed = new(options))
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
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        const int threads = 6;
        Task<IReadOnlyList<NatureCheckCase>>[] tasks = new Task<IReadOnlyList<NatureCheckCase>>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => repo.GetAssignedCasesForConsultantAsync(consultantId, TestContext.CancellationToken));
        }

        IReadOnlyList<NatureCheckCase>[] results = await Task.WhenAll(tasks);

        foreach (IReadOnlyList<NatureCheckCase> r in results)
        {
            Assert.HasCount(assignedCount, r);
        }
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task GetActiveCaseForFarmAsync_Returns_First_Active_For_Given_Farm()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();
        Guid farmId = Guid.NewGuid();

        using (AppDbContext seed = new(options))
        {
            // older completed
            seed.NatureCheckCases.Add(new NatureCheckCase
            {
                Id = Guid.NewGuid(),
                FarmId = farmId,
                ConsultantId = Guid.NewGuid(),
                AssignedByPersonId = Guid.NewGuid(),
                Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Completed,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
            });

            // an in-progress case
            seed.NatureCheckCases.Add(new NatureCheckCase
            {
                Id = Guid.NewGuid(),
                FarmId = farmId,
                ConsultantId = Guid.NewGuid(),
                AssignedByPersonId = Guid.NewGuid(),
                Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.InProgress,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
            });

            // an assigned case
            seed.NatureCheckCases.Add(new NatureCheckCase
            {
                Id = Guid.NewGuid(),
                FarmId = farmId,
                ConsultantId = Guid.NewGuid(),
                AssignedByPersonId = Guid.NewGuid(),
                Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            });

            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        NatureCheckCase? active = await repo.GetActiveCaseForFarmAsync(farmId, TestContext.CancellationToken);

        Assert.IsNotNull(active);
        Assert.AreEqual(farmId, active!.FarmId);
        Assert.IsTrue(active.Status == ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned || active.Status == ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.InProgress);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public async Task Methods_Return_Safe_Defaults_On_COMException()
    {
        // factory that throws COMException
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Throws(new COMException("COM error"));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        IReadOnlyList<NatureCheckCase> active = await repo.GetActiveCasesAsync(TestContext.CancellationToken);
        Assert.IsNotNull(active);
        Assert.IsEmpty(active);

        bool has = await repo.FarmHasActiveCaseAsync(Guid.NewGuid(), TestContext.CancellationToken);
        Assert.IsFalse(has);

        IReadOnlyList<NatureCheckCase> assigned = await repo.GetAssignedCasesForConsultantAsync(Guid.NewGuid(), TestContext.CancellationToken);
        Assert.IsNotNull(assigned);
        Assert.IsEmpty(assigned);

        NatureCheckCase? activeForFarm = await repo.GetActiveCaseForFarmAsync(Guid.NewGuid(), TestContext.CancellationToken);
        Assert.IsNull(activeForFarm);
    }

    [TestMethod]
    [TestCategory("Benchmark")]
    public async Task GetActiveCasesAsync_Performance_Benchmark()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        const int activeCount = 1000;
        using (AppDbContext seed = new(options))
        {
            for (int i = 0; i < activeCount; i++)
            {
                seed.NatureCheckCases.Add(new NatureCheckCase
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.NewGuid(),
                    ConsultantId = Guid.NewGuid(),
                    AssignedByPersonId = Guid.NewGuid(),
                    Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            await seed.SaveChangesAsync(TestContext.CancellationToken);
        }

        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        NatureCheckCaseRepository repo = new(factoryMock.Object);

        Stopwatch sw = Stopwatch.StartNew();
        IReadOnlyList<NatureCheckCase> result = await repo.GetActiveCasesAsync(TestContext.CancellationToken);
        sw.Stop();

        Assert.HasCount(activeCount, result);
        // allow generous time budget - on CI this should be fairly quick for in-memory DB
        Assert.IsLessThan(5000, sw.ElapsedMilliseconds, $"GetActiveCasesAsync took too long: {sw.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    [TestCategory("Concurrency")]
    public async Task MultiSession_Can_Add_And_Read_Concurrently()
    {
        DbContextOptions<AppDbContext> options = CreateOptions();

        // factory that returns a new context for same in-memory DB
        Mock<IDbContextFactory<AppDbContext>> factoryMock = new();
        factoryMock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));

        const int writers = 10;
        Task[] writerTasks = new Task[writers];

        for (int i = 0; i < writers; i++)
        {
            writerTasks[i] = Task.Run(async () =>
            {
                using AppDbContext ctx = new(options);
                ctx.NatureCheckCases.Add(new NatureCheckCase
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.NewGuid(),
                    ConsultantId = Guid.NewGuid(),
                    AssignedByPersonId = Guid.NewGuid(),
                    Status = ArlaNatureConnect.Domain.Enums.NatureCheckCaseStatus.Assigned,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                await ctx.SaveChangesAsync(TestContext.CancellationToken);
            }, TestContext.CancellationToken);
        }

        await Task.WhenAll(writerTasks);

        // repository should see all written entries
        NatureCheckCaseRepository repo = new(factoryMock.Object);
        IReadOnlyList<NatureCheckCase> cases = await repo.GetActiveCasesAsync(TestContext.CancellationToken);

        Assert.HasCount(writers, cases);
    }

    public TestContext TestContext { get; set; }
}
