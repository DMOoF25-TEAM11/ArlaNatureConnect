/*
    File: UC002B.3-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.3 - Update Nature Check Case Assignment
    Safety: This document shows how Entity Framework Core handles data updates for case assignment.
    
    Use Case: UC002B.3 - Update Nature Check Case Assignment
    Description: Shows how EF Core repositories and services update Nature Check Cases.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.2
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    IMPORTANT: In EF Core implementation, notifications are NOT updated in database.
    They are regenerated from NatureCheckCase data when consultant views them.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA UPDATES
-- ================================================================================================
-- The following shows how EF Core repositories and services update Nature Check Cases.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.UpdateCaseAsync()
    ================================================================================================
    
    // From NatureCheckCaseService.cs
    
    public async Task<NatureCheckCase> UpdateCaseAsync(
        Guid farmId, 
        NatureCheckCaseAssignmentRequest request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Find active case for farm
        NatureCheckCase? existingCase = await _natureCheckCaseRepository
            .GetActiveCaseForFarmAsync(farmId, cancellationToken);
        
        if (existingCase == null)
        {
            throw new InvalidOperationException("Der findes ingen aktiv Natur Check opgave for denne gård.");
        }
        
        // 2. Validate consultant
        Person? consultant = await _personRepository.GetByIdAsync(request.ConsultantId, cancellationToken)
            ?? throw new InvalidOperationException("Den valgte konsulent findes ikke længere.");
        
        Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken);
        if (!string.Equals(consultantRole?.Name, RoleName.Consultant.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Den valgte person har ikke konsulent-rollen.");
        }
        
        // 3. Update entity properties
        existingCase.ConsultantId = consultant.Id;
        existingCase.Priority = request.Priority;
        existingCase.Notes = request.Notes;
        existingCase.AssignedAt = DateTimeOffset.UtcNow;  // Update assignment time
        
        // 4. Save changes via repository
        await _natureCheckCaseRepository.UpdateAsync(existingCase, cancellationToken);
        
        return existingCase;
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: Repository.UpdateAsync()
    ================================================================================================
    
    // From Repository.cs (base class)
    
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        ctx.Set<TEntity>().Update(entity);
        await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    
    EF Core automatically:
    - Tracks changes to entity properties
    - Generates UPDATE SQL statement
    - Converts DateTimeOffset to DATETIME2
    - Updates only changed properties (or all if Update() is used)
*/

/*
    ================================================================================================
    GET ACTIVE CASE FOR FARM
    ================================================================================================
    
    // From NatureCheckCaseRepository.cs
    
    public async Task<NatureCheckCase?> GetActiveCaseForFarmAsync(
        Guid farmId, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        
        List<NatureCheckCase> farmCases = await ctx.NatureCheckCases
            .Where(c => c.FarmId == farmId)
            .ToListAsync(cancellationToken);
        
        // Find first active case (filter in memory after enum conversion)
        return farmCases.FirstOrDefault(c =>
            c.Status == NatureCheckCaseStatus.Assigned ||
            c.Status == NatureCheckCaseStatus.InProgress);
    }
*/

/*
    ================================================================================================
    NOTIFICATIONS - NOT UPDATED IN DATABASE
    ================================================================================================
    
    IMPORTANT: In Entity Framework Core implementation, notifications are NOT updated in database.
    
    When a case is updated:
    1. Only the NatureCheckCase entity is updated
    2. Notifications are regenerated when consultant views them
    3. Service method: GetNotificationsForConsultantAsync() queries updated cases
    4. Returns fresh ConsultantNotificationDto objects based on current case data
    
    No database UPDATE for notifications - they are in-memory DTOs created from cases.
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No Stored Procedures: Business logic in service layer, not database
    2. No Triggers: No automatic notification updates - handled in service layer
    3. No Notifications Table Updates: Notifications regenerated from NatureCheckCase data
    4. Entity Updates: Modify C# objects, EF Core handles SQL UPDATE
    5. Change Tracking: EF Core tracks changes and generates SQL automatically
    6. Validation in Code: Business rules enforced in service layer
    7. Repository Pattern: Data access through repository interfaces, not direct SQL
*/

PRINT 'UC002B.3 Entity Framework Core data updates documented.';
PRINT 'Note: Notifications are NOT updated in database - they are regenerated from NatureCheckCase data.';
GO