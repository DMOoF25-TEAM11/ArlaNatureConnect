/*
    File: UC002B.4-DQL.sql
    Purpose: Entity Framework Query Examples for Use Case UC002B.4 - Create Farm (Inline)
    Safety: This document shows how Entity Framework Core LINQ queries work for UC002B.4.
    
    Use Case: UC002B.4 - Create Farm (Inline)
    Description: Shows EF Core LINQ queries for farm creation validation.
    
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
    QUERY 1: Check if CVR Already Exists
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Farms WHERE CVR = @cvr
    
    EF Core LINQ (in FarmRepository.cs):
    
    await using AppDbContext ctx = _factory.CreateDbContext();
    return await ctx.Set<Farm>()
        .FirstOrDefaultAsync(f => f.CVR == cvr.Trim(), cancellationToken);
    
    Generated SQL (approximate):
    SELECT TOP 1 * FROM Farms WHERE CVR = @cvr
*/

/*
    ================================================================================================
    QUERY 2: Check if Owner Email Exists
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Persons WHERE Email = @email
    
    EF Core LINQ (in NatureCheckCaseService.cs):
    
    Person? existingOwner = (await _personRepository.GetAllAsync(cancellationToken))
        .FirstOrDefault(p => p.Email == request.OwnerEmail);
    
    Generated SQL (approximate):
    SELECT * FROM Persons WHERE Email = @email
*/

/*
    ================================================================================================
    QUERY 3: Ensure Role Exists (Get or Create)
    ================================================================================================
    
    SQL Equivalent:
    SELECT * FROM Roles WHERE Name = @roleName
    
    EF Core LINQ (in NatureCheckCaseService.cs):
    
    private async Task<Role> EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        Role? role = await ctx.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        
        if (role == null)
        {
            role = new Role { Id = Guid.NewGuid(), Name = roleName };
            ctx.Set<Role>().Add(role);
            await ctx.SaveChangesAsync(cancellationToken);
        }
        
        return role;
    }
    
    Generated SQL (approximate):
    SELECT TOP 1 * FROM Roles WHERE Name = @roleName
    (If not found, INSERT INTO Roles ...)
*/

PRINT 'UC002B.4 Entity Framework Core query examples documented.';
GO
