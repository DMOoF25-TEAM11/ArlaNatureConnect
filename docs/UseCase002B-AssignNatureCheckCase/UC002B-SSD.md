# **SSD – System Sequence Diagram**


```mermaid
sequenceDiagram
    title UC02b – SSD: Assign Nature Check Case to Consultant

    actor ArlaEmployee as Arla Employee
    participant System as System

    ArlaEmployee ->> System: Open "Nature Check Cases" module
    System -->> ArlaEmployee: Display list of farms

    alt Farm exists in list
        ArlaEmployee ->> System: Select existing farm
        System -->> ArlaEmployee: Display farm details
    else Farm does not exist
        ArlaEmployee ->> System: Click "Create New Farm" (UC02)
        System -->> ArlaEmployee: Display farm creation form
        ArlaEmployee ->> System: Enter farm details and save
        System -->> ArlaEmployee: Display confirmation and return to farm list
        ArlaEmployee ->> System: Select newly created farm
        System -->> ArlaEmployee: Display farm details
    end

    ArlaEmployee ->> System: Click "Create Nature Check Case"
    System -->> ArlaEmployee: Display assignment form

    ArlaEmployee ->> System: Select consultant and enter notes, then submit
    System -->> ArlaEmployee: Display confirmation message

    Note over System: System sends notification to consultant
