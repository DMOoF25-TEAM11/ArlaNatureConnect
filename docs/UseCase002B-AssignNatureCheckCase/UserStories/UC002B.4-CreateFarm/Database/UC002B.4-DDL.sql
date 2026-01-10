/*
    File: UC002B.4-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.4 - Create Farm (Inline)
    Safety: This document shows how Entity Framework Core handles database schema for farm creation.
    
    Use Case: UC002B.4 - Create Farm (Inline)
    Description: Shows Entity Framework Core DbContext configuration for creating farms, addresses, and persons.
    
    Dependencies: Requires entities from UC001 and UC002
    
    Note: This is documentation showing EF Core approach. Actual database schema is managed via EF Core Migrations.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DDL to Entity Framework Core configuration
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DBCONTEXT CONFIGURATION
-- ================================================================================================
-- UC002B.4 uses existing database schema. No new tables or columns are added.
-- Farm creation uses existing Farm, Address, Person, and Role entities.
-- ================================================================================================

/*
    ================================================================================================
    FARM CREATION OPERATIONS
    ================================================================================================
    
    EF Core handles farm creation through:
    1. Create Address entity
    2. Create or reuse Person entity (farmer)
    3. Create Farm entity
    4. Save all via repositories - EF Core generates INSERT SQL automatically
*/

/*
    ================================================================================================
    UNIQUE CONSTRAINTS
    ================================================================================================
    
    CVR Uniqueness:
    - Enforced by database UNIQUE constraint on Farms.CVR column
    - EF Core throws DbUpdateException if duplicate CVR is inserted
    - Service layer catches exception and provides user-friendly error message
    
    Email Uniqueness:
    - Enforced by database UNIQUE constraint on Persons.Email column (if exists)
    - EF Core throws DbUpdateException if duplicate email is inserted
*/

PRINT 'UC002B.4 Entity Framework Core configuration documented.';
PRINT 'Note: Farm creation uses existing entities - no new schema needed.';
GO
