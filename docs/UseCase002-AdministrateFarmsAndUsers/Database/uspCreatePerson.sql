/*
    File: uspCreatePerson.sql
    Purpose: Stored procedure to create a new user.
    safety: This script creates a stored procedure. Review before running in production.

    Use Case: UC002 - Administrate Farms and Users
    Description: This stored procedure creates a new user with the specified username, password, and role.

    created: 2025-11-03
    change log:
        - 2025-11-03: Initial creation
*/

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

/***************************************************************************************************
 Stored Procedure: uspCreatePerson
 Purpose: Creates a new user with the specified username, password, and role.
 ***************************************************************************************************/

IF OBJECT_ID(N'[dbo].[uspCreatePerson]', N'P') IS NOT NULL
BEGIN
    PRINT 'Dropping existing stored procedure [dbo].[uspCreatePerson]...';
    DROP PROCEDURE [dbo].[uspCreatePerson];
END
GO

CREATE PROCEDURE [dbo].[uspCreatePerson]
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(256),
    @RoleId UNIQUEIDENTIFIER,
    @Street NVARCHAR(200) = NULL,
    @City NVARCHAR(100) = NULL,
    @PostalCode NVARCHAR(50) = NULL,
    @Country NVARCHAR(100) = NULL,
    @FarmName NVARCHAR(200) = NULL,
    @FarmStreet NVARCHAR(200) = NULL,
    @FarmCity NVARCHAR(100) = NULL,
    @FarmPostalCode NVARCHAR(50) = NULL,
    @FarmCountry NVARCHAR(100) = NULL,
    @FarmCVR NVARCHAR(50) = NULL,
    @ActorUserId UNIQUEIDENTIFIER = NULL,
    @CreatedUserId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    PRINT 'Creating stored procedure [dbo].[uspCreatePerson]...';
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

            -- Generate user id up front so we can set Farm.UserId when creating the farm
            SET @CreatedUserId = NEWID();

            -- Optional: insert address for user
            DECLARE @AddressId UNIQUEIDENTIFIER = NULL;
            IF @Street IS NOT NULL OR @City IS NOT NULL OR @PostalCode IS NOT NULL OR @Country IS NOT NULL
            BEGIN
            SET @AddressId = NEWID();
            INSERT INTO [dbo].[Address] ([Id], [Street], [City], [PostalCode], [Country])
            VALUES (@AddressId, @Street, @City, @PostalCode, @Country);
            END

            -- Optional: insert farm address
            DECLARE @FarmAddressId UNIQUEIDENTIFIER = NULL;
            IF @FarmStreet IS NOT NULL OR @FarmCity IS NOT NULL OR @FarmPostalCode IS NOT NULL OR @FarmCountry IS NOT NULL
            BEGIN
            SET @FarmAddressId = NEWID();
            INSERT INTO [dbo].[Address] ([Id], [Street], [City], [PostalCode], [Country])
            VALUES (@FarmAddressId, @FarmStreet, @FarmCity, @FarmPostalCode, @FarmCountry);
            END

            -- Optional: insert farm (if provided) and set its UserId to the newly created user
            DECLARE @FarmId UNIQUEIDENTIFIER = NULL;
            IF (@FarmName IS NOT NULL)
            BEGIN
            SET @FarmId = NEWID();
            INSERT INTO [dbo].[Farms] ([Id], [Name], [AddressId], [UserId], [CVR])
            VALUES (@FarmId, @FarmName, @FarmAddressId, @CreatedUserId, @FarmCVR);
            END

            -- Insert user (no FarmId column in Users table)
            INSERT INTO [dbo].[Users] ([Id], [FirstName], [LastName], [Email], [RoleId], [AddressId], [IsActive])
            VALUES (@CreatedUserId, @FirstName, @LastName, @Email, @RoleId, @AddressId,1);

            INSERT INTO [dbo].[AuditLog] ([ActorUserId], [TargetUserId], [Action], [Details])
            VALUES (@ActorUserId, @CreatedUserId, 'CreateUser', CONCAT('Created user ', @Email));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <>0
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO