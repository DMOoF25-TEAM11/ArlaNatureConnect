```mermaid
sequenceDiagram
    title UC02b – Sequence Diagram: Assign Nature Check Case

    actor ArlaEmployee
    participant ViewModel as ArlaEmployeePageViewModel
    participant Service as NatureCheckCaseService
    participant FarmRepo as IFarmRepository
    participant PersonRepo as IPersonRepository
    participant CaseRepo as INatureCheckCaseRepository
    participant Notif as INotificationService
    participant MsgService as IAppMessageService

    ArlaEmployee ->> ViewModel: Open "Nature Check Cases" module
    ViewModel ->> Service: LoadFarmsAndConsultantsAsync()
    Service ->> FarmRepo: GetAllAsync()
    FarmRepo -->> Service: IEnumerable~Farm~
    Service ->> PersonRepo: GetPersonsByRoleAsync("Consultant")
    PersonRepo -->> Service: List~Person~
    Service -->> ViewModel: (IReadOnlyList~Farm~, IReadOnlyList~Person~)
    ViewModel -->> ArlaEmployee: Display farms and consultants

    alt Farm exists in list
        ArlaEmployee ->> ViewModel: Select existing farm
        ViewModel -->> ArlaEmployee: Display farm details
    else Farm does not exist
        Note over ArlaEmployee,ViewModel: Navigate to UC02 - Create Farm
        ArlaEmployee ->> ViewModel: Click "Create New Farm" (UC02)
        Note over ViewModel: UC02 handles farm creation
        ViewModel -->> ArlaEmployee: Return to farm list with new farm
        ArlaEmployee ->> ViewModel: Select newly created farm
        ViewModel -->> ArlaEmployee: Display farm details
    end

    ArlaEmployee ->> ViewModel: Click "Create Nature Check Case"
    ViewModel -->> ArlaEmployee: Display assignment form

    ArlaEmployee ->> ViewModel: Select consultant and enter notes
    ArlaEmployee ->> ViewModel: Submit assignment form

    ViewModel ->> Service: AssignCaseAsync(farmId, consultantId, assignedByPersonId, notes)
    Service ->> FarmRepo: GetByIdAsync(farmId)
    FarmRepo -->> Service: Farm
    Service ->> PersonRepo: GetByIdAsync(consultantId)
    PersonRepo -->> Service: Person (Consultant)
    
    alt Consultant has Consultant role
        Service ->> CaseRepo: AddAsync(natureCheckCase)
        CaseRepo -->> Service: Task completed
        Service ->> Notif: NotifyConsultantAsync(consultantId, caseId)
        Notif -->> Service: Task completed
        Service -->> ViewModel: NatureCheckCase
        ViewModel ->> MsgService: AddInfoMessage("Case assigned successfully")
        ViewModel -->> ArlaEmployee: Display confirmation message
    else Consultant does not have Consultant role
        Service -->> ViewModel: Validation error
        ViewModel ->> MsgService: AddErrorMessage("Selected user is not a consultant")
        ViewModel -->> ArlaEmployee: Error message displayed
    end
