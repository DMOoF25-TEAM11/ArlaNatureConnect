```mermaid
sequenceDiagram
    title UC02b – Sequence Diagram: Assign Nature Check Case

    actor ArlaEmployee
    participant UI
    participant FarmRepo as FarmRepository
    participant UserRepo as UserRepository
    participant CaseRepo as CaseRepository
    participant Notif as NotificationService

    ArlaEmployee ->> UI: Open case module
    UI ->> FarmRepo: getAllFarms()
    FarmRepo -->> UI: farmList

    ArlaEmployee ->> UI: Select farm
    UI ->> FarmRepo: getFarm(farmId)
    FarmRepo -->> UI: farmDetails

    ArlaEmployee ->> UI: Create Nature Check Case
    UI ->> UserRepo: getConsultants()
    UserRepo -->> UI: consultantList

    ArlaEmployee ->> UI: Submit assignment form
    UI ->> CaseRepo: createCase(farmId, consultantId, notes)
    CaseRepo -->> UI: caseCreated

    UI ->> Notif: sendCaseAssigned(consultantId)
    Notif -->> UI: delivered

    UI -->> ArlaEmployee: Case assignment confirmed
