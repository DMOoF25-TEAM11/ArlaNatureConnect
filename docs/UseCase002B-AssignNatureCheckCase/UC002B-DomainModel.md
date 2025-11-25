# UC002B Domain Model
```mermaid
---
title: Use Case 002B - Domain Model
---
classDiagram
    class Farm {
        name
        cvr
    }

    class Person {
        firstName
        lastName
        email
    }

    class ArlaEmployee
    class Consultant

    Person <|-- ArlaEmployee
    Person <|-- Consultant

    class Address {
        street
        city
        postalCode
        country
    }

    class NatureCheckCase {
        status
        notes
        priority
    }

    ArlaEmployee "1" -- "0..*" NatureCheckCase : assignments
    Consultant "1" -- "0..*" NatureCheckCase : responsibilities
    Farm "1" -- "0..*" NatureCheckCase : inspections

    Person "1" -- "1" Address : residence
    Farm "1" -- "1" Address : location
