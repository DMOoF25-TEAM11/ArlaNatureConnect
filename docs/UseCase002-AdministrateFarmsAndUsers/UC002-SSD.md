# UC002-SSD – Administrate Farms and Users (Original)

This is the original System Sequence Diagram before updating to match Larmann's UML conventions.

```mermaid
---
title: UC002 - System Sequence Diagram
---
sequenceDiagram
    participant Admin as Administrator
    participant System as System

    Admin->>+System: Open Users management view
    System-->>-Admin: Display list of users

    alt Create user
        Admin->>+System: AddUser(User)
        System-->>Admin: Validation result / Confirmation
        System-->>-Admin: Refresh list
    end

    alt Read user details
        Admin->>System: GetUserDetails(userId)
        System-->>Admin: User details
    end

    alt Update user
        Admin->>System: UpdateUser(User)
        System-->>System: Validation(User)
        System-->>Admin: New list of user
    end

    alt Delete user
        Admin->>System: DeleteUser(userId)
        System-->>Admin: Confirmation / Refresh list
    end
```

**Note:** This is the original diagram. See UC002-SSD-v2.md for the updated version that follows Larmann's UML conventions.
