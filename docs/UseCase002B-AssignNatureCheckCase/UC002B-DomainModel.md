# UC002B Domain Model
```mermaid
classDiagram
    class Farm {
        +farmId : int
        +name : string
        +cvr : string
        +address : string
        +farmerId : int
    }

    class User {
        +userId : int
        +name : string
        +email : string
        +role : string
    }

    class NatureCheckCase {
        +caseId : int
        +farmId : int
        +consultantId : int
        +status : string
        +notes : string
        +createdDate : datetime
    }

    User "1" --> "0..*" NatureCheckCase : assignedTo
    Farm "1" --> "0..*" NatureCheckCase : has
