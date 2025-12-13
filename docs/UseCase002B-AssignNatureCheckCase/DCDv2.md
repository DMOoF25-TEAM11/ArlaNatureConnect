
```mermaid
classDiagram
    direction TB
    class Farm_Consultant {
        +Guid Id
        +Guid ConsultantID
        +Guid FarmId
        +DateTime AssignedDate
        +DateTime? UnassignedDate
    }

    class Farm_ArlaEmployee {
        +Guid Id
        +Guid ArlaEmployeeID
        +Guid FarmId
        +DateTime AssignedDate
        +DateTime? UnassignedDate
    }
```    