# **UC002B-SSD – Assign Nature Check Case to Consultant**

System Sequence Diagram showing the interaction between the Arla Employee actor and the System, following Larmann's UML conventions.

```mermaid
sequenceDiagram
    title UC002B – SSD: Assign Nature Check Case to Consultant

    actor ArlaEmployee as Arla Employee
    participant System as System

    ArlaEmployee ->> System: loadAssignmentContext()
    System -->> ArlaEmployee: assignmentContext(farms, consultants)

    alt Farm exists
        ArlaEmployee ->> System: selectFarm(farmId)
        alt Farm has active case
            System -->> ArlaEmployee: farmDetails(farmId, activeCase)
        else Farm has no active case
            System -->> ArlaEmployee: farmDetails(farmId)
        end
    else Farm does not exist
        ArlaEmployee ->> System: createFarm(farmData)
        alt Success
            System -->> ArlaEmployee: farmCreated(farmId)
        else Error
            System -->> ArlaEmployee: error
        end
    end

    ArlaEmployee ->> System: assignCase(farmId, consultantId, priority, notes)
    alt Success
        System -->> ArlaEmployee: caseAssigned(caseId)
    else Error
        System -->> ArlaEmployee: error
    end
```

**Notes:**
- This SSD shows the high-level interaction between the actor and the system at use case level.
- All internal operations (validation, database access, etc.) are hidden within the system boundary.
- Error messages are generic - specific validation details are not exposed at this level.
