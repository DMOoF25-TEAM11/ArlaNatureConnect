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

    %% Initialization Flow
    ArlaEmployee ->> ViewModel: Open "Nature Check Cases" module
    ViewModel ->> StatusService: BeginLoading()
    StatusService -->> ViewModel: IDisposable loading
    ViewModel ->> Service: LoadAssignmentContextAsync()
    
    %% Service loads data from multiple repositories
    Service ->> FarmRepo: GetAllAsync()
    FarmRepo -->> Service: List~Farm~
    Service ->> PersonRepo: GetAllAsync()
    PersonRepo -->> Service: List~Person~
    Service ->> PersonRepo: GetPersonsByRoleAsync("Consultant")
    PersonRepo -->> Service: List~Person~ (Consultants)
    Service ->> AddressRepo: GetAllAsync()
    AddressRepo -->> Service: List~Address~
    Service ->> CaseRepo: GetActiveCasesAsync()
    CaseRepo -->> Service: IReadOnlyList~NatureCheckCase~
    
    %% Service maps Entities to DTOs
    Note over Service: Maps Farm + Person + Address + Case → FarmAssignmentOverviewDto
    Service -->> ViewModel: NatureCheckCaseAssignmentContext<br/>(Farms: List~FarmAssignmentOverviewDto~,<br/>Consultants: List~Person~)
    
    %% ViewModel maps DTOs to ViewModel Items
    Note over ViewModel: Maps FarmAssignmentOverviewDto → AssignableFarmViewModel
    ViewModel ->> ViewModel: UpdateFarms(context.Farms)
    ViewModel ->> ViewModel: UpdateConsultants(context.Consultants)
    ViewModel ->> StatusService: Dispose loading
    ViewModel -->> ArlaEmployee: Display farms and consultants

    %% Farm Selection Flow
    alt Farm exists in list
        ArlaEmployee ->> ViewModel: Select existing farm
        ViewModel ->> ViewModel: SelectedFarm = farm
        alt Farm has active case
            Note over ViewModel: Auto-populate form with existing assignment data<br/>Button text = "Opdater natur Check opgave"
            ViewModel ->> ViewModel: SelectedConsultant = assigned consultant
            ViewModel ->> ViewModel: SelectedPriority = assigned priority (converted to Danish)
            ViewModel ->> ViewModel: AssignmentNotes = existing notes
            ViewModel ->> ViewModel: AssignCaseButtonText = "Opdater natur Check opgave"
        else Farm has no active case
            Note over ViewModel: Button text = "Lav natur Check opgave"
            ViewModel ->> ViewModel: AssignCaseButtonText = "Lav natur Check opgave"
        end
        ViewModel -->> ArlaEmployee: Display farm details and form
    else Farm does not exist
        Note over ArlaEmployee,ViewModel: Navigate to UC02 - Create Farm
        ArlaEmployee ->> ViewModel: Click "Create New Farm" (UC02)
        ViewModel ->> ViewModel: Show farm creation form
        ViewModel -->> ArlaEmployee: Display farm creation form
        ArlaEmployee ->> ViewModel: Fill form and click "Tilføj gård"
        ViewModel ->> ViewModel: Validate form data
        alt Validation fails
            ViewModel ->> MsgService: AddErrorMessage("Udfyld alle påkrævede felter")
            ViewModel -->> ArlaEmployee: Display error message
        else Validation succeeds
            ViewModel ->> StatusService: BeginLoading()
            ViewModel ->> Service: SaveFarmAsync(FarmRegistrationRequest)
            
            %% Service validates CVR
            Service ->> FarmRepo: GetByCvrAsync(request.Cvr)
            FarmRepo -->> Service: Farm?
            alt CVR already exists
                Service -->> ViewModel: InvalidOperationException("En gård med CVR-nummer '[CVR]' findes allerede i systemet. Vælg et andet CVR-nummer.")
                ViewModel ->> MsgService: AddErrorMessage(...)
                ViewModel -->> ArlaEmployee: Display error message
            else CVR is unique
                %% Check if owner email exists
                Service ->> PersonRepo: GetByEmailAsync(request.OwnerEmail)
                PersonRepo -->> Service: Person?
                alt Owner email exists
                    Service ->> RoleRepo: GetByIdAsync(existingPerson.RoleId)
                    RoleRepo -->> Service: Role?
                    alt Person does not have Farmer role
                        Service -->> ViewModel: InvalidOperationException("En person med e-mail '[Email]' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen.")
                        ViewModel ->> MsgService: AddErrorMessage(...)
                        ViewModel -->> ArlaEmployee: Display error message
                    else Person has Farmer role
                        Note over Service: Reuse existing person<br/>They can have multiple farms
                        Service ->> Service: Use existing person as owner
                        Service ->> AddressRepo: AddAsync(farmAddress)
                        AddressRepo -->> Service: Address
                        Service ->> FarmRepo: AddAsync(newFarm)
                        FarmRepo -->> Service: Farm
                        Service -->> ViewModel: Farm (created)
                        ViewModel ->> MsgService: AddInfoMessage("Ny gård er tilføjet")
                        ViewModel ->> ViewModel: Refresh farm list
                        ViewModel ->> StatusService: Dispose loading
                        ViewModel -->> ArlaEmployee: Display success message and updated list
                    end
                else Owner email does not exist
                    %% Create new person
                    Service ->> RoleRepo: GetByNameAsync("Farmer")
                    RoleRepo -->> Service: Role (Farmer)
                    alt Person address provided
                        Service ->> AddressRepo: AddAsync(personAddress)
                        AddressRepo -->> Service: Address
                    end
                    Service ->> PersonRepo: AddAsync(newPerson)
                    PersonRepo -->> Service: Person
                    Service ->> AddressRepo: AddAsync(farmAddress)
                    AddressRepo -->> Service: Address
                    Service ->> FarmRepo: AddAsync(newFarm)
                    FarmRepo -->> Service: Farm
                    Service -->> ViewModel: Farm (created)
                    ViewModel ->> MsgService: AddInfoMessage("Ny gård er tilføjet")
                    ViewModel ->> ViewModel: Refresh farm list
                    ViewModel ->> StatusService: Dispose loading
                    ViewModel -->> ArlaEmployee: Display success message and updated list
                end
            end
        end
        alt Farm created successfully
            ViewModel ->> ViewModel: Hide farm editor
            ViewModel -->> ArlaEmployee: Return to farm list with new farm
            ArlaEmployee ->> ViewModel: Select newly created farm
            ViewModel -->> ArlaEmployee: Display farm details<br/>Empty assignment form
        end
    end

    %% Assignment Flow
    ArlaEmployee ->> ViewModel: Select consultant, priority, and enter notes
    alt Farm has active case
        Note over ViewModel: Button text = "Opdater natur Check opgave"
        ArlaEmployee ->> ViewModel: Click "Opdater natur Check opgave"
    else Farm has no active case
        Note over ViewModel: Button text = "Lav natur Check opgave"
        ArlaEmployee ->> ViewModel: Click "Lav natur Check opgave"
    end
    
    ViewModel ->> ViewModel: Validate selection (farm and consultant selected)
    alt Validation fails
        ViewModel ->> MsgService: AddErrorMessage("Vælg gård og konsulent")
        ViewModel -->> ArlaEmployee: Display error message
    else Validation succeeds
        ViewModel ->> StatusService: BeginLoading()
        
        alt Farm has active case
            %% Update existing case
            ViewModel ->> Service: UpdateCaseAsync(farmId, NatureCheckCaseAssignmentRequest)
            
            %% Service validates and updates case
            Service ->> CaseRepo: GetActiveCaseForFarmAsync(farmId)
            CaseRepo -->> Service: NatureCheckCase?
            alt Active case not found
                Service -->> ViewModel: InvalidOperationException("Der findes ingen aktiv opgave")
                ViewModel ->> MsgService: AddErrorMessage(...)
                ViewModel -->> ArlaEmployee: Display error message
            else Active case found
                Service ->> PersonRepo: GetByIdAsync(request.ConsultantId)
                PersonRepo -->> Service: Person? (Consultant)
                alt Consultant not found
                    Service -->> ViewModel: InvalidOperationException("Konsulent findes ikke")
                    ViewModel ->> MsgService: AddErrorMessage(...)
                    ViewModel -->> ArlaEmployee: Display error message
                else Consultant found
                    Service ->> RoleRepo: GetByIdAsync(consultant.RoleId)
                    RoleRepo -->> Service: Role?
                    alt Consultant does not have Consultant role
                        Service -->> ViewModel: InvalidOperationException("Person har ikke konsulent-rollen")
                        ViewModel ->> MsgService: AddErrorMessage(...)
                        ViewModel -->> ArlaEmployee: Display error message
                    else Consultant has correct role
                        %% Update existing case
                        Service ->> Service: Update NatureCheckCase entity
                        Note over Service: ConsultantId = request.ConsultantId<br/>Priority = request.Priority<br/>Notes = request.Notes<br/>AssignedAt = DateTimeOffset.UtcNow
                        Service ->> CaseRepo: UpdateAsync(existingCase)
                        CaseRepo -->> Service: Task completed
                        Service -->> ViewModel: NatureCheckCase (updated entity)
                        ViewModel ->> MsgService: AddInfoMessage("Natur Check opgave er opdateret for [FarmName]")
                        ViewModel ->> ViewModel: Refresh farm list (reload data)
                        ViewModel ->> StatusService: Dispose loading
                        ViewModel -->> ArlaEmployee: Display success message and updated list
                    end
                end
            end
        else Farm has no active case
            %% Create new case
            ViewModel ->> Service: AssignCaseAsync(NatureCheckCaseAssignmentRequest)
            
            %% Service validates and creates case
            Service ->> FarmRepo: GetByIdAsync(request.FarmId)
            FarmRepo -->> Service: Farm?
            alt Farm not found
                Service -->> ViewModel: InvalidOperationException("Gården findes ikke")
                ViewModel ->> MsgService: AddErrorMessage(...)
                ViewModel -->> ArlaEmployee: Display error message
            else Farm found
                Service ->> PersonRepo: GetByIdAsync(request.ConsultantId)
                PersonRepo -->> Service: Person? (Consultant)
                alt Consultant not found
                    Service -->> ViewModel: InvalidOperationException("Konsulent findes ikke")
                    ViewModel ->> MsgService: AddErrorMessage(...)
                    ViewModel -->> ArlaEmployee: Display error message
                else Consultant found
                    Service ->> RoleRepo: GetByIdAsync(consultant.RoleId)
                    RoleRepo -->> Service: Role?
                    alt Consultant does not have Consultant role
                        Service -->> ViewModel: InvalidOperationException("Person har ikke konsulent-rollen")
                        ViewModel ->> MsgService: AddErrorMessage(...)
                        ViewModel -->> ArlaEmployee: Display error message
                    else Consultant has correct role
                        Service ->> CaseRepo: FarmHasActiveCaseAsync(farmId)
                        CaseRepo -->> Service: bool
                        alt Farm has active case AND allowDuplicate is false
                            Service -->> ViewModel: InvalidOperationException("Gården har allerede en aktiv opgave")
                            ViewModel ->> MsgService: AddErrorMessage(...)
                            ViewModel -->> ArlaEmployee: Display error message
                        else No active case OR allowDuplicate is true
                            %% Create new case
                            Service ->> Service: Create NatureCheckCase entity
                            Note over Service: Status = Assigned<br/>Priority = request.Priority<br/>Notes = request.Notes<br/>CreatedAt = DateTimeOffset.UtcNow<br/>AssignedAt = DateTimeOffset.UtcNow
                            Service ->> CaseRepo: AddAsync(natureCheckCase)
                            CaseRepo -->> Service: Task completed
                            Service -->> ViewModel: NatureCheckCase (created entity)
                            ViewModel ->> MsgService: AddInfoMessage("Natur Check opgave er oprettet for [FarmName]")
                            ViewModel ->> ViewModel: Refresh farm list (reload data)
                            ViewModel ->> StatusService: Dispose loading
                            ViewModel -->> ArlaEmployee: Display success message and updated list
                        end
                    end
                end
            end
        end
    end
```

