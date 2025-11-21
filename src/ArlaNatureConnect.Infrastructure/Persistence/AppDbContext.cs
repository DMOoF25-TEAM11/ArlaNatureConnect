using System.Threading.Tasks;
using ArlaNatureConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ArlaNatureConnect.Core.Services;

namespace ArlaNatureConnect.Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;

    private readonly IStatusInfoServices? _statusInfoServices;

    public AppDbContext(DbContextOptions<AppDbContext> options, IStatusInfoServices? statusInfoServices = null)
        : base(options)
    {
        _statusInfoServices = statusInfoServices;

        // Check DB connectivity once on construction and set status service accordingly.
        if (_statusInfoServices != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    bool connected = await Database.CanConnectAsync().ConfigureAwait(false);
                    _statusInfoServices.HasDbConnection = connected;
                }
                catch
                {
                    _statusInfoServices.HasDbConnection = false;
                }
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
