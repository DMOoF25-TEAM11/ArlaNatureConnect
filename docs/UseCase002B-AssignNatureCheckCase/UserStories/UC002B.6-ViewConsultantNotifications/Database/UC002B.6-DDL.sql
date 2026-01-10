/*
    File: UC002B.6-DDL.sql
    Purpose: Entity Framework Database Configuration for Use Case UC002B.6 - View Consultant Notifications
    Safety: This document shows how Entity Framework Core handles database schema for viewing notifications.
    
    Use Case: UC002B.6 - View Consultant Notifications
    Description: Shows Entity Framework Core DbContext configuration for querying notifications.
    
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
-- UC002B.6 uses existing database schema. No new tables or columns are added.
-- Notifications are queried from NatureCheckCase entities, not a separate table.
-- ================================================================================================

/*
    ================================================================================================
    NOTIFICATIONS - NOT A DATABASE TABLE
    ================================================================================================
    
    IMPORTANT: In Entity Framework Core implementation, there is NO separate NOTIFICATIONS table.
    
    Instead, notifications are:
    1. Queried from NatureCheckCase entities where ConsultantId matches and Status = Assigned
    2. Converted to ConsultantNotificationDto objects in the service layer
    3. Returned to UI as in-memory DTOs
    
    Service Method: NatureCheckCaseService.GetNotificationsForConsultantAsync()
    - Queries NatureCheckCases for consultant
    - Loads related Farm entities
    - Converts to ConsultantNotificationDto objects
    - Returns DTOs sorted by AssignedAt date
    
    No database table, no indexes, no foreign keys needed for notifications.
*/

PRINT 'UC002B.6 Entity Framework Core configuration documented.';
PRINT 'Note: Notifications are queried from NatureCheckCase data, not a separate table.';
GO
