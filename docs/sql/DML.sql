


/***********************************************************************
-- Add data for UC002 - Administrate Farms and Users
***********************************************************************/

SET NOCOUNT ON;

PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO

-- Clean target tables to ensure deterministic sample data (delete in FK-safe order)
PRINT 'Cleaning target tables...';
IF OBJECT_ID(N'[dbo].[UserFarms]', N'U') IS NOT NULL
    DELETE FROM [dbo].[UserFarms];

IF OBJECT_ID(N'[dbo].[Farms]', N'U') IS NOT NULL
    DELETE FROM [dbo].[Farms];

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
    DELETE FROM [dbo].[Users];

IF OBJECT_ID(N'[dbo].[Address]', N'U') IS NOT NULL
    DELETE FROM [dbo].[Address];

-- Only remove the sample roles we will re-create to avoid removing other role records
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NOT NULL
    DELETE FROM [dbo].[Roles] WHERE [Name] IN (N'Admin', N'Farmer', N'Consultant', N'Employee');

-- Continue with script
-- Ensure roles exist and capture ids
DECLARE @Role_Admin UNIQUEIDENTIFIER = NEWID();
DECLARE @Role_Farmer UNIQUEIDENTIFIER = NEWID();
DECLARE @Role_Consultant UNIQUEIDENTIFIER = NEWID();
DECLARE @Role_Employee UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = N'Admin')
    INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (@Role_Admin, N'Admin');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = N'Farmer')
    INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (@Role_Farmer, N'Farmer');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = N'Consultant')
    INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (@Role_Consultant, N'Consultant');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = N'Employee')
    INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (@Role_Employee, N'Employee');

-- Create UserFarms mapping table if it doesn't exist (to represent many-to-many user<->farm)
IF OBJECT_ID(N'[dbo].[UserFarms]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserFarms](
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [FarmId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_UserFarms] PRIMARY KEY ([UserId],[FarmId]),
        CONSTRAINT [FK_UserFarms_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserFarms_Farms] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms]([Id]) ON DELETE CASCADE
    );
END

-- Insert farm addresses & farms (all near Varde, Jutland, Denmark)
DECLARE
    @F1 UNIQUEIDENTIFIER = NEWID(),  @FA1 UNIQUEIDENTIFIER = NEWID(),
    @F2 UNIQUEIDENTIFIER = NEWID(),  @FA2 UNIQUEIDENTIFIER = NEWID(),
    @F3 UNIQUEIDENTIFIER = NEWID(),  @FA3 UNIQUEIDENTIFIER = NEWID(),
    @F4 UNIQUEIDENTIFIER = NEWID(),  @FA4 UNIQUEIDENTIFIER = NEWID(),
    @F5 UNIQUEIDENTIFIER = NEWID(),  @FA5 UNIQUEIDENTIFIER = NEWID(),
    @F6 UNIQUEIDENTIFIER = NEWID(),  @FA6 UNIQUEIDENTIFIER = NEWID(),
    @F7 UNIQUEIDENTIFIER = NEWID(),  @FA7 UNIQUEIDENTIFIER = NEWID(),
    @F8 UNIQUEIDENTIFIER = NEWID(),  @FA8 UNIQUEIDENTIFIER = NEWID(),
    @F9 UNIQUEIDENTIFIER = NEWID(),  @FA9 UNIQUEIDENTIFIER = NEWID(),
    @F10 UNIQUEIDENTIFIER = NEWID(), @FA10 UNIQUEIDENTIFIER = NEWID(),
    @F11 UNIQUEIDENTIFIER = NEWID(), @FA11 UNIQUEIDENTIFIER = NEWID(),
    @F12 UNIQUEIDENTIFIER = NEWID(), @FA12 UNIQUEIDENTIFIER = NEWID(),
    @F13 UNIQUEIDENTIFIER = NEWID(), @FA13 UNIQUEIDENTIFIER = NEWID(),
    @F14 UNIQUEIDENTIFIER = NEWID(), @FA14 UNIQUEIDENTIFIER = NEWID(),
    @F15 UNIQUEIDENTIFIER = NEWID(), @FA15 UNIQUEIDENTIFIER = NEWID(),
    @F16 UNIQUEIDENTIFIER = NEWID(), @FA16 UNIQUEIDENTIFIER = NEWID(),
    @F17 UNIQUEIDENTIFIER = NEWID(), @FA17 UNIQUEIDENTIFIER = NEWID(),
    @F18 UNIQUEIDENTIFIER = NEWID(), @FA18 UNIQUEIDENTIFIER = NEWID();

-- Addresses for farms (near Varde)
INSERT INTO [dbo].[Address] ([Id],[Street],[City],[PostalCode],[Country])
VALUES
(@FA1, N'Stuevej 1',      N'Varde',        N'6800', N'Denmark'),
(@FA2, N'Agerskovvej 12',  N'Ølgod',        N'6870', N'Denmark'),
(@FA3, N'Kærgårdsvej 5',   N'Nørre Nebel',  N'6830', N'Denmark'),
(@FA4, N'Rødhusvej 3',     N'Henne',        N'6854', N'Denmark'),
(@FA5, N'Kystvej 10',      N'Vejers Strand',N'6853', N'Denmark'),
(@FA6, N'Bjergvej 8',      N'Varde',        N'6800', N'Denmark'),
(@FA7, N'Bækvej 2',        N'Varde',        N'6800', N'Denmark'),
(@FA8, N'Lyngvej 14',      N'Blåvand',      N'6857', N'Denmark'),
(@FA9, N'Hulsigvej 6',     N'Esbjerg',      N'6700', N'Denmark'),
(@FA10, N'Skovvej 7',      N'Tistrup',      N'6840', N'Denmark'),
(@FA11, N'Engvej 11',      N'Varde',        N'6800', N'Denmark'),
(@FA12, N'Åkandevej 9',    N'Varde',        N'6800', N'Denmark'),
(@FA13, N'Holmskov 4',     N'Varde',        N'6800', N'Denmark'),
(@FA14, N'Gammeltoft 16',  N'Varde',        N'6800', N'Denmark'),
(@FA15, N'Rugvej 18',      N'Varde',        N'6800', N'Denmark'),
(@FA16, N'Kærvej 20',      N'Oksbøl',       N'6840', N'Denmark'),
(@FA17, N'Vindvej 22',     N'Nørre Nebel',  N'6830', N'Denmark'),
(@FA18, N'Markvej 24',     N'Varde',        N'6800', N'Denmark');

-- Farms
INSERT INTO [dbo].[Farms] ([Id],[Name],[AddressId],[CVR])
VALUES
(@F1,  N'Søndergaard Farm', @FA1, N'12345678'),
(@F2,  N'Bakken Farm', @FA2, N'23456789'),
(@F3,  N'Engholm Farm', @FA3, N'34567890'),
(@F4,  N'Lynggård Farm', @FA4, N'45678901'),
(@F5,  N'Vester Farm', @FA5, N'56789012'),
(@F6,  N'Skovgård Farm', @FA6, N'67890123'),
(@F7,  N'Bølling Farm', @FA7, N'78901234'),
(@F8,  N'Havbakken Farm', @FA8, N'89012345'),
(@F9,  N'Strande Farm', @FA9, N'90123456'),
(@F10, N'Tistrup Farm', @FA10, N'01234567'),
(@F11, N'Engvang Farm', @FA11, N'11223344'),
(@F12, N'Ågade Farm', @FA12, N'22334455'),
(@F13, N'Holmgaard Farm', @FA13, N'33445566'),
(@F14, N'Gammelgård Farm', @FA14, N'44556677'),
(@F15, N'Rugmark Farm', @FA15, N'55667788'),
(@F16, N'Mosegård Farm', @FA16, N'66778899'),
(@F17, N'Vindmark Farm', @FA17, N'77889900'),
(@F18, N'Markholm Farm', @FA18, N'88990011');

-- Insert user addresses (Danish addresses)
DECLARE
    @A1 UNIQUEIDENTIFIER = NEWID(),  @A2 UNIQUEIDENTIFIER = NEWID(),
    @A3 UNIQUEIDENTIFIER = NEWID(),  @A4 UNIQUEIDENTIFIER = NEWID(),
    @A5 UNIQUEIDENTIFIER = NEWID(),  @A6 UNIQUEIDENTIFIER = NEWID(),
    @A7 UNIQUEIDENTIFIER = NEWID(),  @A8 UNIQUEIDENTIFIER = NEWID(),
    @A9 UNIQUEIDENTIFIER = NEWID(),  @A10 UNIQUEIDENTIFIER = NEWID(),
    @A11 UNIQUEIDENTIFIER = NEWID(), @A12 UNIQUEIDENTIFIER = NEWID(),
    @A13 UNIQUEIDENTIFIER = NEWID(), @A14 UNIQUEIDENTIFIER = NEWID(),
    @A15 UNIQUEIDENTIFIER = NEWID(), @A16 UNIQUEIDENTIFIER = NEWID(),
    @A17 UNIQUEIDENTIFIER = NEWID(), @A18 UNIQUEIDENTIFIER = NEWID(),
    @A19 UNIQUEIDENTIFIER = NEWID(), @A20 UNIQUEIDENTIFIER = NEWID(),
    @A21 UNIQUEIDENTIFIER = NEWID(), @A22 UNIQUEIDENTIFIER = NEWID(),
    @A23 UNIQUEIDENTIFIER = NEWID(), @A24 UNIQUEIDENTIFIER = NEWID(),
    @A25 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Address] ([Id],[Street],[City],[PostalCode],[Country])
VALUES
(@A1,  N'Rosenvej 3',       N'København',   N'2300', N'Denmark'),
(@A2,  N'Borggade 12',      N'Aarhus',      N'8000', N'Denmark'),
(@A3,  N'Slotgade 5',       N'Odense',      N'5000', N'Denmark'),
(@A4,  N'Prinsensgade 8',   N'Aalborg',     N'9000', N'Denmark'),
(@A5,  N'Strandvej 2',      N'Esbjerg',     N'6700', N'Denmark'),
(@A6,  N'Vestre Bygade 10', N'Varde',       N'6800', N'Denmark'),
(@A7,  N'Skovvej 1',        N'Horsens',     N'8700', N'Denmark'),
(@A8,  N'Parkalle 4',       N'Kolding',     N'6000', N'Denmark'),
(@A9,  N'Engvej 6',         N'Herning',     N'7400', N'Denmark'),
(@A10, N'Elmevej 7',        N'Silkeborg',   N'8600', N'Denmark'),
(@A11, N'Bredgade 9',       N'Randers',     N'8900', N'Denmark'),
(@A12, N'Kystvej 1',        N'Fredericia',  N'7000', N'Denmark'),
(@A13, N'Vestergade 22',    N'Hillerød',    N'3400', N'Denmark'),
(@A14, N'Liljevej 18',      N'Slagelse',    N'4200', N'Denmark'),
(@A15, N'Enggade 3',        N'Ribe',        N'6760', N'Denmark'),
(@A16, N'Stationsvej 2',    N'Rebild',      N'9530', N'Denmark'),
(@A17, N'Markedsgade 5',    N'Aalborg',     N'9000', N'Denmark'),
(@A18, N'Kirkegade 11',     N'Vejle',       N'7100', N'Denmark'),
(@A19, N'Langgade 14',      N'Verb',        N'0000', N'Denmark'), -- placeholder
(@A20, N'Sønderskov 12',    N'Verb2',       N'0000', N'Denmark'), -- placeholder
(@A21, N'Bøgade 2',         N'Varde',       N'6800', N'Denmark'),
(@A22, N'Hovedgade 6',      N'Ølgod',       N'6870', N'Denmark'),
(@A23, N'Kirkeby 3',        N'Nørre Nebel', N'6830', N'Denmark'),
(@A24, N'Bøstrup 7',        N'Henne',       N'6854', N'Denmark'),
(@A25, N'Strandvej 9',      N'Blåvand',     N'6857', N'Denmark');

-- Insert 15 farmers (15 users with farms)
DECLARE
    @U1 UNIQUEIDENTIFIER = NEWID(),  @U2 UNIQUEIDENTIFIER = NEWID(),
    @U3 UNIQUEIDENTIFIER = NEWID(),  @U4 UNIQUEIDENTIFIER = NEWID(),
    @U5 UNIQUEIDENTIFIER = NEWID(),  @U6 UNIQUEIDENTIFIER = NEWID(),
    @U7 UNIQUEIDENTIFIER = NEWID(),  @U8 UNIQUEIDENTIFIER = NEWID(),
    @U9 UNIQUEIDENTIFIER = NEWID(),  @U10 UNIQUEIDENTIFIER = NEWID(),
    @U11 UNIQUEIDENTIFIER = NEWID(), @U12 UNIQUEIDENTIFIER = NEWID(),
    @U13 UNIQUEIDENTIFIER = NEWID(), @U14 UNIQUEIDENTIFIER = NEWID(),
    @U15 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Users] ([Id],[FirstName],[LastName],[Email],[RoleId],[AddressId],[IsActive])
VALUES
(@U1, N'Anders', N'Hansen', N'anders.hansen@example.dk', @Role_Farmer, @A6,1),
(@U2, N'Jens', N'Jensen', N'jens.jensen@example.dk', @Role_Farmer, @A21,1),
(@U3, N'Peter', N'Nielsen', N'peter.nielsen@example.dk', @Role_Farmer, @A22,1),
(@U4, N'Kasper', N'Olsen', N'kasper.olsen@example.dk', @Role_Farmer, @A23,1),
(@U5, N'Morten', N'Larsen', N'morten.larsen@example.dk', @Role_Farmer, @A24,1),
(@U6, N'Søren', N'Sorensen', N'soeren.sorensen@example.dk', @Role_Farmer, @A25,1),
(@U7, N'Torben', N'Pedersen', N'torben.pedersen@example.dk', @Role_Farmer, @A7,1),
(@U8, N'Henrik', N'Kristensen',N'henrik.kristensen@example.dk',@Role_Farmer,@A8,1),
(@U9, N'Steffen', N'Andersen', N'steffen.andersen@example.dk', @Role_Farmer, @A9,1),
(@U10, N'Bjorn', N'Madsen', N'bjorn.madsen@example.dk', @Role_Farmer, @A10,1),
(@U11, N'Svend', N'Christensen',N'svend.christensen@example.dk',@Role_Farmer,@A11,1),
(@U12, N'Ulrik', N'Lykke', N'ulrik.lykke@example.dk', @Role_Farmer, @A12,1),
(@U13, N'Bo', N'Karlsen', N'bo.karlsen@example.dk', @Role_Farmer, @A13,1),
(@U14, N'Niels', N'Eriksen', N'niels.eriksen@example.dk', @Role_Farmer, @A14,1),
(@U15, N'Rasmus', N'Poulsen', N'rasmus.poulsen@example.dk', @Role_Farmer, @A15,1);

-- Assign extra farms so3 farmers have2 farms each.
-- We'll give U1 -> F16, U2 -> F17, U3 -> F18 (in addition to their primary farm mapping below)
INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U1,@F1);
INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U1,@F16);

INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U2,@F2);
INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U2,@F17);

INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U3,@F3);
INSERT INTO [dbo].[UserFarms] ([UserId],[FarmId]) VALUES (@U3,@F18);

-- Insert5 consultants (no primary farm)
DECLARE
    @C1 UNIQUEIDENTIFIER = NEWID(), @C2 UNIQUEIDENTIFIER = NEWID(),
    @C3 UNIQUEIDENTIFIER = NEWID(), @C4 UNIQUEIDENTIFIER = NEWID(),
    @C5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Users] ([Id],[FirstName],[LastName],[Email],[RoleId],[AddressId],[IsActive])
VALUES
(@C1, N'Camilla', N'Jepsen',    N'camilla.jepsen@example.dk', @Role_Consultant, @A1, 1),
(@C2, N'Maria',   N'Andersen',   N'maria.andersen@example.dk', @Role_Consultant, @A2, 1),
(@C3, N'Louise',  N'Mortensen',  N'louise.mortensen@example.dk',@Role_Consultant, @A3, 1),
(@C4, N'Ida',     N'Holst',      N'ida.holst@example.dk',      @Role_Consultant, @A4, 1),
(@C5, N'Anna',    N'Bonde',      N'anna.bonde@example.dk',     @Role_Consultant, @A5, 1);

-- Insert 5 employees (no farm)
DECLARE
    @E1 UNIQUEIDENTIFIER = NEWID(), @E2 UNIQUEIDENTIFIER = NEWID(),
    @E3 UNIQUEIDENTIFIER = NEWID(), @E4 UNIQUEIDENTIFIER = NEWID(),
    @E5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Users] ([Id],[FirstName],[LastName],[Email],[RoleId],[AddressId],[IsActive])
VALUES
(@E1, N'Christian', N'Holm', N'christian.holm@example.dk', @Role_Employee, @A16,1),
(@E2, N'Sofie', N'Kjær', N'sofie.kjaer@example.dk', @Role_Employee, @A17,1),
(@E3, N'Emil', N'Bennett', N'emil.bennett@example.dk', @Role_Employee, @A18,1),
(@E4, N'Jonas', N'Hansen', N'jonas.hansen@example.dk', @Role_Employee, @A19,1),
(@E5, N'Mathilde', N'Christoff',N'mathilde.christoff@example.dk', @Role_Employee, @A20,1);

-- After users inserted: set primary owner on farms (F1..F15 -> U1..U15)
UPDATE [dbo].[Farms] SET [UserId] = @U1 WHERE [Id] = @F1;
UPDATE [dbo].[Farms] SET [UserId] = @U2 WHERE [Id] = @F2;
UPDATE [dbo].[Farms] SET [UserId] = @U3 WHERE [Id] = @F3;
UPDATE [dbo].[Farms] SET [UserId] = @U4 WHERE [Id] = @F4;
UPDATE [dbo].[Farms] SET [UserId] = @U5 WHERE [Id] = @F5;
UPDATE [dbo].[Farms] SET [UserId] = @U6 WHERE [Id] = @F6;
UPDATE [dbo].[Farms] SET [UserId] = @U7 WHERE [Id] = @F7;
UPDATE [dbo].[Farms] SET [UserId] = @U8 WHERE [Id] = @F8;
UPDATE [dbo].[Farms] SET [UserId] = @U9 WHERE [Id] = @F9;
UPDATE [dbo].[Farms] SET [UserId] = @U10 WHERE [Id] = @F10;
UPDATE [dbo].[Farms] SET [UserId] = @U11 WHERE [Id] = @F11;
UPDATE [dbo].[Farms] SET [UserId] = @U12 WHERE [Id] = @F12;
UPDATE [dbo].[Farms] SET [UserId] = @U13 WHERE [Id] = @F13;
UPDATE [dbo].[Farms] SET [UserId] = @U14 WHERE [Id] = @F14;
UPDATE [dbo].[Farms] SET [UserId] = @U15 WHERE [Id] = @F15;

-- Also set owners for extra farms so three users own two farms each
UPDATE [dbo].[Farms] SET [UserId] = @U1 WHERE [Id] = @F16;
UPDATE [dbo].[Farms] SET [UserId] = @U2 WHERE [Id] = @F17;
UPDATE [dbo].[Farms] SET [UserId] = @U3 WHERE [Id] = @F18;

-- Simple verification queries (uncomment to run)
PRINT 'Using database [ArlaNatureConnect_Dev]...';
USE [ArlaNatureConnect_Dev];
GO
SELECT TOP 20 * FROM [dbo].[Users];
SELECT TOP 20 * FROM [dbo].[Farms];
SELECT TOP 50 * FROM [dbo].[UserFarms] JOIN [dbo].[Users] u ON UserFarms.UserId = u.Id JOIN [dbo].[Farms] f ON UserFarms.FarmId = f.Id;


PRINT 'Sample data insertion completed.';

