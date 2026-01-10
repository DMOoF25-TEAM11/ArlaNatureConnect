/*
    File: UC002B.2-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.2 - Assign Nature Check Case to Consultant
    Safety: This document shows how Entity Framework Core handles database schema for case assignment.
    
    Use Case: UC002B.2 - Assign Nature Check Case to Consultant
    Description: Shows Entity Framework Core DbContext configuration and entity mappings for creating
    and assigning Nature Check Cases. EF Core handles all database schema management automatically.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.1
    
    Note: This is documentation showing EF Core approach. Actual database schema is managed via EF Core Migrations.
    
    IMPORTANT: In EF Core implementation, there is NO separate NOTIFICATIONS table.
    Notifications are generated from NatureCheckCase data in the service layer.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DDL to Entity Framework Core configuration
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DBCONTEXT CONFIGURATION
-- ================================================================================================
-- In Entity Framework Core, database schema is managed through DbContext configuration
-- and automatic migrations. The following shows how UC002B.2 entities are configured.
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
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; }
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Address> Addresses { get; set; }
    
    NOTE: There is NO DbSet<Notification> because notifications are not stored in database.
    Notifications are generated from NatureCheckCase entities in the service layer.
    
    TABLE CREATION: The NATURE_CHECK_CASES table is created via EF Core Migrations.
    The table is defined by:
    - Entity: NatureCheckCase (Domain/Entities/NatureCheckCase.cs)
    - Configuration: NatureCheckCaseConfiguration (Infrastructure/Persistence/Configurations/NatureCheckCaseConfiguration.cs)
    - DbSet: AppDbContext.NatureCheckCases (Infrastructure/Persistence/AppDbContext.cs)
    
    When you run: Add-Migration <MigrationName> and Update-Database,
    EF Core generates and applies the SQL DDL to create the table.
*/

-- ================================================================================================
-- NATURECHECKCASE ENTITY CONFIGURATION
-- ================================================================================================

/*
    NatureCheckCase Entity Configuration:
    
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.Status)
        .HasColumnType("nvarchar(50)")
        .HasConversion(statusConverter);
    
    Status Enum Conversion:
    - Database stores: NVARCHAR(50) with values: "Assigned", "InProgress", "Completed", "Cancelled"
    - Code uses: NatureCheckCaseStatus enum
    - EF Core ValueConverter handles conversion automatically
    
    DateTimeOffset Conversion:
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.CreatedAt)
        .HasConversion(
            v => v.DateTime,  // Convert DateTimeOffset to DateTime when saving
            v => new DateTimeOffset(v, TimeSpan.Zero));  // Convert DateTime to DateTimeOffset when reading
    
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.AssignedAt)
        .HasConversion(
            v => v.HasValue ? v.Value.DateTime : (DateTime?)null,
            v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
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
    
    Navigation Properties:
    public virtual Farm Farm { get; set; }
    public virtual Person Consultant { get; set; }
    public virtual Person AssignedByPerson { get; set; }
*/

-- ================================================================================================
-- NOTIFICATIONS - NOT A DATABASE TABLE
-- ================================================================================================

/*
    IMPORTANT: In Entity Framework Core implementation, there is NO separate NOTIFICATIONS table.
    
    Instead, notifications are:
    1. Generated from NatureCheckCase entities in the service layer
    2. Created as ConsultantNotificationDto objects (in-memory DTOs)
    3. Returned to UI without being stored in database
    
    Service Method: NatureCheckCaseService.GetNotificationsForConsultantAsync()
    - Queries NatureCheckCases where ConsultantId matches and Status = Assigned
    - Converts cases to ConsultantNotificationDto objects
    - Returns DTOs sorted by AssignedAt date
    
    DTO Structure:
    public class ConsultantNotificationDto
    {
        public Guid CaseId { get; init; }
        public Guid FarmId { get; init; }
        public string FarmName { get; init; }
        public DateTimeOffset AssignedAt { get; init; }
        public string? Priority { get; init; }
        public string? Notes { get; init; }
    }
    
    No database table, no foreign keys, no indexes needed for notifications.
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
    
    These indexes support efficient queries for:
    - Finding cases by farm
    - Finding cases by consultant (used for notifications)
    - Filtering by status
*/

-- ================================================================================================
-- BUSINESS RULES (Enforced in Service Layer, Not Database)
-- ================================================================================================

/*
    In EF Core implementation, business rules are enforced in the service layer:
    
    1. Consultant Role Validation:
       - Checked in NatureCheckCaseService.AssignCaseAsync()
       - Validates consultant has "Consultant" role before assignment
    
    2. No Duplicate Active Cases:
       - Checked via NatureCheckCaseRepository.FarmHasActiveCaseAsync()
       - Service throws InvalidOperationException if farm has active case
    
    3. Priority Format:
       - Converted in service layer (Danish UI → English database)
       - Stored as NVARCHAR in database
    
    4. Status Default:
       - Set to NatureCheckCaseStatus.Assigned in service layer
       - EF Core converts enum to "Assigned" string in database
    
    No CHECK constraints or triggers needed - all validation in service layer.
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
    
    1. No Notifications Table: Notifications generated from NatureCheckCase data
    2. No Stored Procedures: Business logic in service layer, data access via repositories
    3. No Triggers: Validation and business rules in service layer
    4. Automatic Schema Management: Migrations handle all DDL changes
    5. Type Conversions: EF Core handles enum/DateTimeOffset conversions automatically
    6. Navigation Properties: Relationships managed through object references
    7. Service Layer Validation: Business rules enforced in code, not database
    
    For UC002B.2, the following repositories are used:
    - IFarmRepository: GetByIdAsync() - validates farm exists
    - IPersonRepository: GetByIdAsync() - validates consultant exists
    - IRoleRepository: GetByIdAsync() - validates consultant role
    - INatureCheckCaseRepository: 
        - FarmHasActiveCaseAsync() - checks for duplicate active cases
        - AddAsync() - creates new case
*/

PRINT 'UC002B.2 Entity Framework Core configuration documented.';
PRINT 'Note: Actual database schema is managed via EF Core Migrations, not this script.';
PRINT 'IMPORTANT: No NOTIFICATIONS table exists in EF Core implementation.';
GO
