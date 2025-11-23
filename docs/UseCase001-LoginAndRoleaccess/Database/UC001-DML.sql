/*
 File: UC001-DML.sql
 Purpose: Insert sample login sessions for UC001 - Login & Role Access
 Depends on: UC002-DDL.sql and UC002-DML.sql

 created:2025-11-04
 change log:
 2025-11-04: Initial creation
*/

SET NOCOUNT ON;

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

-- Safety check
IF OBJECT_ID(N'[dbo].[Persons]', N'U') IS NULL
BEGIN
    RAISERROR('UC001 requires UC002-DDL.sql and UC002-DML.sql to have been executed first.',16,1);
    RETURN;
END

IF OBJECT_ID(N'[dbo].[LoginSession]', N'U') IS NULL
BEGIN
    RAISERROR('LoginSession table missing. Run UC001-DDL.sql first.',16,1);
    RETURN;
END

PRINT 'Inserting login sessions for all users from UC002...';

-- Farmers (create active sessions)
INSERT INTO [dbo].[LoginSession] (UserId, LoginTimestamp)
SELECT [Id], DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 60), SYSUTCDATETIME())
FROM [dbo].[Persons]
WHERE [RoleId] IN (SELECT [Id] FROM [dbo].[Roles] WHERE [Name] = N'Farmer');

-- Consultants (simulate recent logins)
INSERT INTO [dbo].[LoginSession] (UserId, LoginTimestamp)
SELECT [Id], DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 120), SYSUTCDATETIME())
FROM [dbo].[Persons]
WHERE [RoleId] IN (SELECT [Id] FROM [dbo].[Roles] WHERE [Name] = N'Consultant');

-- Employees (simulate older sessions that include logout time)
INSERT INTO [dbo].[LoginSession] (UserId, LoginTimestamp, LogoutTimestamp)
SELECT [Id],
       DATEADD(HOUR, -ABS(CHECKSUM(NEWID()) % 24), SYSUTCDATETIME()),
       DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 60), SYSUTCDATETIME())
FROM [dbo].[Persons]
WHERE [RoleId] IN (SELECT [Id] FROM [dbo].[Roles] WHERE [Name] IN (N'Employee'));

PRINT 'Login sessions successfully generated for Farmers, Consultants, and Employees.';

-- Optional: verify counts
SELECT r.Name AS RoleName, COUNT(s.Id) AS SessionCount
FROM [dbo].[LoginSession] s
JOIN [dbo].[Persons] u ON s.UserId = u.Id
JOIN [dbo].[Roles] r ON u.RoleId = r.Id
GROUP BY r.Name;
GO
