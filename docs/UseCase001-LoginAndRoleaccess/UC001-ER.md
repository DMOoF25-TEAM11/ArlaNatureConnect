```mermaid
erDiagram
    ROLE {
        uniqueidentifier ID PK
        nvarchar(50) Name
    }

    PERSONS {
        uniqueidentifier ID PK
        nvarchar(100) FirstName
        nvarchar(100) LastName
        nvarchar(256) Email
        uniqueidentifier RoleID FK
        uniqueidentifier AddressID FK "nullable"
        bit IsActive
    }

    LOGIN_SESSION {
        uniqueidentifier SessionID PK
        uniqueidentifier UserId FK "references PERSONS.ID, kept as UserId for backward compatibility"
        datetime2 LoginTimestamp
        datetime2 LogoutTimestamp  "nullable"
    }

    %% blank line above fixes syntax error
    ROLE ||--o{ PERSONS : "has many persons"
    PERSONS ||--o{ LOGIN_SESSION : "creates many sessions"
   ```
