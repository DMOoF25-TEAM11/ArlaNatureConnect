/*
    File: UC002B.5-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.5 - Delete Farm
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.5.
    
    Use Case: UC002B.5 - Delete Farm
    Description: Shows EF Core LINQ queries for farm deletion validation.
    
    Note: This is documentation showing EF Core LINQ approach. Actual queries are in repository/service code.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DQL to Entity Framework Core LINQ queries
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE LINQ QUERIES
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
    QUERY 2: Check if Farm Has Active Case
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
    (Status filtering happens in memory after enum conversion)
*/

/*
    ================================================================================================
    QUERY 3: Delete Farm (DELETE Statement)
    ================================================================================================
    
    SQL Equivalent:
    DELETE FROM Farms WHERE Id = @farmId
    
    EF Core LINQ (in Repository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    Farm? entity = await ctx.Set<Farm>()
        .FirstOrDefaultAsync(f => f.Id == farmId, cancellationToken);
    
    if (entity != null)
    {
        ctx.Set<Farm>().Remove(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }
    
    EF Core automatically:
    - Generates DELETE SQL statement
    - Handles foreign key CASCADE rules (if configured)
    
    Generated SQL (approximate):
    DELETE FROM Farms WHERE Id = @farmId
*/

PRINT 'UC002B.5 Entity Framework Core query examples documented.';
GO
