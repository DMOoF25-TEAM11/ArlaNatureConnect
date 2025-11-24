
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
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
    
    -- Create indexes for performance
    CREATE INDEX [IX_NatureCheckCases_FarmId] 
        ON [dbo].[NatureCheckCases]([FarmId]);
    
    CREATE INDEX [IX_NatureCheckCases_ConsultantId] 
        ON [dbo].[NatureCheckCases]([ConsultantId]);
    
    CREATE INDEX [IX_NatureCheckCases_AssignedByPersonId] 
        ON [dbo].[NatureCheckCases]([AssignedByPersonId]);
    
    CREATE INDEX [IX_NatureCheckCases_Status] 
        ON [dbo].[NatureCheckCases]([Status]);
    
    CREATE INDEX [IX_NatureCheckCases_CreatedAt] 
        ON [dbo].[NatureCheckCases]([CreatedAt]);
    
    PRINT 'Table [NatureCheckCases] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [NatureCheckCases] already exists.';
END
GO

/***************************************************************************************************
    Check Constraints
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
        
        PRINT 'Check constraint [CK_NatureCheckCases_Status] added.';
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
        
        PRINT 'Check constraint [CK_NatureCheckCases_Priority] added.';
    END
END
GO

PRINT 'UC002B DDL script completed successfully.';
GO

