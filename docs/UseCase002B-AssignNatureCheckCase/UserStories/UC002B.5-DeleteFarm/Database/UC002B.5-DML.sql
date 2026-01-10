/*
    File: UC002B.5-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.5 - Delete Farm
    Safety: This document shows how Entity Framework Core handles farm deletion.
    
    Use Case: UC002B.5 - Delete Farm
    Description: Shows how EF Core repositories and services delete farms.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.2
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA DELETION
-- ================================================================================================
-- The following shows how EF Core repositories and services delete farms.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.DeleteFarmAsync()
    ================================================================================================
    
    // From NatureCheckCaseService.cs
    
    public async Task DeleteFarmAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        // 1. Validate farm exists
        Farm? farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken);
        if (farm == null)
        {
            return;  // Farm doesn't exist, nothing to delete
        }
        
        // 2. Check if farm has active cases (business rule)
        bool hasActiveCase = await _natureCheckCaseRepository
            .FarmHasActiveCaseAsync(farm.Id, cancellationToken);
        
        if (hasActiveCase)
        {
            throw new InvalidOperationException(
                "Gården har aktive Natur Check opgaver og kan ikke slettes.");
        }
        
        // 3. Delete farm via repository
        await _farmRepository.DeleteAsync(farmId, cancellationToken);
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: Repository.DeleteAsync()
    ================================================================================================
    
    // From Repository.cs (base class)
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        TEntity? entity = await ctx.Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false);
        
        if (entity == null)
        {
            return;  // Entity doesn't exist
        }
        
        ctx.Set<TEntity>().Remove(entity);
        await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    
    EF Core automatically:
    - Generates DELETE SQL statement
    - Handles foreign key CASCADE rules (if configured in database)
    - Returns without error if entity doesn't exist
*/

/*
    ================================================================================================
    BUSINESS RULE VALIDATION
    ================================================================================================
    
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
    
    This validation prevents deletion of farms with active cases.
    Business rule enforced in service layer, not database.
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No Stored Procedures: Business logic in service layer, not database
    2. No Triggers: Validation handled in service layer
    3. Entity Deletion: Remove C# objects, EF Core handles SQL DELETE
    4. Business Rules: Validation in service layer before deletion
    5. Repository Pattern: Data access through repository interfaces, not direct SQL
    6. Change Tracking: EF Core tracks deletions and generates SQL automatically
    7. CASCADE Handling: Foreign key CASCADE rules handled by database, not EF Core code
*/

PRINT 'UC002B.5 Entity Framework Core data deletion documented.';
GO