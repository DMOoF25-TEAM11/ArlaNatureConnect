/*
    File: UC002B.5-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.5 - Delete Farm
    Safety: This document shows how Entity Framework Core handles database schema for farm deletion.
    
    Use Case: UC002B.5 - Delete Farm
    Description: Shows Entity Framework Core DbContext configuration for deleting farms.
    
    Dependencies: Requires entities from UC001, UC002, and UC002B.2
    
    Note: This is documentation showing EF Core approach. Actual database schema is managed via EF Core Migrations.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DDL to Entity Framework Core configuration
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DBCONTEXT CONFIGURATION
-- ================================================================================================
-- UC002B.5 uses existing database schema. No new tables or columns are added.
-- Farm deletion uses existing Farm and NatureCheckCase entities.
-- ================================================================================================

/*
    ================================================================================================
    DELETE OPERATIONS
    ================================================================================================
    
    EF Core handles farm deletion through:
    1. Validate farm has no active cases (business rule in service layer)
    2. Call repository DeleteAsync() method
    3. EF Core generates DELETE SQL automatically
    4. Foreign key CASCADE rules handle related data (if configured)
*/

/*
    ================================================================================================
    FOREIGN KEY CASCADE RULES
    ================================================================================================
    
    If foreign keys are configured with ON DELETE CASCADE:
    - Deleting a farm will automatically delete related NatureCheckCases
    - This is handled by database, not EF Core code
    
    Current implementation validates active cases before deletion
    to prevent accidental deletion of farms with active assignments.
*/

PRINT 'UC002B.5 Entity Framework Core configuration documented.';
PRINT 'Note: Farm deletion uses existing entities - no new schema needed.';
GO
