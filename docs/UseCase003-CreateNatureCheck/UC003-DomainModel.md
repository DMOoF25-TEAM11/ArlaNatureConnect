```mermaid
---
title: Use Case 003 - Domain Model
---
classDiagram
    class NatureCheck {
    }
    class Farm {
        farmName
        location
        cvr
    }
    class Person {
        firstName
        lastName
        email
        role
        isActive
    }
    NatureCheck -- Farm
    Farm -- Person
```
