# UC002B Domain Model
```mermaid
classDiagram
    class Farm {
        +Guid Id
        +string Name
        +string CVR
        +Guid PersonId
        +Guid AddressId
    }

    class Person {
        +Guid Id
        +Guid RoleId
        +Guid AddressId
        +string FirstName
        +string LastName
        +string Email
        +bool IsActive
    }

    class Role {
        +Guid Id
        +string Name
    }

    class NatureCheckCase {
        +Guid Id
        +Guid FarmId
        +Guid ConsultantId
        +Guid AssignedByPersonId
        +NatureCheckCaseStatus Status
        +string? Notes
        +string? Priority
        +DateTimeOffset CreatedAt
        +DateTimeOffset? AssignedAt
    }

    Person "1" --> "0..*" NatureCheckCase : assignedTo
    Farm "1" --> "0..*" NatureCheckCase : has
    Person "1" --> "1" Role : has
    Person "1" --> "1" Address : has
    Farm "1" --> "1" Address : has
