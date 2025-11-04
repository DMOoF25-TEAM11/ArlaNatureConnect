


SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE master;
GO

-- Drop existing database if exists
IF DB_ID(N'ArlaNatureConnect_Dev') IS NOT NULL
BEGIN
    PRINT 'Dropping existing database [ArlaNatureConnect_Dev]...';
    ALTER DATABASE [ArlaNatureConnect_Dev] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [ArlaNatureConnect_Dev];
END
GO

PRINT 'Creating database [ArlaNatureConnect_Dev]...';
CREATE DATABASE [ArlaNatureConnect_Dev];
GO

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

/***************************************************************************************************
 Table: VERSIONINFO
 Purpose: Tracks the version of the database schema.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[VersionInfo]') IS NULL
BEGIN
    CREATE TABLE [dbo].[VersionInfo] (
        [Id] INT NOT NULL PRIMARY KEY,
        [Version] NVARCHAR(50) NOT NULL,
        [AppliedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

/***************************************************************************************************
 Table: ROLES
 Purpose: Stores user roles.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Roles]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(100) NOT NULL UNIQUE
    );
END
GO

/***************************************************************************************************
    Table: Address
    Purpose: Stores address information.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Address]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Address] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Street] NVARCHAR(200) NULL,
        [City] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(50) NULL,
        [Country] NVARCHAR(100) NULL
    );
END

/***************************************************************************************************
    Table: Farms
    Purpose: Stores farm information.
    Note: `UserId` added to represent primary owner. FK to Users added after Users table creation to avoid forward reference.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Farms]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Farms] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(200) NOT NULL,
        [AddressId] UNIQUEIDENTIFIER NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [CVR] NVARCHAR(50) NULL,
        CONSTRAINT [UQ_Farms_CVR] UNIQUE([CVR])
    );
END

/***************************************************************************************************
    Table: Users
    Purpose: Stores user information.
    Note: `FarmId` removed from Users. Use `Farms.UserId` and/or mapping table `UserFarms` for associations.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Users]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [AddressId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION,
        CONSTRAINT [UQ_Users_Email] UNIQUE([Email]),
        CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]),
        CONSTRAINT [FK_Users_Address] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address]([Id])
    );
    CREATE INDEX [IX_Users_RoleId] ON [dbo].[Users]([RoleId]);
END

-- Add FK from Farms to Users now that Users table exists
IF OBJECT_ID(N'[dbo].[Farms]') IS NOT NULL AND OBJECT_ID(N'[dbo].[Users]') IS NOT NULL
BEGIN
 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Farms_Users')
 BEGIN
 ALTER TABLE [dbo].[Farms]
 ADD CONSTRAINT [FK_Farms_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]);
 CREATE INDEX [IX_Farms_UserId] ON [dbo].[Farms]([UserId]);
 END
END

/***************************************************************************************************
    Table: AuditLog
    Purpose: Stores audit log entries.
***************************************************************************************************/
IF OBJECT_ID(N'dbo.[AuditLog]') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLog] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ActorUserId] UNIQUEIDENTIFIER NULL,
        [TargetUserId] UNIQUEIDENTIFIER NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
        );
    CREATE INDEX [IX_AuditLog_Actor] ON [dbo].[AuditLog]([ActorUserId]);
    CREATE INDEX [IX_AuditLog_Target] ON [dbo].[AuditLog]([TargetUserId]);
END
GO
