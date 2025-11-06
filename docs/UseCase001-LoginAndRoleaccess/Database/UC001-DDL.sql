/*
 File: UC001-DDL.sql
 Purpose: Database DDL for Use Case UC001 - Login & Role Access
 Safety: Depends on UC002-DDL.sql (which defines Roles, Users, Address, Farms)
 
 Use Case: UC001 - Login & Role Access
 Description:
   Extends the existing database schema from UC002 by adding the LoginSession table
   for tracking user login and logout events. Ensures that required base tables from
   UC002 exist before proceeding.

 created: 2025-11-05
 change log:
    - 2025-11-05: Initial creation with dependency checks for UC002 schema
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT 'Using existing database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

/***************************************************************************************************
 Pre-checks to ensure UC002 schema exists
***************************************************************************************************/
PRINT 'Checking for required UC002 tables...';

IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    RAISERROR('Missing table [Roles]. UC001 requires UC002-DDL.sql to be executed first.', 16, 1);
    RETURN;
END

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    RAISERROR('Missing table [Users]. UC001 requires UC002-DDL.sql to be executed first.', 16, 1);
    RETURN;
END

PRINT 'UC002 schema validated. Continuing with UC001 setup...';
GO

/***************************************************************************************************
 Table: LoginSession
 Purpose: Tracks login and logout timestamps for each User.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[LoginSession]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[LoginSession]...';

    CREATE TABLE [dbo].[LoginSession] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [LoginTimestamp] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [LogoutTimestamp] DATETIME2 NULL,
        CONSTRAINT [FK_LoginSession_Users] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users]([Id])
            ON DELETE CASCADE
            ON UPDATE CASCADE
    );

    CREATE INDEX [IX_LoginSession_UserId] ON [dbo].[LoginSession]([UserId]);
    CREATE INDEX [IX_LoginSession_LoginTimestamp] ON [dbo].[LoginSession]([LoginTimestamp]);

    PRINT '[LoginSession] table created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[LoginSession] already exists. Skipping creation.';
END
GO

/***************************************************************************************************
 View: vw_UserRoleSessions
 Purpose: Provides a combined view for role-based dashboards (UC001)
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[vw_UserRoleSessions]', N'V') IS NULL
BEGIN
    PRINT 'Creating view [vw_UserRoleSessions]...';
    EXEC('
        CREATE VIEW [dbo].[vw_UserRoleSessions]
        AS
        SELECT 
            u.Id AS UserId,
            u.FirstName,
            u.LastName,
            r.Name AS RoleName,
            s.LoginTimestamp,
            s.LogoutTimestamp
        FROM [dbo].[Users] u
        INNER JOIN [dbo].[Roles] r ON u.RoleId = r.Id
        LEFT JOIN [dbo].[LoginSession] s ON u.Id = s.UserId;
    ');
    PRINT '[vw_UserRoleSessions] view created successfully.';
END
ELSE
BEGIN
    PRINT 'View [vw_UserRoleSessions] already exists. Skipping creation.';
END
GO

/***************************************************************************************************
 Stored Procedure: uspCreateLoginSession
 Purpose: Standardized insertion of login sessions for UC001
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[uspCreateLoginSession]', N'P') IS NULL
BEGIN
    PRINT 'Creating stored procedure [uspCreateLoginSession]...';
    EXEC('
        CREATE PROCEDURE [dbo].[uspCreateLoginSession]
            @UserId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;
            INSERT INTO [dbo].[LoginSession] ([UserId], [LoginTimestamp])
            VALUES (@UserId, SYSUTCDATETIME());
        END
    ');
    PRINT '[uspCreateLoginSession] created successfully.';
END
GO

