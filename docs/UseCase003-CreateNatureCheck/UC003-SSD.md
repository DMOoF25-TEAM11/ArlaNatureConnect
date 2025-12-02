```mermaid

sequenceDiagram
    title: UC003 SSD - Create Nature Check

    actor User
    participant System

    User ->> System: Select role (Consultant / Arla Employee)
    System -->> User: Display role-specific dashboard
    User ->> System: Choose specific person (if applicable)
    System -->> User: Display person-specific data

    Note over User,System: Person selection only applies for Farmer and Consultant roles.

    User ->> System: Navigate to "Create Nature Check" section
    System -->> User: Display nature check creation form
    User ->> System: Submit nature check suggestion from form
    System -->> User: Create nature check and notify farmer
```