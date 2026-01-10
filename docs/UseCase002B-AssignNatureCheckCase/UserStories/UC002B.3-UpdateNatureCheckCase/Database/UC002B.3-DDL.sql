/*
    File: UC002B.3-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.3 - Update Nature Check Case Assignment
    Safety: This document shows how Entity Framework Core handles database schema for updating cases.
    
    Use Case: UC002B.3 - Update Nature Check Case Assignment
    Description: Shows Entity Framework Core DbContext configuration for updating Nature Check Cases.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.2
    
    Note: This is documentation showing EF Core approach. Actual database schema is managed via EF Core Migrations.
    
    IMPORTANT: In EF Core implementation, there is NO separate NOTIFICATIONS table.
    Notifications are generated from NatureCheckCase data in the service layer.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DDL to Entity Framework Core configuration
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DBCONTEXT CONFIGURATION
-- ================================================================================================
-- UC002B.3 uses the same database schema as UC002B.2. No new tables or columns are added.
-- Updates are handled through EF Core change tracking and SaveChangesAsync().
-- ================================================================================================

/*
    ================================================================================================
    UPDATE OPERATIONS
    ================================================================================================
    
    EF Core handles updates through change tracking:
    1. Load entity from database
    2. Modify entity properties
    3. Call SaveChangesAsync() - EF Core generates UPDATE SQL automatically
*/

/*
    ================================================================================================
    OPTIMISTIC CONCURRENCY
    ================================================================================================
    
    Note: The current EF Core implementation does NOT use RowVersion for concurrency control.
    Updates are performed directly without concurrency checking.
    
    If concurrency control were needed, it could be added via:
    
    modelBuilder.Entity<NatureCheckCase>()
        .Property(e => e.RowVersion)
        .IsRowVersion();
    
    Then in repository:
    try {
        await ctx.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException) {
        // Handle concurrent modification
    }
*/

PRINT 'UC002B.3 Entity Framework Core configuration documented.';
PRINT 'Note: Updates use EF Core change tracking - no stored procedures needed.';
GO
