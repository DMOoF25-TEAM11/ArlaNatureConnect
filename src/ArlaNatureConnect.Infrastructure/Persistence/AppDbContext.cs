using ArlaNatureConnect.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure automatic inclusion of related entities
        modelBuilder.Entity<Person>().Navigation(e => e.Role).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Address).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Farms).AutoInclude();

        // Farm auto-includes (add these)
        modelBuilder.Entity<Farm>().Navigation(f => f.Address).AutoInclude();
        modelBuilder.Entity<Farm>().Navigation(f => f.Person).AutoInclude();
    }
}
