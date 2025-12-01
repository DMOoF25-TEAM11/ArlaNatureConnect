# UC002B Domain Model


```mermaid
---
title: Use Case 002B - Domain Model
---
classDiagram
    direction TB

    class Farm {
        Name
        CVR
    }

    class Person {
        FirstName
        LastName
        Email
        IsActive
    }

    class Address {
        Street
        City
        PostalCode
        Country
    }

    class Role {
        Name
    }

    class NatureCheckCase {
        Status
        Notes
        Priority
    }

    %% Relationships - conceptual domain relationships
    Farm "1" -- "0..*" NatureCheckCase : has
    Person "1" -- "0..*" NatureCheckCase : assignedTo
    Person "1" -- "0..*" NatureCheckCase : assignedBy
    Person "1" -- "1" Role : has
    Person "1" -- "0..1" Address : residesAt
    Farm "1" -- "1" Address : locatedAt
    Farm "1" -- "1" Person : ownedBy
```

**Domain Concepts:**
- **Farm** - A farm that can have Nature Check Cases assigned to it
- **Person** - A person in the system who can be a consultant, Arla employee, or farm owner
- **Address** - A physical address where a person resides or a farm is located
- **Role** - A role that defines a person's function in the system (Consultant, Employee, Farmer)
- **NatureCheckCase** - An assignment of a Nature Check task to a consultant for a specific farm

**Domain Relationships:**
- A Farm can have zero or more Nature Check Cases
- A Person (consultant) can be assigned to zero or more Nature Check Cases
- A Person (Arla employee) can assign zero or more Nature Check Cases
- A Person has exactly one Role
- A Person may have one Address (where they reside)
- A Farm has exactly one Address (where it is located)
- A Farm has exactly one Person as owner

**Business Rules:**
- A Nature Check Case is always linked to one Farm
- A Nature Check Case is always assigned to one Person (consultant)
- A Nature Check Case is always assigned by one Person (Arla employee)
- A Farm must have an owner (Person)
- A Farm must have an address (Address)
- Priority is an optional attribute that indicates the urgency of a Nature Check Case
- Status indicates the current state of a Nature Check Case (Assigned, InProgress, Completed, etc.)
- Notes provide additional information about a Nature Check Case
