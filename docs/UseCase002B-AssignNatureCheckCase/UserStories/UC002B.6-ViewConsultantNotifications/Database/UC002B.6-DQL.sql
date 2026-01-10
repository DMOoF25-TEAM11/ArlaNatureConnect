/*
    File: UC002B.6-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.6 - View Consultant Notifications
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.6.
    
    Use Case: UC002B.6 - View Consultant Notifications
    Description: Shows EF Core LINQ queries for viewing consultant notifications.
    
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
    QUERY 1: Get Assigned Cases for Consultant (Notifications Source)
    ================================================================================================
    
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
    
    Note: Notifications are NOT queried from a Notifications table.
    They are generated from NatureCheckCase entities.
*/

/*
    ================================================================================================
    QUERY 2: Load Farms for Notifications
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
    QUERY 3: Convert Cases to Notification DTOs (In-Memory)
    ================================================================================================
    
    This is done in memory using LINQ after loading entities:
    
    // From NatureCheckCaseService.cs
    
    List<ConsultantNotificationDto> notifications = new();
    foreach (NatureCheckCase caseEntity in assignedCases)
    {
        if (!farmsById.TryGetValue(caseEntity.FarmId, out Farm? farm))
            continue;
        
        notifications.Add(new ConsultantNotificationDto
        {
            CaseId = caseEntity.Id,
            FarmId = caseEntity.FarmId,
            FarmName = farm.Name,
            AssignedAt = caseEntity.AssignedAt ?? caseEntity.CreatedAt,
            Priority = caseEntity.Priority,
            Notes = caseEntity.Notes
        });
    }
    
    // Sort by AssignedAt (newest first)
    return notifications.OrderByDescending(n => n.AssignedAt).ToList();
    
    No SQL generated - this is in-memory LINQ processing.
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
    10. DTOs: Data transformed to DTOs for UI, not raw SQL results
    11. No IsRead Flag: Current implementation doesn't track read status
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
    
    4. Return DTOs sorted by AssignedAt (newest first)
    
    No database queries to Notifications table - all data comes from NatureCheckCases.
*/

PRINT 'UC002B.6 Entity Framework Core query examples documented.';
PRINT 'Note: Notifications are queried from NatureCheckCase data, not a separate table.';
GO
