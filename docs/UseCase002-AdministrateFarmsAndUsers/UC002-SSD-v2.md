# UC002-SSD – Administrate Farms and Users (v2)

System Sequence Diagram showing the interaction between the Administrator actor and the System, following Larmann's UML conventions.

```mermaid
sequenceDiagram
    title UC002 – SSD: Administrate Farms and Users

    actor Admin as Administrator
    participant System as System

    Admin ->> System: openUsersManagementView()
    System -->> Admin: usersList

    alt Create person
        Admin ->> System: addPerson(firstName, lastName, email, roleId, address, isActive)
        alt Success
            System -->> Admin: personCreated(personId)
        else Error
            System -->> Admin: error
        end
    end

    alt Read person details
        Admin ->> System: getPersonDetails(personId)
        System -->> Admin: personDetails
    end

    alt Update person
        Admin ->> System: updatePerson(personId, firstName, lastName, email, roleId, address, isActive)
        alt Success
            System -->> Admin: personUpdated(personId)
        else Error
            System -->> Admin: error
        end
    end

    alt Delete person
        Admin ->> System: deletePerson(personId)
        alt Success
            System -->> Admin: personDeleted(personId)
        else Error
            System -->> Admin: error
        end
    end
```

**Notes:**
- This SSD shows the high-level interaction for managing persons (users).
- All internal operations (validation, repository calls, etc.) are hidden within the system boundary.
- Method names use camelCase following Larmann's conventions.
- Parameters are shown in parentheses.
- Return values are generic (e.g., "error", "personCreated").
