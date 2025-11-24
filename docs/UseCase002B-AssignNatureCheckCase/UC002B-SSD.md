# **SSD – System Sequence Diagram**


```mermaid
sequenceDiagram
    title UC02b – SSD: Assign Nature Check Case to Consultant

    actor ArlaEmployee as Arla Employee
    participant UI as UI
    participant System as System

    ArlaEmployee ->> UI: Open "Nature Check Cases"
    UI ->> System: LoadFarmsAndConsultantsAsync()
    System -->> UI: (IReadOnlyList~Farm~, IReadOnlyList~Person~)

    ArlaEmployee ->> UI: Select farm (or create via UC02)
    Note over UI,System: Farm details already loaded

    ArlaEmployee ->> UI: Click "Create Nature Check Case"
    UI ->> ArlaEmployee: Display assignment form

    ArlaEmployee ->> UI: Select consultant + enter notes
    UI ->> System: AssignCaseAsync(farmId, consultantId, assignedByPersonId, notes)
    System -->> UI: NatureCheckCase (confirmation)

    System ->> Consultant: NotifyConsultantAsync(consultantId, caseId)
