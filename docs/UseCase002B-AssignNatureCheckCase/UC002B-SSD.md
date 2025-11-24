
# **SSD – System Sequence Diagram**


```mermaid
sequenceDiagram
    title UC02b – SSD: Assign Nature Check Case to Consultant

    actor ArlaEmployee as Arla Employee
    participant UI as UI
    participant System as System

    ArlaEmployee ->> UI: Open "Nature Check Cases"
    UI ->> System: requestFarmList()
    System -->> UI: listOfFarms

    ArlaEmployee ->> UI: Select farm (or create via UC02)
    UI ->> System: getFarmDetails(farmId)
    System -->> UI: farmDetails

    ArlaEmployee ->> UI: Click "Create Nature Check Case"
    UI ->> ArlaEmployee: Display assignment form

    ArlaEmployee ->> UI: Select consultant + notes
    UI ->> System: createNatureCheckCase(farmId, consultantId, notes)
    System -->> UI: confirmation

    System ->> Consultant: sendNotification(caseAssigned)
