
/***********************************************************************
-- Add data for UC002B - Assign Nature Check Case to Consultant
-- This script creates sample Nature Check Case assignments
-- Prerequisites: UC002-DML.sql must have been run to create farms, persons, and consultants
***********************************************************************/

SET NOCOUNT ON;

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

-- Clean target table to ensure deterministic sample data
PRINT 'Cleaning target table [NatureCheckCases]...';
IF OBJECT_ID(N'[dbo].[NatureCheckCases]', N'U') IS NOT NULL
    DELETE FROM [dbo].[NatureCheckCases];
GO

-- Get IDs for consultants (from UC002-DML.sql sample data)
DECLARE @Consultant1 UNIQUEIDENTIFIER;
DECLARE @Consultant2 UNIQUEIDENTIFIER;
DECLARE @Consultant3 UNIQUEIDENTIFIER;
DECLARE @Consultant4 UNIQUEIDENTIFIER;
DECLARE @Consultant5 UNIQUEIDENTIFIER;

-- Get consultant IDs by email (from UC002 sample data)
SELECT @Consultant1 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'camilla.jepsen@example.dk';
SELECT @Consultant2 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'maria.andersen@example.dk';
SELECT @Consultant3 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'louise.mortensen@example.dk';
SELECT @Consultant4 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'ida.holst@example.dk';
SELECT @Consultant5 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'anna.bonde@example.dk';

-- Get IDs for Arla employees (from UC002-DML.sql sample data)
DECLARE @Employee1 UNIQUEIDENTIFIER;
DECLARE @Employee2 UNIQUEIDENTIFIER;

-- Get employee IDs by email (from UC002 sample data)
SELECT @Employee1 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'christian.holm@example.dk';
SELECT @Employee2 = [Id] FROM [dbo].[Persons] WHERE [Email] = N'sofie.kjaer@example.dk';

-- Get farm IDs by name (from UC002 sample data)
DECLARE @Farm1 UNIQUEIDENTIFIER;
DECLARE @Farm2 UNIQUEIDENTIFIER;
DECLARE @Farm3 UNIQUEIDENTIFIER;
DECLARE @Farm4 UNIQUEIDENTIFIER;
DECLARE @Farm5 UNIQUEIDENTIFIER;
DECLARE @Farm6 UNIQUEIDENTIFIER;
DECLARE @Farm7 UNIQUEIDENTIFIER;
DECLARE @Farm8 UNIQUEIDENTIFIER;

SELECT @Farm1 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Søndergaard Farm';
SELECT @Farm2 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Bakken Farm';
SELECT @Farm3 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Engholm Farm';
SELECT @Farm4 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Lynggård Farm';
SELECT @Farm5 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Vester Farm';
SELECT @Farm6 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Skovgård Farm';
SELECT @Farm7 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Bølling Farm';
SELECT @Farm8 = [Id] FROM [dbo].[Farms] WHERE [Name] = N'Havbakken Farm';

-- Verify that we have the required data
IF @Consultant1 IS NULL OR @Employee1 IS NULL OR @Farm1 IS NULL
BEGIN
    PRINT 'ERROR: Required sample data from UC002-DML.sql not found.';
    PRINT 'Please run UC002-DML.sql first to create farms, consultants, and employees.';
    RETURN;
END

-- Insert sample Nature Check Cases
PRINT 'Inserting sample Nature Check Cases...';

-- Case 1: Assigned to Consultant 1 (Camilla) for Farm 1
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm1, @Consultant1, @Employee1, N'Assigned', N'Spring 2024 batch - Priority case', N'High', 
     DATEADD(DAY, -5, SYSUTCDATETIME()), DATEADD(DAY, -5, SYSUTCDATETIME()));

-- Case 2: Assigned to Consultant 2 (Maria) for Farm 2
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm2, @Consultant2, @Employee1, N'Assigned', N'Standard check', N'Medium', 
     DATEADD(DAY, -4, SYSUTCDATETIME()), DATEADD(DAY, -4, SYSUTCDATETIME()));

-- Case 3: Assigned to Consultant 1 (Camilla) for Farm 3 - In Progress
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm3, @Consultant1, @Employee1, N'InProgress', N'Follow-up from previous year', N'Medium', 
     DATEADD(DAY, -10, SYSUTCDATETIME()), DATEADD(DAY, -10, SYSUTCDATETIME()));

-- Case 4: Assigned to Consultant 3 (Louise) for Farm 4
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm4, @Consultant3, @Employee2, N'Assigned', N'New farm - Initial assessment needed', N'High', 
     DATEADD(DAY, -2, SYSUTCDATETIME()), DATEADD(DAY, -2, SYSUTCDATETIME()));

-- Case 5: Assigned to Consultant 2 (Maria) for Farm 5 - Completed
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm5, @Consultant2, @Employee1, N'Completed', N'Completed successfully', N'Medium', 
     DATEADD(DAY, -30, SYSUTCDATETIME()), DATEADD(DAY, -30, SYSUTCDATETIME()));

-- Case 6: Assigned to Consultant 4 (Ida) for Farm 6
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm6, @Consultant4, @Employee2, N'Assigned', NULL, N'Low', 
     DATEADD(DAY, -1, SYSUTCDATETIME()), DATEADD(DAY, -1, SYSUTCDATETIME()));

-- Case 7: Assigned to Consultant 5 (Anna) for Farm 7
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm7, @Consultant5, @Employee1, N'Assigned', N'Urgent - Time sensitive', N'Urgent', 
     SYSUTCDATETIME(), SYSUTCDATETIME());

-- Case 8: Assigned to Consultant 1 (Camilla) for Farm 8 - Cancelled
INSERT INTO [dbo].[NatureCheckCases] 
    ([Id], [FarmId], [ConsultantId], [AssignedByPersonId], [Status], [Notes], [Priority], [CreatedAt], [AssignedAt])
VALUES 
    (NEWID(), @Farm8, @Consultant1, @Employee2, N'Cancelled', N'Farm withdrew from program', N'Low', 
     DATEADD(DAY, -15, SYSUTCDATETIME()), DATEADD(DAY, -15, SYSUTCDATETIME()));

PRINT 'Sample Nature Check Cases inserted successfully.';
GO

-- Verification queries
PRINT 'Verification queries:';
PRINT '';

-- Show all Nature Check Cases with related information
SELECT 
    nc.[Id] AS CaseId,
    f.[Name] AS FarmName,
    f.[CVR] AS FarmCVR,
    CONCAT(p_consultant.[FirstName], ' ', p_consultant.[LastName]) AS ConsultantName,
    p_consultant.[Email] AS ConsultantEmail,
    CONCAT(p_employee.[FirstName], ' ', p_employee.[LastName]) AS AssignedBy,
    nc.[Status],
    nc.[Priority],
    nc.[Notes],
    nc.[CreatedAt],
    nc.[AssignedAt]
FROM [dbo].[NatureCheckCases] nc
    INNER JOIN [dbo].[Farms] f ON nc.[FarmId] = f.[Id]
    INNER JOIN [dbo].[Persons] p_consultant ON nc.[ConsultantId] = p_consultant.[Id]
    INNER JOIN [dbo].[Persons] p_employee ON nc.[AssignedByPersonId] = p_employee.[Id]
ORDER BY nc.[CreatedAt] DESC;

PRINT '';
PRINT 'Summary by Status:';
SELECT 
    [Status],
    COUNT(*) AS CaseCount
FROM [dbo].[NatureCheckCases]
GROUP BY [Status]
ORDER BY [Status];

PRINT '';
PRINT 'Summary by Consultant:';
SELECT 
    CONCAT(p.[FirstName], ' ', p.[LastName]) AS ConsultantName,
    COUNT(*) AS CaseCount
FROM [dbo].[NatureCheckCases] nc
    INNER JOIN [dbo].[Persons] p ON nc.[ConsultantId] = p.[Id]
GROUP BY p.[FirstName], p.[LastName]
ORDER BY CaseCount DESC;

PRINT '';
PRINT 'UC002B DML script completed successfully.';
GO

