/*
    File: uspCreateNatureCheck.sql
    Purpose: Stored procedure to create a nature check.
    safety: This script creates a stored procedure. Review before running in production.

    Use Case: UC003 - Create Nature Check
    Description: This script holds all the columns needed to create a nature check.

    created: 2025-11-27
*/

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

/***************************************************************************************************
 Stored Procedure: uspCreateNatureCheck
 Purpose: Creates a new nature check entry.
 ***************************************************************************************************/

 /* 1) Ensure the NatureCheck table has the extra columns */



IF OBJECT_ID(N'[dbo].[uspCreateNatureCheck]', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE [dbo].[uspCreateNatureCheck];
END
GO

CREATE PROCEDURE [dbo].[uspCreateNatureCheck]
    @NatureCheckId        UNIQUEIDENTIFIER OUTPUT,
    @FarmId               UNIQUEIDENTIFIER,
    @PersonId             UNIQUEIDENTIFIER,
    @FarmName             NVARCHAR(200),
    @FarmCVR              NVARCHAR(50),
    @FarmAddress          UNIQUEIDENTIFIER,
    @ConsultantFirstName  NVARCHAR(100),
    @ConsultantLastName   NVARCHAR(100),
    @DateTime             DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @NatureCheckId IS NULL
            SET @NatureCheckId = NEWID();

        IF NOT EXISTS (SELECT 1 FROM dbo.Farms WHERE Id = @FarmId)
        BEGIN
            RAISERROR('Farm not found for given FarmId.', 16, 1);
            RETURN;
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Persons WHERE Id = @PersonId)
        BEGIN
            RAISERROR('Person not found for given PersonId.', 16, 1);
            RETURN;
        END;

        INSERT INTO dbo.NatureCheck (
            Id,
            [Date],
            FarmId,
            PersonId,
            FarmName,
            FarmCVR,
            FarmAddress,
            ConsultantFirstName,
            ConsultantLastName
        )
        VALUES (
            @NatureCheckId,
            @DateTime,
            @FarmId,
            @PersonId,
            @FarmName,
            @FarmCVR,
            @FarmAddress,
            @ConsultantFirstName,
            @ConsultantLastName
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        DECLARE 
            @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(),
            @ErrorSeverity INT = ERROR_SEVERITY(),
            @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO