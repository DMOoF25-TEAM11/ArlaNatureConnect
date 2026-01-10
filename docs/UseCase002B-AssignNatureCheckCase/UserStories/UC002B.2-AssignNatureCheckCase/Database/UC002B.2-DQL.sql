/*
    File: UC002B.2-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.2 - Assign Nature Check Case to Consultant
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.2.
    
    Use Case: UC002B.2 - Assign Nature Check Case to Consultant
    Description: Shows EF Core LINQ queries for case assignment and notification generation.
    
    Note: This is documentation showing EF Core LINQ approach. Actual queries are in repository/service code.
    
    IMPORTANT: Notifications are queried from NatureCheckCase data, not a separate table.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DQL to Entity Framework Core LINQ queries
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE LINQ QUERIES
-- ================================================================================================
-- The following shows EF Core LINQ queries that EF Core translates to SQL automatically.
-- ================================================================================================

/*
    ================================================================================================
    QUERY 1: Validate Farm Exists
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Farms WHERE Id = @farmId
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    Farm? farm = await ctx.Set<Farm>()
        .FirstOrDefaultAsync(f => f.Id == farmId, cancellationToken);
    
    Generated SQL (approximate):
    SELECT TOP 1 * FROM Farms WHERE Id = @farmId
*/

/*
    ================================================================================================
    QUERY 2: Validate Consultant Exists and Has Consultant Role
    ================================================================================================
    
    SQL Equivalent:
    SELECT p.*, r.*
    FROM Persons p
    INNER JOIN Roles r ON p.RoleId = r.Id
    WHERE p.Id = @consultantId AND r.Name = 'Consultant'
    
    EF Core LINQ (in NatureCheckCaseService.cs):
    
    Person? consultant = await _personRepository.GetByIdAsync(consultantId, cancellationToken);
    Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken);
    
    if (!string.Equals(consultantRole?.Name, RoleName.Consultant.ToString(), StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Den valgte person har ikke konsulent-rollen.");
    }
    
    EF Core automatically includes:
    - Person.Role via AutoInclude when loading Person
    
    Generated SQL (approximate):
    SELECT p.*, r.* FROM Persons p
    INNER JOIN Roles r ON p.RoleId = r.Id
    WHERE p.Id = @consultantId
*/

/*
    ================================================================================================
    QUERY 3: Check if Farm Has Active Case
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
    
    // Filter in memory after enum conversion
    return farmCases.Any(c =>
        c.Status == NatureCheckCaseStatus.Assigned ||
        c.Status == NatureCheckCaseStatus.InProgress);
    
    Generated SQL (approximate):
    SELECT * FROM NatureCheckCases WHERE FarmId = @farmId
    (Status filtering happens in memory after enum conversion)
*/

/*
    ================================================================================================
    QUERY 4: Create Nature Check Case (INSERT)
    ================================================================================================
    
    SQL Equivalent:
    INSERT INTO NatureCheckCases (Id, FarmId, ConsultantId, AssignedByPersonId, Status, Notes, Priority, CreatedAt, AssignedAt)
    VALUES (@Id, @FarmId, @ConsultantId, @AssignedByPersonId, 'Assigned', @Notes, @Priority, @CreatedAt, @AssignedAt)
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    ctx.Set<NatureCheckCase>().Add(entity);
    await ctx.SaveChangesAsync(cancellationToken);
    
    EF Core automatically:
    - Converts Status enum to "Assigned" string
    - Converts DateTimeOffset to DATETIME2
    - Generates INSERT SQL statement
    
    Generated SQL (approximate):
    INSERT INTO NatureCheckCases (Id, FarmId, ConsultantId, AssignedByPersonId, Status, Notes, Priority, CreatedAt, AssignedAt)
    VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)
*/

/*
    ================================================================================================
    QUERY 5: Get Notifications for Consultant (NOT from Notifications table)
    ================================================================================================
    
    IMPORTANT: In EF Core implementation, notifications are NOT queried from a Notifications table.
    Instead, they are generated from NatureCheckCase data.
    
    SQL Equivalent (if Notifications table existed):
    SELECT * FROM Notifications WHERE ConsultantId = @consultantId AND IsRead = 0
    
    EF Core LINQ (in NatureCheckCaseRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    return await ctx.NatureCheckCases
        .AsNoTracking()
        .Where(c => c.ConsultantId == consultantId && 
                   c.Status == NatureCheckCaseStatus.Assigned)
        .OrderByDescending(c => c.AssignedAt ?? c.CreatedAt)
        .ToListAsync(cancellationToken);
    
    Generated SQL (approximate):
    SELECT * FROM NatureCheckCases
    WHERE ConsultantId = @consultantId
    AND Status = 'Assigned'
    ORDER BY COALESCE(AssignedAt, CreatedAt) DESC
    
    Then in service layer, cases are converted to ConsultantNotificationDto:
    
    foreach (NatureCheckCase caseEntity in assignedCases)
    {
        notifications.Add(new ConsultantNotificationDto
        {
            CaseId = caseEntity.Id,
            FarmId = caseEntity.FarmId,
            FarmName = farm.Name,  // Loaded separately
            AssignedAt = caseEntity.AssignedAt ?? caseEntity.CreatedAt,
            Priority = caseEntity.Priority,
            Notes = caseEntity.Notes
        });
    }
    
    No query to Notifications table - notifications are in-memory DTOs created from cases.
*/

/*
    ================================================================================================
    QUERY 6: Load Farms for Notifications
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Farms WHERE Id IN (@farmId1, @farmId2, ...)
    
    EF Core LINQ (in NatureCheckCaseService.cs):
    
    HashSet<Guid> farmIds = assignedCases.Select(c => c.FarmId).ToHashSet();
    List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken))
        .Where(f => farmIds.Contains(f.Id))
        .ToList();
    
    Generated SQL (approximate):
    SELECT * FROM Farms WHERE Id IN (@p0, @p1, @p2, ...)
    
    EF Core automatically includes:
    - Farm.Owner via AutoInclude
    - Farm.Address via AutoInclude
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DQL
    ================================================================================================
    
    1. No Notifications Table Queries: Notifications generated from NatureCheckCase data
    2. LINQ Syntax: Queries written in C# LINQ, not SQL
    3. Automatic Translation: EF Core translates LINQ to SQL automatically
    4. Type Safety: Compile-time checking, not runtime SQL errors
    5. Navigation Properties: Use object references (case.Farm) instead of JOINs
    6. AutoInclude: Related entities loaded automatically via configuration
    7. In-Memory Processing: DTO creation done in memory after data loaded
    8. Value Converters: Enum/DateTimeOffset conversions handled automatically
    9. AsNoTracking: Read-only queries don't track changes (better performance)
    10. Repository Pattern: Queries encapsulated in repositories, not direct SQL
*/

/*
    ================================================================================================
    NOTIFICATION GENERATION FLOW
    ================================================================================================
    
    1. Query NatureCheckCases for consultant:
       GetAssignedCasesForConsultantAsync(consultantId)
       → Returns List<NatureCheckCase> where ConsultantId matches and Status = Assigned
    
    2. Load related Farms:
       GetAllAsync() → Filter by farm IDs from cases
    
    3. Convert to DTOs in memory:
       foreach (case in cases) {
           Create ConsultantNotificationDto from case + farm data
       }
    
    4. Return DTOs sorted by AssignedAt
    
    No database queries to Notifications table - all data comes from NatureCheckCases.
*/

PRINT 'UC002B.2 Entity Framework Core query examples documented.';
PRINT 'Note: Notifications are queried from NatureCheckCase data, not a separate table.';
GO
