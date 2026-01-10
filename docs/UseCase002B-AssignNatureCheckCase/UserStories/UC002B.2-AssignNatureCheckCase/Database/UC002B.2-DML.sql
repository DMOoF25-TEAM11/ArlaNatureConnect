/*
    File: UC002B.2-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.2 - Assign Nature Check Case to Consultant
    Safety: This document shows how Entity Framework Core handles data creation for case assignment.
    
    Use Case: UC002B.2 - Assign Nature Check Case to Consultant
    Description: Shows how EF Core repositories and services create Nature Check Cases.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.1
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    IMPORTANT: In EF Core implementation, notifications are NOT created in database.
    They are generated from NatureCheckCase data in the service layer.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA CREATION
-- ================================================================================================
-- The following shows how EF Core repositories and services create Nature Check Cases.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.AssignCaseAsync()
    ================================================================================================
    
    This is the main entry point for UC002B.2. It orchestrates case creation.
*/

/*
    // From NatureCheckCaseService.cs
    
    public async Task<NatureCheckCase> AssignCaseAsync(
        NatureCheckCaseAssignmentRequest request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Validate farm exists
        Farm? farm = await _farmRepository.GetByIdAsync(request.FarmId, cancellationToken)
            ?? throw new InvalidOperationException("Gården findes ikke længere.");
        
        // 2. Validate consultant exists
        Person? consultant = await _personRepository.GetByIdAsync(request.ConsultantId, cancellationToken)
            ?? throw new InvalidOperationException("Den valgte konsulent findes ikke længere.");
        
        // 3. Validate consultant has Consultant role
        Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken);
        if (!string.Equals(consultantRole?.Name, RoleName.Consultant.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Den valgte person har ikke konsulent-rollen.");
        }
        
        // 4. Validate AssignedByPersonId
        if (request.AssignedByPersonId == Guid.Empty)
        {
            throw new InvalidOperationException("Du mangler at vælge en gyldig Arla medarbejder.");
        }
        
        // 5. Check if farm already has active case
        bool hasActiveCase = await _natureCheckCaseRepository
            .FarmHasActiveCaseAsync(farm.Id, cancellationToken);
        if (hasActiveCase && !request.AllowDuplicateActiveCase)
        {
            throw new InvalidOperationException("Gården har allerede en aktiv Natur Check opgave.");
        }
        
        // 6. Create new NatureCheckCase entity
        NatureCheckCase entity = new()
        {
            Id = Guid.NewGuid(),
            FarmId = farm.Id,
            ConsultantId = consultant.Id,
            AssignedByPersonId = request.AssignedByPersonId,
            Status = NatureCheckCaseStatus.Assigned,  // Enum, converted to "Assigned" string by EF Core
            Notes = request.Notes,
            Priority = request.Priority,  // Already in English format from service layer
            CreatedAt = DateTimeOffset.UtcNow,  // Converted to DATETIME2 by EF Core
            AssignedAt = DateTimeOffset.UtcNow   // Converted to DATETIME2 by EF Core
        };
        
        // 7. Save to database via repository
        await _natureCheckCaseRepository.AddAsync(entity, cancellationToken);
        
        return entity;
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: Repository.AddAsync()
    ================================================================================================
    
    // From Repository.cs (base class)
    
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        ctx.Set<TEntity>().Add(entity);
        await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }
    
    EF Core automatically:
    - Generates INSERT SQL statement
    - Converts enum Status to "Assigned" string
    - Converts DateTimeOffset to DATETIME2
    - Sets CreatedAt and AssignedAt timestamps
    - Returns entity with database-generated values
*/

/*
    ================================================================================================
    NOTIFICATIONS - NOT CREATED IN DATABASE
    ================================================================================================
    
    IMPORTANT: In Entity Framework Core implementation, notifications are NOT created in database.
    
    Instead:
    1. When a case is assigned, only the NatureCheckCase is created
    2. Notifications are generated later when consultant views them
    3. Service method: GetNotificationsForConsultantAsync() queries NatureCheckCases
    4. Converts cases to ConsultantNotificationDto objects (in-memory)
    
    // From NatureCheckCaseService.cs
    
    public async Task<IReadOnlyList<ConsultantNotificationDto>> GetNotificationsForConsultantAsync(
        Guid consultantId, CancellationToken cancellationToken = default)
    {
        // Load assigned cases for consultant
        IReadOnlyList<NatureCheckCase> assignedCases = await _natureCheckCaseRepository
            .GetAssignedCasesForConsultantAsync(consultantId, cancellationToken);
        
        if (assignedCases.Count == 0)
            return Array.Empty<ConsultantNotificationDto>();
        
        // Load farms for the cases
        HashSet<Guid> farmIds = assignedCases.Select(c => c.FarmId).ToHashSet();
        List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken))
            .Where(f => farmIds.Contains(f.Id))
            .ToList();
        
        Dictionary<Guid, Farm> farmsById = farms.ToDictionary(f => f.Id, f => f);
        
        // Convert cases to notification DTOs
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
        
        return notifications.OrderByDescending(n => n.AssignedAt).ToList();
    }
    
    No database INSERT for notifications - they exist only as in-memory DTOs.
*/

/*
    ================================================================================================
    VALIDATION METHODS
    ================================================================================================
*/

/*
    1. FarmHasActiveCaseAsync() - Checks for duplicate active cases
    
    // From NatureCheckCaseRepository.cs
    
    public async Task<bool> FarmHasActiveCaseAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        
        List<NatureCheckCase> farmCases = await ctx.NatureCheckCases
            .AsNoTracking()
            .Where(c => c.FarmId == farmId)
            .ToListAsync(cancellationToken);
        
        // Filter in memory after enum conversion
        return farmCases.Any(c =>
            c.Status == NatureCheckCaseStatus.Assigned ||
            c.Status == NatureCheckCaseStatus.InProgress);
    }
*/

/*
    2. GetByIdAsync() - Validates entities exist
    
    // From Repository.cs (base class)
    
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No Stored Procedures: Business logic in service layer, not database
    2. No Triggers: Validation and notifications handled in service layer
    3. No Notifications Table: Notifications generated from NatureCheckCase data
    4. Entity Creation: Create C# objects, EF Core handles SQL INSERT
    5. Type Conversions: EF Core handles enum/DateTimeOffset conversions automatically
    6. Validation in Code: Business rules enforced in service layer, not database
    7. Repository Pattern: Data access through repository interfaces, not direct SQL
    8. Change Tracking: EF Core tracks changes and generates SQL automatically
*/

/*
    ================================================================================================
    TRANSACTION HANDLING
    ================================================================================================
    
    EF Core automatically handles transactions:
    - SaveChangesAsync() wraps all changes in a transaction
    - If any operation fails, entire transaction is rolled back
    - No explicit BEGIN TRANSACTION / COMMIT / ROLLBACK needed
    
    // Example:
    await using AppDbContext ctx = _factory.CreateDbContext();
    ctx.Set<NatureCheckCase>().Add(entity);
    await ctx.SaveChangesAsync(cancellationToken);  // Automatic transaction
*/

PRINT 'UC002B.2 Entity Framework Core data manipulation documented.';
PRINT 'Note: Notifications are NOT created in database - they are generated from NatureCheckCase data.';
GO
