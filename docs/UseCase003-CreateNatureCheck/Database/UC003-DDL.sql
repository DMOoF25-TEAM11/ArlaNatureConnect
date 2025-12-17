SET NOCOUNT ON;

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

/***************************************************************************************************
    Table: NatureCheck
    Purpose: Stores Nature Check information.
***************************************************************************************************/
IF OBJECT_ID(N'[dbo].[NatureCheck]') IS NULL
BEGIN
    CREATE TABLE [dbo].[NatureCheck] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Date] DATETIME NOT NULL,
        [FarmId] UNIQUEIDENTIFIER NOT NULL,
        [PersonId] UNIQUEIDENTIFIER NOT NULL,

        CONSTRAINT [FK_NatureCheck_Farm] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms]([Id]),
        CONSTRAINT [FK_NatureCheck_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons]([Id])
    );

    CREATE INDEX [IX_NatureCheck_FarmId] ON [dbo].[NatureCheck]([FarmId]);
    CREATE INDEX [IX_NatureCheck_PersonId] ON [dbo].[NatureCheck]([PersonId]);
END;