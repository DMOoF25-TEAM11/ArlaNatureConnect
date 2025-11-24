# UC002B Domain Model
```mermaid
---
title: Use Case 002B - Domain Model
---
classDiagram
    class Farm {
        name
        cvr
        personId
        addressId
    }

    class Person {
        firstName
        lastName
        email
        roleId
        addressId
        isActive
    }

    class Role {
        roleName
    }

    class Address {
        street
        city
        postalCode
        country
    }

    class NatureCheckCase {
        farmId
        consultantId
        assignedByPersonId
        status
        notes
        priority
        createdAt
        assignedAt
    }

    Person "1" --> "0..*" NatureCheckCase : assignedTo
    Farm "1" --> "0..*" NatureCheckCase : has
    Person "1" --> "1" Role : has
    Person "1" --> "1" Address : has
    Farm "1" --> "1" Address : has
