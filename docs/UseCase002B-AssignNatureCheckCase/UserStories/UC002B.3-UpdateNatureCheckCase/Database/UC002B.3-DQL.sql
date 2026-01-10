/*
    File: UC002B.3-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.3 - Update Nature Check Case Assignment
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.3.
    
    Use Case: UC002B.3 - Update Nature Check Case Assignment
    Description: Shows EF Core LINQ queries for updating cases.
    
    Note: This is documentation showing EF Core LINQ approach. Actual queries are in repository/service code.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DQL to Entity Framework Core LINQ queries
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE LINQ QUERIES FOR UPDATES
-- ================================================================================================

/*
    ================================================================================================
    QUERY 1: Get Active Case for Farm (Before Update)
    ================================================================================================
    
    SQL Equivalent:
    SELECT TOP 1 * FROM NatureCheckCases
    WHERE FarmId = @farmId 
    AND Status IN ('Assigned', 'InProgress')
    
    EF Core LINQ (in NatureCheckCaseRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    List<NatureCheckCase> farmCases = await ctx.NatureCheckCases
        .Where(c => c.FarmId == farmId)
        .ToListAsync(cancellationToken);
    
    return farmCases.FirstOrDefault(c =>
        c.Status == NatureCheckCaseStatus.Assigned ||
        c.Status == NatureCheckCaseStatus.InProgress);
    
    Generated SQL (approximate):
    SELECT * FROM NatureCheckCases WHERE FarmId = @farmId
    (Status filtering happens in memory after enum conversion)
*/

/*
    ================================================================================================
    QUERY 2: Update Case (UPDATE Statement)
    ================================================================================================
    
    SQL Equivalent:
    UPDATE NatureCheckCases
    SET ConsultantId = @consultantId,
        Priority = @priority,
        Notes = @notes,
        AssignedAt = @assignedAt
    WHERE Id = @caseId
    
    EF Core LINQ (in Repository.cs):
    
    // Load entity (with change tracking)
    await using AppDbContext ctx = _factory.CreateDbContext();
    NatureCheckCase? entity = await ctx.NatureCheckCases
        .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);
    
    // Modify properties
    entity.ConsultantId = newConsultantId;
    entity.Priority = newPriority;
    entity.Notes = newNotes;
    entity.AssignedAt = DateTimeOffset.UtcNow;
    
    // Save changes
    await ctx.SaveChangesAsync(cancellationToken);
    
    EF Core automatically:
    - Tracks property changes
    - Generates UPDATE SQL with only changed properties
    - Converts DateTimeOffset to DATETIME2
    
    Generated SQL (approximate):
    UPDATE NatureCheckCases
    SET ConsultantId = @p0, Priority = @p1, Notes = @p2, AssignedAt = @p3
    WHERE Id = @p4
*/

/*
    ================================================================================================
    QUERY 3: Validate Consultant (Before Update)
    ================================================================================================
    
    Same as UC002B.2 - validates consultant exists and has Consultant role.
    See UC002B.2-DQL.sql for details.
*/

PRINT 'UC002B.3 Entity Framework Core query examples documented.';
GO
