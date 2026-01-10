/*
    File: UC002B.1-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.1 - View Farms and Consultants for Assignment
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.1.
    
    Use Case: UC002B.1 - View Farms and Consultants for Assignment
    Description: Shows EF Core LINQ queries that are equivalent to SQL queries for viewing farms and consultants.
    
    Note: This is documentation showing EF Core LINQ approach. Actual queries are in repository/service code.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DQL to Entity Framework Core LINQ queries
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE LINQ QUERIES
-- ================================================================================================
-- The following shows EF Core LINQ queries that EF Core translates to SQL automatically.
-- These queries are executed through repositories and services, not direct SQL.
-- ================================================================================================

/*
    ================================================================================================
    QUERY 1: Load All Farms with Related Data
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Farms
    INNER JOIN Persons ON Farms.OwnerId = Persons.Id
    INNER JOIN Addresses ON Farms.AddressId = Addresses.Id
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    return await ctx.Set<Farm>()
        .AsNoTracking()  // Read-only, no change tracking
        .ToListAsync(cancellationToken);
    
    EF Core automatically includes:
    - Farm.Owner (Person) via AutoInclude
    - Farm.Address (Address) via AutoInclude
    
    Generated SQL (approximate):
    SELECT f.*, p.*, a.*
    FROM Farms f
    INNER JOIN Persons p ON f.OwnerId = p.Id
    INNER JOIN Addresses a ON f.AddressId = a.Id
*/

/*
    ================================================================================================
    QUERY 2: Load Consultants by Role
    ================================================================================================
    
    SQL Equivalent:
    SELECT p.*, r.*
    FROM Persons p
    INNER JOIN Roles r ON p.RoleId = r.Id
    WHERE r.Name = 'Consultant' AND p.IsActive = 1
    ORDER BY p.FirstName, p.LastName
    
    EF Core LINQ (in PersonRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    // First resolve role
    Role? roleEntity = await ctx.Set<Role>()
        .FirstOrDefaultAsync(r => r.Name.ToLower() == "consultant", ct);
    
    if (roleEntity == null)
        return [];
    
    // Then filter persons
    return await ctx.Set<Person>()
        .Where(p => p.RoleId == roleEntity.Id)
        .ToListAsync(ct);
    
    EF Core automatically includes:
    - Person.Role via AutoInclude
    - Person.Address via AutoInclude
    
    Generated SQL (approximate):
    SELECT p.*, r.*, a.*
    FROM Persons p
    INNER JOIN Roles r ON p.RoleId = r.Id
    LEFT JOIN Addresses a ON p.AddressId = a.Id
    WHERE p.RoleId = @roleId
*/

/*
    ================================================================================================
    QUERY 3: Load Active Nature Check Cases
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM NatureCheckCases
    WHERE Status IN ('Assigned', 'InProgress')
    
    EF Core LINQ (in NatureCheckCaseRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    // Load all cases
    List<NatureCheckCase> allCases = await ctx.NatureCheckCases
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    
    // Filter for active statuses (done in memory after materialization)
    List<NatureCheckCase> activeCases = allCases
        .Where(c => c.Status == NatureCheckCaseStatus.Assigned ||
                   c.Status == NatureCheckCaseStatus.InProgress)
        .ToList();
    
    Note: Filtering is done in memory because EF Core ValueConverter handles enum conversion.
    The Status field is stored as NVARCHAR in database but used as enum in code.
    
    Generated SQL (approximate):
    SELECT * FROM NatureCheckCases
    (Status filtering happens in memory after enum conversion)
*/

/*
    ================================================================================================
    QUERY 4: Check if Farm Has Active Case
    ================================================================================================
    
    SQL Equivalent:
    SELECT COUNT(*) 
    FROM NatureCheckCases
    WHERE FarmId = @farmId 
    AND Status IN ('Assigned', 'InProgress')
    
    EF Core LINQ (in NatureCheckCaseRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    List<NatureCheckCase> farmCases = await ctx.NatureCheckCases
        .AsNoTracking()
        .Where(c => c.FarmId == farmId)
        .ToListAsync(cancellationToken);
    
    return farmCases.Any(c =>
        c.Status == NatureCheckCaseStatus.Assigned ||
        c.Status == NatureCheckCaseStatus.InProgress);
    
    Generated SQL (approximate):
    SELECT * FROM NatureCheckCases WHERE FarmId = @farmId
    (Status filtering happens in memory)
*/

/*
    ================================================================================================
    QUERY 5: Build Farm Assignment Overview (Service Layer)
    ================================================================================================
    
    This is done in memory using LINQ after loading all entities:
    
    // From NatureCheckCaseService.cs
    
    // Group active cases by farm
    Dictionary<Guid, NatureCheckCase> activeCasesByFarm = activeCases
        .GroupBy(c => c.FarmId)
        .Select(g => g.OrderByDescending(c => c.AssignedAt ?? c.CreatedAt).First())
        .ToDictionary(c => c.FarmId, c => c);
    
    // Create DTOs
    List<FarmAssignmentOverviewDto> overview = farms
        .Select(f => CreateFarmOverview(f, personsById, addressesById, 
            activeFarmIds.Contains(f.Id), activeCasesByFarm))
        .OrderBy(f => f.FarmName)
        .ToList();
    
    // Sort consultants
    List<Person> sortedConsultants = consultants
        .OrderBy(c => c.FirstName)
        .ThenBy(c => c.LastName)
        .ToList();
    
    No SQL generated - this is in-memory LINQ processing after data is loaded.
*/

/*
    ================================================================================================
    QUERY 6: Get All Addresses
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Addresses
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    return await ctx.Set<Address>()
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    
    Generated SQL:
    SELECT * FROM Addresses
*/

/*
    ================================================================================================
    QUERY 7: Get All Persons (for building lookup dictionaries)
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Persons
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    return await ctx.Set<Person>()
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    
    EF Core automatically includes:
    - Person.Role via AutoInclude
    - Person.Address via AutoInclude
    
    Generated SQL (approximate):
    SELECT p.*, r.*, a.*
    FROM Persons p
    INNER JOIN Roles r ON p.RoleId = r.Id
    LEFT JOIN Addresses a ON p.AddressId = a.Id
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DQL
    ================================================================================================
    
    1. LINQ Syntax: Queries written in C# LINQ, not SQL
    2. Automatic Translation: EF Core translates LINQ to SQL automatically
    3. Type Safety: Compile-time checking, not runtime SQL errors
    4. Navigation Properties: Use object references (farm.Owner) instead of JOINs
    5. AutoInclude: Related entities loaded automatically via configuration
    6. In-Memory Processing: Some operations (grouping, filtering) done in memory
    7. Value Converters: Enum/DateTimeOffset conversions handled automatically
    8. AsNoTracking: Read-only queries don't track changes (better performance)
    9. DTOs: Data transformed to DTOs for UI, not raw SQL results
    10. Repository Pattern: Queries encapsulated in repositories, not direct SQL
*/

/*
    ================================================================================================
    PERFORMANCE CONSIDERATIONS
    ================================================================================================
    
    1. Materialization: All entities loaded into memory before processing
    2. AsNoTracking: Used for read-only operations to improve performance
    3. Dictionary Lookups: Related entities loaded once, then looked up via dictionaries
    4. In-Memory Filtering: Some filtering done in memory after materialization
    5. AutoInclude: Automatic loading of related entities (can be optimized if needed)
*/

PRINT 'UC002B.1 Entity Framework Core query examples documented.';
PRINT 'Note: Actual queries are executed through repositories and services, not direct SQL.';
GO
