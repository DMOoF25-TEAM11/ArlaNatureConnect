## UC002B – Sequence Diagram: Assign Nature Check Case

This sequence diagram shows the detailed interaction flow when an Arla employee assigns a Nature Check Case to a consultant, following Larmann's UML conventions.

```mermaid
sequenceDiagram
    title UC002B – Sequence Diagram: Assign Nature Check Case

    actor ArlaEmployee as Arla Employee
    participant ViewModel as ArlaEmployeeAssignNatureCheckViewModel
    participant Service as NatureCheckCaseService
    participant FarmRepo as IFarmRepository
    participant PersonRepo as IPersonRepository
    participant AddressRepo as IAddressRepository
    participant CaseRepo as INatureCheckCaseRepository
    participant RoleRepo as IRoleRepository
    participant MsgService as IAppMessageService
    participant StatusService as IStatusInfoServices

    ArlaEmployee ->>+ ViewModel: InitializeAsync()
    ViewModel ->>+ StatusService: BeginLoading()
    StatusService -->>- ViewModel: IDisposable
    ViewModel ->>+ Service: LoadAssignmentContextAsync()
    
    Service ->>+ FarmRepo: GetAllAsync()
    FarmRepo -->>- Service: List~Farm~
    Service ->>+ PersonRepo: GetAllAsync()
    PersonRepo -->>- Service: List~Person~
    Service ->>+ PersonRepo: GetPersonsByRoleAsync("Consultant")
    PersonRepo -->>- Service: List~Person~
    Service ->>+ AddressRepo: GetAllAsync()
    AddressRepo -->>- Service: List~Address~
    Service ->>+ CaseRepo: GetActiveCasesAsync()
    CaseRepo -->>- Service: IReadOnlyList~NatureCheckCase~
    
    Service ->>+ Service: CreateFarmAssignmentOverviewDto()
    Service -->>- Service: List~FarmAssignmentOverviewDto~
    Service -->>- ViewModel: NatureCheckCaseAssignmentContext
    
    ViewModel ->>+ ViewModel: UpdateFarms(context.Farms)
    ViewModel -->>- ViewModel: void
    ViewModel ->>+ ViewModel: UpdateConsultants(context.Consultants)
    ViewModel -->>- ViewModel: void
    ViewModel ->> StatusService: Dispose()
    ViewModel -->>- ArlaEmployee: void

    ArlaEmployee ->>+ ViewModel: SelectFarm(farmId)
    ViewModel ->>+ Service: GetActiveCaseForFarmAsync(farmId)
    Service ->>+ CaseRepo: GetActiveCaseForFarmAsync(farmId)
    CaseRepo -->>- Service: NatureCheckCase?
    
    alt Farm has active case
        Service -->>- ViewModel: NatureCheckCase
        ViewModel ->>+ ViewModel: PopulateFormWithCaseData()
        ViewModel -->>- ViewModel: void
        ViewModel -->>- ArlaEmployee: void
    else Farm has no active case
        Service -->>- ViewModel: null
        ViewModel -->>- ArlaEmployee: void
    end

    opt Farm does not exist
        rect rgb(240, 240, 240)
            Note over ArlaEmployee,ViewModel: UC002B.4: Create Farm (ref)
            ArlaEmployee ->> ViewModel: SaveFarmAsync(farmData)
            ViewModel ->> Service: SaveFarmAsync(request)
            Service -->> ViewModel: Farm
            ViewModel -->> ArlaEmployee: void
        end
    end

    alt Farm has active case
        ArlaEmployee ->>+ ViewModel: UpdateNatureCheckCaseAsync()
        rect rgb(240, 240, 240)
            Note over ViewModel,Service: UC002B.3: Update Case (ref)
            ViewModel ->> Service: UpdateCaseAsync(farmId, request)
            Service -->> ViewModel: NatureCheckCase
        end
        ViewModel -->>- ArlaEmployee: void
    else Farm has no active case
        ArlaEmployee ->>+ ViewModel: AssignNatureCheckCaseAsync()
        rect rgb(240, 240, 240)
            Note over ViewModel,Service: UC002B.2: Assign Case (ref)
            ViewModel ->> Service: AssignCaseAsync(request)
            Service -->> ViewModel: NatureCheckCase
        end
        ViewModel -->>- ArlaEmployee: void
    end
```

**Notes:**
- This epic-level SD shows the main flow with references to detailed user story SDs.
- Ref fragments (gray boxes) indicate where detailed flows are shown in UC002B.2, UC002B.3, and UC002B.4.
- All method calls use PascalCase (C# convention).
- All calls have return arrows (including void methods).
- Activation bars show object lifetime using automatic activation/deactivation (+/-).
- Exception handling follows UML conventions with alt fragments for alternatives.
- Maximum 3 levels of nested fragments for readability.
