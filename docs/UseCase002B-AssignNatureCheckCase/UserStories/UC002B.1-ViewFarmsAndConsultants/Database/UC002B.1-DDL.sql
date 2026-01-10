/*
    File: UC002B.1-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.1 - View Farms and Consultants for Assignment
    Safety: This document shows how Entity Framework Core handles database schema for viewing farms and consultants.
    
    Use Case: UC002B.1 - View Farms and Consultants for Assignment
    Description: Shows Entity Framework Core DbContext configuration and entity mappings for loading farm assignment
    data and consultant lists. EF Core handles all database schema management automatically through migrations.
    
    Dependencies: Requires entities from UC001 and UC002 (Farm, Person, Role, Address, NatureCheckCase)
    
    Note: This is documentation showing EF Core approach. Actual database schema is managed via EF Core Migrations.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DDL to Entity Framework Core configuration
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DBCONTEXT CONFIGURATION
-- ================================================================================================
-- In Entity Framework Core, database schema is managed through DbContext configuration
-- and automatic migrations. The following shows how UC002B.1 entities are configured.
-- ================================================================================================

/*
    AppDbContext Configuration (from AppDbContext.cs)
    
    Entity Framework Core automatically manages the database schema through:
    1. DbSet properties define which entities are tracked
    2. OnModelCreating() configures entity relationships and conversions
    3. Migrations generate and apply SQL DDL changes
*/

-- ================================================================================================
-- DBSET PROPERTIES (Entities tracked by EF Core)
-- ================================================================================================

/*
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; }
    
    NOTE: The NATURE_CHECK_CASES table is created when EF Core Migrations are run.
    The table is defined by:
    - Entity: NatureCheckCase (Domain/Entities/NatureCheckCase.cs)
    - Configuration: NatureCheckCaseConfiguration (Infrastructure/Persistence/Configurations/NatureCheckCaseConfiguration.cs)
    - DbSet: AppDbContext.NatureCheckCases (Infrastructure/Persistence/AppDbContext.cs)
    
    When you run: Add-Migration <MigrationName> and Update-Database,
    EF Core generates and applies the SQL DDL to create the table.
*/

-- ================================================================================================
-- AUTOMATIC INCLUSION (Navigation Properties)
-- ================================================================================================

/*
    EF Core AutoInclude configuration ensures related entities are loaded automatically:
    
    modelBuilder.Entity<Person>().Navigation(e => e.Role).AutoInclude();
    modelBuilder.Entity<Person>().Navigation(e => e.Address).AutoInclude();
    modelBuilder.Entity<Farm>().Navigation(e => e.Owner).AutoInclude();
    modelBuilder.Entity<Farm>().Navigation(e => e.Address).AutoInclude();
    
    This means when loading Person or Farm entities, related Role and Address entities
    are automatically included without explicit .Include() calls.
*/

-- ================================================================================================
-- ENTITY CONFIGURATIONS
-- ================================================================================================

/*
    NatureCheckCase Entity Configuration:
    
    - Status: Enum (NatureCheckCaseStatus) converted to NVARCHAR(50) via ValueConverter
    - CreatedAt: DateTimeOffset converted to DATETIME2
    - AssignedAt: DateTimeOffset? converted to DATETIME2 (nullable)
    
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.Status)
        .HasColumnType("nvarchar(50)")
        .HasConversion(statusConverter);
    
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.CreatedAt)
        .HasConversion(
            v => v.DateTime,  // Convert DateTimeOffset to DateTime when saving
            v => new DateTimeOffset(v, TimeSpan.Zero));  // Convert DateTime to DateTimeOffset when reading
*/

-- ================================================================================================
-- FOREIGN KEY RELATIONSHIPS (Managed by EF Core)
-- ================================================================================================

/*
    EF Core automatically creates foreign key relationships based on navigation properties:
    
    NatureCheckCase:
    - FarmId → Farms.Id (via Farm navigation property)
    - ConsultantId → Persons.Id (via Consultant navigation property)
    - AssignedByPersonId → Persons.Id (via AssignedByPerson navigation property)
    
    Farm:
    - OwnerId → Persons.Id (via Owner navigation property)
    - AddressId → Addresses.Id (via Address navigation property)
    
    Person:
    - RoleId → Roles.Id (via Role navigation property)
    - AddressId → Addresses.Id (via Address navigation property)
*/

-- ================================================================================================
-- INDEXES (Created by EF Core Migrations)
-- ================================================================================================

/*
    EF Core can create indexes through Fluent API or data annotations:
    
    - Indexes are typically created automatically for foreign keys
    - Additional indexes can be configured in OnModelCreating():
    
    modelBuilder.Entity<NatureCheckCase>()
        .HasIndex(c => c.FarmId);
    
    modelBuilder.Entity<NatureCheckCase>()
        .HasIndex(c => c.ConsultantId);
    
    modelBuilder.Entity<NatureCheckCase>()
        .HasIndex(c => c.Status);
*/

-- ================================================================================================
-- MIGRATIONS
-- ================================================================================================

/*
    To create or update database schema:
    
    1. Add-Migration <MigrationName>  (in Package Manager Console)
    2. Update-Database                 (applies migration to database)
    
    EF Core generates SQL DDL automatically based on entity configurations.
    No manual SQL DDL scripts are needed when using Entity Framework Core.
*/

-- ================================================================================================
-- NOTES
-- ================================================================================================

/*
    Key differences from standard SQL DDL:
    
    1. No Views: EF Core uses LINQ queries in repositories instead of database views
    2. No Stored Procedures: Business logic is in service layer, data access via repositories
    3. Automatic Schema Management: Migrations handle all DDL changes
    4. Type Conversions: EF Core handles enum/DateTimeOffset conversions automatically
    5. Navigation Properties: Relationships are managed through object references, not just foreign keys
    
    For UC002B.1, the following repositories are used:
    - IFarmRepository: GetAllAsync() - loads all farms
    - IPersonRepository: GetPersonsByRoleAsync("Consultant") - loads consultants
    - IAddressRepository: GetAllAsync() - loads addresses
    - INatureCheckCaseRepository: GetActiveCasesAsync() - loads active cases
*/

PRINT 'UC002B.1 Entity Framework Core configuration documented.';
PRINT 'Note: Actual database schema is managed via EF Core Migrations, not this script.';
GO
