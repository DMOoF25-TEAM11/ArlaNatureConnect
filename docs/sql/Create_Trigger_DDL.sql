/*
    File: Create_Trigger_DDL.sql
    Description:
        Generate CREATE TRIGGER DDL statements for auditing changes to all user tables with primary keys.
        The generated triggers log INSERT, UPDATE, and DELETE operations to a central AuditLog table.
        Each trigger captures the key values and before/after data in JSON format.

    Usage:
        1. Run this script to generate the CREATE TRIGGER statements.
        2. Copy each generated DDL statement and execute it in its own batch to create the triggers.

    Note:
        - Ensure that the AuditLog table exists before creating the triggers.
        - Modify the AuditLog table schema as needed to fit your auditing requirements.

    Script created by ChatGPT (GPT-4) based on user requirements.

    Version: 1.0.0
    Changes:
        - 2025-12-19: 1.0.0 - Initial script content
*/

SET NOCOUNT ON;

/***********************************************************************
    Create AuditLog if missing (use NVARCHAR(MAX) for JSON payloads)
***********************************************************************/
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY DEFAULT (NEWID()),
        EventTime   DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_AuditLog_EventTime DEFAULT (SYSUTCDATETIME()),
        UserName    NVARCHAR(128)     NOT NULL CONSTRAINT DF_AuditLog_UserName DEFAULT (SUSER_SNAME()),
        TableSchema SYSNAME           NOT NULL,
        TableName   SYSNAME           NOT NULL,
        Operation   NVARCHAR(10)      NOT NULL,
        KeyValues   NVARCHAR(MAX) NULL,
        BeforeData  NVARCHAR(MAX) NULL,
        AfterData   NVARCHAR(MAX) NULL,
        ContextInfo NVARCHAR(4000) NULL
    );
END;

-- Generate CREATE TRIGGER DDL for every user table that has a primary key (exclude AuditLog)
-- Result set: TableSchema, TableName, TriggerName, DDL (copy and run each DDL in its own batch)
SELECT
    s.name AS TableSchema,
    t.name AS TableName,
    'trg_au_' + s.name + '_' + t.name + '_Audit' AS TriggerName,
    CAST(
        'IF OBJECT_ID(''' + REPLACE(QUOTENAME(s.name) + '.' + QUOTENAME('trg_au_' + s.name + '_' + t.name + '_Audit'), '''', '''''') + ''', ''TR'') IS NOT NULL' + CHAR(13) + CHAR(10)
      + '    DROP TRIGGER ' + QUOTENAME(s.name) + '.' + QUOTENAME('trg_au_' + s.name + '_' + t.name + '_Audit') + ';' + CHAR(13) + CHAR(10)
      + 'CREATE TRIGGER ' + QUOTENAME(s.name) + '.' + QUOTENAME('trg_au_' + s.name + '_' + t.name + '_Audit') + CHAR(13) + CHAR(10)
      + 'ON ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + CHAR(13) + CHAR(10)
      + 'AFTER INSERT, UPDATE, DELETE' + CHAR(13) + CHAR(10)
      + 'AS' + CHAR(13) + CHAR(10)
      + 'BEGIN' + CHAR(13) + CHAR(10)
      + '    SET NOCOUNT ON;' + CHAR(13) + CHAR(10)
      + '    -- DELETE (in deleted, not in inserted)' + CHAR(13) + CHAR(10)
      + '    INSERT INTO dbo.AuditLog (TableSchema, TableName, Operation, KeyValues, BeforeData, AfterData)' + CHAR(13) + CHAR(10)
      + '    SELECT ' + '''' + REPLACE(s.name, '''', '''''') + ''', ' + '''' + REPLACE(t.name, '''', '''''') + ''', ''DELETE'',' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.pk_d_cols + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.col_list_deleted + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),' + CHAR(13) + CHAR(10)
      + '           NULL' + CHAR(13) + CHAR(10)
      + '    FROM deleted d' + CHAR(13) + CHAR(10)
      + '    LEFT JOIN inserted i ON ' + frag.join_condition + CHAR(13) + CHAR(10)
      + '    WHERE i.' + QUOTENAME(frag.first_pk_col) + ' IS NULL;' + CHAR(13) + CHAR(10)
      + '    -- INSERT (in inserted, not in deleted)' + CHAR(13) + CHAR(10)
      + '    INSERT INTO dbo.AuditLog (TableSchema, TableName, Operation, KeyValues, BeforeData, AfterData)' + CHAR(13) + CHAR(10)
      + '    SELECT ' + '''' + REPLACE(s.name, '''', '''''') + ''', ' + '''' + REPLACE(t.name, '''', '''''') + ''', ''INSERT'',' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.pk_i_cols + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),' + CHAR(13) + CHAR(10)
      + '           NULL,' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.col_list_insert + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)' + CHAR(13) + CHAR(10)
      + '    FROM inserted i' + CHAR(13) + CHAR(10)
      + '    LEFT JOIN deleted d ON ' + frag.join_condition + CHAR(13) + CHAR(10)
      + '    WHERE d.' + QUOTENAME(frag.first_pk_col) + ' IS NULL;' + CHAR(13) + CHAR(10)
      + '    -- UPDATE (present in both inserted and deleted)' + CHAR(13) + CHAR(10)
      + '    INSERT INTO dbo.AuditLog (TableSchema, TableName, Operation, KeyValues, BeforeData, AfterData)' + CHAR(13) + CHAR(10)
      + '    SELECT ' + '''' + REPLACE(s.name, '''', '''''') + ''', ' + '''' + REPLACE(t.name, '''', '''''') + ''', ''UPDATE'',' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.pk_i_cols + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.col_list_deleted + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),' + CHAR(13) + CHAR(10)
      + '           (SELECT ' + frag.col_list_insert + ' FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)' + CHAR(13) + CHAR(10)
      + '    FROM inserted i' + CHAR(13) + CHAR(10)
      + '    JOIN deleted d ON ' + frag.join_condition + ';' + CHAR(13) + CHAR(10)
      + 'END;'
    AS NVARCHAR(MAX)
    ) AS DDL
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
CROSS APPLY (
    SELECT
        pk_d_cols = STUFF((
            SELECT ', d.' + QUOTENAME(c.name) + ' AS ' + QUOTENAME(c.name)
            FROM sys.indexes ix
            JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            WHERE ix.object_id = t.object_id AND ix.is_primary_key = 1
            ORDER BY ic.key_ordinal
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''),

        pk_i_cols = STUFF((
            SELECT ', i.' + QUOTENAME(c.name) + ' AS ' + QUOTENAME(c.name)
            FROM sys.indexes ix
            JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            WHERE ix.object_id = t.object_id AND ix.is_primary_key = 1
            ORDER BY ic.key_ordinal
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''),

        col_list_insert = STUFF((
            SELECT ', i.' + QUOTENAME(name) + ' AS ' + QUOTENAME(name)
            FROM sys.columns
            WHERE object_id = t.object_id
            ORDER BY column_id
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''),

        col_list_deleted = STUFF((
            SELECT ', d.' + QUOTENAME(name) + ' AS ' + QUOTENAME(name)
            FROM sys.columns
            WHERE object_id = t.object_id
            ORDER BY column_id
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''),

        join_condition = STUFF((
            SELECT ' AND i.' + QUOTENAME(c.name) + ' = d.' + QUOTENAME(c.name)
            FROM sys.indexes ix
            JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            WHERE ix.object_id = t.object_id AND ix.is_primary_key = 1
            ORDER BY ic.key_ordinal
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 5, ''),

        first_pk_col = (
            SELECT TOP (1) c.name
            FROM sys.indexes ix
            JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            WHERE ix.object_id = t.object_id AND ix.is_primary_key = 1
            ORDER BY ic.key_ordinal
        )
) AS frag
WHERE t.is_ms_shipped = 0
  AND NOT (s.name = N'dbo' AND t.name = N'AuditLog')
  AND frag.pk_i_cols IS NOT NULL
ORDER BY s.name, t.name;