/*
 Project: ArlaNatureConnect
 Database: ArlaNatureConnect_Dev
 Version: 1.0.0
 
 Change Log:
  - 2025-12-19: v1.0.0 - Initial schema creation: VersionInfo, Roles, Addresses, Farms, Persons, AuditLog; added foreign keys and indexes.

  Purpose:
  - Define the initial database schema for ArlaNatureConnect_Dev.

  Fix issue: Table name Address to Addresses so C# can handle navigation properties correctly.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

USE master;

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


/***************************************************************************************************
 Table: ROLES
 Purpose: Stores Person roles.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Roles]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(100) NOT NULL UNIQUE
    );
END


/***************************************************************************************************
    Table: Addresses
    Purpose: Stores addresses information.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Addresses]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Addresses] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Street] NVARCHAR(200) NULL,
        [City] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(50) NULL,
        [Country] NVARCHAR(100) NULL
    );
END

/***************************************************************************************************
    Table: Persons
    Purpose: Stores Person information.
    Note: `FarmId` removed from Persons. Use `Farms.PersonId` and/or mapping table `PersonFarms` for associations.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Persons]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Persons] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [AddressId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [RowVersion] ROWVERSION,
        CONSTRAINT [UQ_Persons_Email] UNIQUE([Email]),
        CONSTRAINT [FK_Persons_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]),
        CONSTRAINT [FK_Persons_Addresses] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Addresses]([Id])
    );
END
GO

-- Create index on Persons.RoleId if it doesn't exist
IF OBJECT_ID(N'[dbo].[Persons]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Persons_RoleId' AND object_id = OBJECT_ID(N'[dbo].[Persons]'))
    BEGIN
        CREATE INDEX [IX_Persons_RoleId] ON [dbo].[Persons]([RoleId]);
    END
END
GO


/***************************************************************************************************
    Table: Farms
    Purpose: Stores farm information.
    Note: `PersonId` added to represent primary owner. FK to Persons added after Persons table creation to avoid forward reference.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[Farms]') IS NULL
BEGIN
    CREATE TABLE [dbo].[Farms] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(200) NOT NULL,
        [AddressId] UNIQUEIDENTIFIER NULL,
        [OwnerId] UNIQUEIDENTIFIER NULL,
        [CVR] NVARCHAR(50) NULL,
        CONSTRAINT [UQ_Farms_CVR] UNIQUE([CVR])
    );
END

/***************************************************************************************************
    Table: NatureAreas
    Purpose: Stores nature area information associated with farms.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[NatureAreas]') IS NULL
BEGIN
    CREATE TABLE [dbo].[NatureAreas] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        [FarmId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Type] NVARCHAR(100) NOT NULL,
        CONSTRAINT [FK_NatureArea_Farm] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms]([Id]) ON DELETE CASCADE
    );
END
GO

-- Create index on NatureAreas.FarmId if it doesn't exist
IF OBJECT_ID(N'[dbo].[NatureAreas]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureArea_FarmId' AND object_id = OBJECT_ID(N'[dbo].[NatureAreas]'))
    BEGIN
        CREATE INDEX [IX_NatureArea_FarmId] ON [dbo].[NatureAreas]([FarmId]);
    END
END
GO

/***************************************************************************************************
    Table: NatureAreaCoordinates
    Purpose: Stores coordinates for nature areas.
 ***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[NatureAreaCoordinates]') IS NULL
BEGIN
    CREATE TABLE [dbo].[NatureAreaCoordinates] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [NatureAreaId] UNIQUEIDENTIFIER NOT NULL,
        [Latitude] FLOAT(53) NOT NULL,
        [Longitude] FLOAT(53) NOT NULL,
        [OrderIndex] INT NOT NULL DEFAULT(0),
        CONSTRAINT [FK_NAC_NatureArea] FOREIGN KEY ([NatureAreaId]) REFERENCES [dbo].[NatureAreas]([Id]) ON DELETE CASCADE
    );
END
GO

-- Create index on NatureAreaCoordinates.NatureAreaId if it doesn't exist
IF OBJECT_ID(N'[dbo].[NatureAreaCoordinates]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NAC_NatureAreaId' AND object_id = OBJECT_ID(N'[dbo].[NatureAreaCoordinates]'))
    BEGIN
        CREATE INDEX [IX_NAC_NatureAreaId] ON [dbo].[NatureAreaCoordinates]([NatureAreaId]);
    END
END
GO

/***************************************************************************************************
    Table: NatureAreaImages
    Purpose: Stores images for nature areas.
 ***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[NatureAreaImages]') IS NULL
BEGIN
    CREATE TABLE [dbo].[NatureAreaImages] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [NatureAreaId] UNIQUEIDENTIFIER NOT NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        CONSTRAINT [FK_NAI_NatureArea] FOREIGN KEY ([NatureAreaId]) REFERENCES [dbo].[NatureAreas]([Id]) ON DELETE CASCADE
    );
END
GO

-- Create index on NatureAreaImages.NatureAreaId if it doesn't exist
IF OBJECT_ID(N'[dbo].[NatureAreaImages]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NAI_NatureAreaId' AND object_id = OBJECT_ID(N'[dbo].[NatureAreaImages]'))
    BEGIN
        CREATE INDEX [IX_NAI_NatureAreaId] ON [dbo].[NatureAreaImages]([NatureAreaId]);
    END
END
GO

-- Add FK from Farms to Persons now that Persons table exists
IF OBJECT_ID(N'[dbo].[Farms]') IS NOT NULL AND OBJECT_ID(N'[dbo].[Persons]') IS NOT NULL
BEGIN
 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Farms_Owner')
 BEGIN
 ALTER TABLE [dbo].[Farms]
 ADD CONSTRAINT [FK_Farms_Owner] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Persons]([Id]);
 END
END
GO

-- Create index on Farms.OwnerId if it doesn't exist
IF OBJECT_ID(N'[dbo].[Farms]') IS NOT NULL
BEGIN
 IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Farms_OwnerId' AND object_id = OBJECT_ID(N'[dbo].[Farms]'))
 BEGIN
     CREATE INDEX [IX_Farms_OwnerId] ON [dbo].[Farms]([OwnerId]);
 END
END
GO

/***************************************************************************************************
    Table: UserFarms
    Purpose: Mapping table to represent many-to-many relationship between Persons and Farms.
    Note: Allows a Person to be associated with multiple Farms and vice versa.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[UserFarms]') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserFarms] (
        [PersonId] UNIQUEIDENTIFIER NOT NULL,
        [FarmId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_UserFarms] PRIMARY KEY ([PersonId], [FarmId]),
        CONSTRAINT [FK_UserFarms_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserFarms_Farms] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms]([Id]) ON DELETE CASCADE
    );
END
GO

-- Create indexes on UserFarms if they don't exist
IF OBJECT_ID(N'[dbo].[UserFarms]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserFarms_PersonId' AND object_id = OBJECT_ID(N'[dbo].[UserFarms]'))
    BEGIN
        CREATE INDEX [IX_UserFarms_PersonId] ON [dbo].[UserFarms]([PersonId]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserFarms_FarmId' AND object_id = OBJECT_ID(N'[dbo].[UserFarms]'))
    BEGIN
        CREATE INDEX [IX_UserFarms_FarmId] ON [dbo].[UserFarms]([FarmId]);
    END
END
GO
GO

/***************************************************************************************************
    Table: AuditLog
    Purpose: Stores audit log entries.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[AuditLog]') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLog] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ActorUserId] UNIQUEIDENTIFIER NULL,
        [TargetUserId] UNIQUEIDENTIFIER NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
        );
END
GO

-- Create indexes on AuditLog if they don't exist
IF OBJECT_ID(N'[dbo].[AuditLog]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_Actor' AND object_id = OBJECT_ID(N'[dbo].[AuditLog]'))
    BEGIN
        CREATE INDEX [IX_AuditLog_Actor] ON [dbo].[AuditLog]([ActorUserId]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_Target' AND object_id = OBJECT_ID(N'[dbo].[AuditLog]'))
    BEGIN
        CREATE INDEX [IX_AuditLog_Target] ON [dbo].[AuditLog]([TargetUserId]);
    END
END
GO

/***************************************************************************************************
    Table: NatureCheckCases
    Purpose: Stores Nature Check Case assignments, linking farms to consultants.
    Note: This table extends the existing database schema from UC001 and UC002.
    Dependencies: Requires Farms and Persons tables to exist (from UC001/UC002).
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[NatureCheckCases]') IS NULL
BEGIN
    CREATE TABLE [dbo].[NatureCheckCases] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [FarmId] UNIQUEIDENTIFIER NOT NULL,
        [ConsultantId] UNIQUEIDENTIFIER NOT NULL,
        [AssignedByPersonId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT N'Assigned',
        [Notes] NVARCHAR(MAX) NULL,
        [Priority] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [AssignedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION,
        
        -- Foreign Key Constraints
        CONSTRAINT [FK_NatureCheckCases_Farms] 
            FOREIGN KEY ([FarmId]) 
            REFERENCES [dbo].[Farms]([Id]) 
            ON DELETE CASCADE,
        
        CONSTRAINT [FK_NatureCheckCases_Consultant] 
            FOREIGN KEY ([ConsultantId]) 
            REFERENCES [dbo].[Persons]([Id]) 
            ON DELETE NO ACTION,
        
        CONSTRAINT [FK_NatureCheckCases_AssignedBy] 
            FOREIGN KEY ([AssignedByPersonId]) 
            REFERENCES [dbo].[Persons]([Id]) 
            ON DELETE NO ACTION
    );
END
GO

-- Create indexes on NatureCheckCases if they don't exist
IF OBJECT_ID(N'[dbo].[NatureCheckCases]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureCheckCases_FarmId' AND object_id = OBJECT_ID(N'[dbo].[NatureCheckCases]'))
    BEGIN
        CREATE INDEX [IX_NatureCheckCases_FarmId] 
            ON [dbo].[NatureCheckCases]([FarmId]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureCheckCases_ConsultantId' AND object_id = OBJECT_ID(N'[dbo].[NatureCheckCases]'))
    BEGIN
        CREATE INDEX [IX_NatureCheckCases_ConsultantId] 
            ON [dbo].[NatureCheckCases]([ConsultantId]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureCheckCases_AssignedByPersonId' AND object_id = OBJECT_ID(N'[dbo].[NatureCheckCases]'))
    BEGIN
        CREATE INDEX [IX_NatureCheckCases_AssignedByPersonId] 
            ON [dbo].[NatureCheckCases]([AssignedByPersonId]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureCheckCases_Status' AND object_id = OBJECT_ID(N'[dbo].[NatureCheckCases]'))
    BEGIN
        CREATE INDEX [IX_NatureCheckCases_Status] 
            ON [dbo].[NatureCheckCases]([Status]);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NatureCheckCases_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[NatureCheckCases]'))
    BEGIN
        CREATE INDEX [IX_NatureCheckCases_CreatedAt] 
            ON [dbo].[NatureCheckCases]([CreatedAt]);
    END
END
GO
GO

/***************************************************************************************************
    Check Constraints for NatureCheckCases
    Purpose: Ensure data integrity for Status and Priority values
***************************************************************************************************/

-- Add check constraint for Status if it doesn't exist
IF OBJECT_ID(N'[dbo].[NatureCheckCases]') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.check_constraints 
        WHERE name = N'CK_NatureCheckCases_Status'
    )
    BEGIN
        ALTER TABLE [dbo].[NatureCheckCases]
        ADD CONSTRAINT [CK_NatureCheckCases_Status]
            CHECK ([Status] IN (N'Assigned', N'InProgress', N'Completed', N'Cancelled'));
    END
END
GO

-- Add check constraint for Priority if it doesn't exist
IF OBJECT_ID(N'[dbo].[NatureCheckCases]') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.check_constraints 
        WHERE name = N'CK_NatureCheckCases_Priority'
    )
    BEGIN
        ALTER TABLE [dbo].[NatureCheckCases]
        ADD CONSTRAINT [CK_NatureCheckCases_Priority]
            CHECK ([Priority] IS NULL OR [Priority] IN (N'Low', N'Medium', N'High', N'Urgent'));
    END
END
GO

PRINT 'Database [ArlaNatureConnect_Dev] schema creation completed.';

/*
 Insert initial version info   
 */
INSERT INTO [dbo].[VersionInfo] ([Id], [Version]) VALUES (1, '1.0.0');
GO
