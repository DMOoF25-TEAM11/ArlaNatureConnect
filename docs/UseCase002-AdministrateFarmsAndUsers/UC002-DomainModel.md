```mermaid
---
title: Use Case 002 - Domain Model
---
classDiagram
    direction TB

    class person {
        firstName
        lastName
        email
        address
        aktiveStatus
    }

    class Farmer {
        farmName
        address
        cvr
    }

    class Role {
        roleName
    }

    note for Person "Represents a Person personal entity with personal details and active status."
    note for Role "Defines a role that can be assigned to Persons for access control."
    note for Farmer "Extends Person with additional attributes specific to farmers."

    Person "1" -- "*" Role : has >
    Farmer "0..1" -- "1" Person : has >
    ```