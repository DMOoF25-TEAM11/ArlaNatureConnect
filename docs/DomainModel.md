# Domain Model Overview

```mermaid
---
  title: "Domain Model overview"
---
classDiagram
  direction TB

  class Role {
    name
  }

  class Person {
    firstName
    lastName
    email
    isActive
  }

  class Address {
    street
    city
    postalCode
    country
  }

  class Farm {
    name
    cvr
  }

  class NatureCheckCase {
    status
    notes
    priority
  }

  class NatureArea {
    name
    description
  }

  class NatureAreaCoordinate {
    latitude
    longitude
    orderIndex
  }

  class NatureAreaImage {
    imageUrl
  }

  %% Relationships - conceptual domain relationships
  Person "1" -- "1" Role : has
  Person "1" -- "0..1" Address : residesAt
  Person "1" -- "0..*" Farm : owns
  Person "0..*" -- "0..*" Farm : "assigned to (UserFarms)"
  Farm "1" -- "1" Address : locatedAt
  Farm "1" -- "0..*" NatureArea : has
  Farm "1" -- "0..*" NatureCheckCase : has
  Person "1" -- "0..*" NatureCheckCase : "assigned as consultant"
  Person "1" -- "0..*" NatureCheckCase : "assigned by"
  NatureArea "1" -- "0..*" NatureAreaCoordinate : has
  NatureArea "1" -- "0..*" NatureAreaImage : has
```

**Domain Concepts:**
- **Person** - A person in the system who can be a farmer, consultant, Arla employee, or administrator
- **Role** - A role that defines a person's function in the system (Farmer, Consultant, Employee, Administrator)
- **Address** - A physical address where a person resides or a farm is located
- **Farm** - A farm that can have Nature Check Cases assigned to it
- **NatureCheckCase** - An assignment of a Nature Check task to a consultant for a specific farm
- **NatureArea** - A nature area associated with a farm
- **NatureAreaCoordinate** - Geographic coordinates for a nature area
- **NatureAreaImage** - Images associated with a nature area

**Domain Relationships:**
- A Person has exactly one Role
- A Person may have one Address (where they reside)
- A Person can own zero or more Farms
- A Person can be assigned to zero or more Farms (via UserFarms)
- A Farm has exactly one Address (where it is located)
- A Farm can have zero or more Nature Areas
- A Farm can have zero or more Nature Check Cases
- A Person (consultant) can be assigned to zero or more Nature Check Cases
- A Person (Arla employee) can assign zero or more Nature Check Cases
- A Nature Area has zero or more Coordinates
- A Nature Area has zero or more Images