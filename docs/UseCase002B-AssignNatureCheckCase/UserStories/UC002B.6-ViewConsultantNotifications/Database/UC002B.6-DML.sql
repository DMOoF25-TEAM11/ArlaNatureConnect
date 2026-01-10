/*
    File: UC002B.6-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.6 - View Consultant Notifications
    Safety: This document shows how Entity Framework Core handles notification queries.
    
    Use Case: UC002B.6 - View Consultant Notifications
    Description: Shows how EF Core repositories and services query notifications.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.2
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    IMPORTANT: In EF Core implementation, notifications are NOT stored in database.
    They are generated from NatureCheckCase data when consultant views them.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA QUERIES (Read-Only)
-- ================================================================================================
-- UC002B.6 is a read-only use case. It queries data for display but does not modify data.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.GetNotificationsForConsultantAsync()
    ================================================================================================
    
    // From NatureCheckCaseService.cs
    
    public async Task<IReadOnlyList<ConsultantNotificationDto>> GetNotificationsForConsultantAsync(
        Guid consultantId, CancellationToken cancellationToken = default)
    {
        // 1. Load assigned cases for consultant
        IReadOnlyList<NatureCheckCase> assignedCases = await _natureCheckCaseRepository
            .GetAssignedCasesForConsultantAsync(consultantId, cancellationToken);
        
        if (assignedCases.Count == 0)
            return Array.Empty<ConsultantNotificationDto>();
        
        // 2. Get all farm IDs from cases
        HashSet<Guid> farmIds = assignedCases.Select(c => c.FarmId).ToHashSet();
        
        // 3. Load all farms in one query
        List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken))
            .Where(f => farmIds.Contains(f.Id))
            .ToList();
        
        Dictionary<Guid, Farm> farmsById = farms.ToDictionary(f => f.Id, f => f);
        
        // 4. Convert cases to notification DTOs
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
        
        // 5. Return sorted by AssignedAt (newest first)
        return notifications.OrderByDescending(n => n.AssignedAt).ToList();
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: GetAssignedCasesForConsultantAsync()
    ================================================================================================
    
    // From NatureCheckCaseRepository.cs
    
    public async Task<IReadOnlyList<NatureCheckCase>> GetAssignedCasesForConsultantAsync(
        Guid consultantId, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        
        return await ctx.NatureCheckCases
            .AsNoTracking()  // Read-only, no change tracking
            .Where(c => c.ConsultantId == consultantId && 
                      c.Status == NatureCheckCaseStatus.Assigned)
            .OrderByDescending(c => c.AssignedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    EF Core automatically:
    - Converts Status enum to string for database query
    - Handles NULL AssignedAt values in ORDER BY
    - Returns entities sorted by assignment date
*/

/*
    ================================================================================================
    NOTIFICATIONS - NOT STORED IN DATABASE
    ================================================================================================
    
    IMPORTANT: In Entity Framework Core implementation:
    
    1. Notifications are NOT stored in a database table
    2. Notifications are generated from NatureCheckCase entities
    3. Returned as ConsultantNotificationDto objects (in-memory DTOs)
    4. No "IsRead" flag - notifications are regenerated each time consultant views them
    
    If "IsRead" functionality were needed, it could be:
    - Added as a property to NatureCheckCase entity
    - Or stored in a separate NotificationReadStatus table
    - Current implementation does not track read status
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No Notifications Table: Notifications generated from NatureCheckCase data
    2. No Stored Procedures: Queries in repository layer, not database
    3. LINQ Queries: EF Core translates LINQ to SQL automatically
    4. DTOs: Data transformed to DTOs for UI, not raw SQL results
    5. In-Memory Processing: DTO creation done in memory after data loaded
    6. Repository Pattern: Queries encapsulated in repositories, not direct SQL
    7. No IsRead Tracking: Current implementation doesn't track read status
*/

PRINT 'UC002B.6 Entity Framework Core notification queries documented.';
PRINT 'Note: Notifications are NOT stored in database - they are generated from NatureCheckCase data.';
GO