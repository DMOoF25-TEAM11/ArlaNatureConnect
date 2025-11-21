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

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
