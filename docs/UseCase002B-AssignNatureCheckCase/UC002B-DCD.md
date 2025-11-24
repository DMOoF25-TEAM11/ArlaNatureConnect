## UC002B – High-Level Design Class Diagram

This draft shows the main components that will collaborate when an Arla employee assigns a Nature Check Case to a consultant. It focuses on responsibilities and relationships only; properties/methods will be refined when the detailed design solidifies.

```mermaid
classDiagram
    direction TB

    namespace ArlaNatureConnect.WinUI.ViewModels.Pages {
        class ArlaEmployeePageViewModel {
            +ObservableCollection~Farm~ Farms
            +ObservableCollection~Person~ Consultants
            +Farm? SelectedFarm
            +Person? SelectedConsultant
            +string? AssignmentNotes
            +Task InitializeAsync(Role role)
            +Task AssignCaseAsync()
        }
    }

    namespace ArlaNatureConnect.Core.Services {
        class INatureCheckCaseService {
            <<interface>>
            +Task~(IReadOnlyList~Farm~,IReadOnlyList~Person~)~ LoadFarmsAndConsultantsAsync(CancellationToken) Task
            +Task~NatureCheckCase~ AssignCaseAsync(Guid farmId, Guid consultantId, Guid assignedByPersonId, string? notes, bool allowDuplicate, CancellationToken) Task
        }

        class NatureCheckCaseService {
            +Task~(IReadOnlyList~Farm~,IReadOnlyList~Person~)~ LoadFarmsAndConsultantsAsync(CancellationToken) Task
            +Task~NatureCheckCase~ AssignCaseAsync(Guid farmId, Guid consultantId, Guid assignedByPersonId, string? notes, bool allowDuplicate, CancellationToken) Task
        }
    }

    namespace ArlaNatureConnect.Domain.Entities {
        class NatureCheckCase {
            +Guid Id
            +Guid FarmId
            +Guid ConsultantId
            +Guid AssignedByPersonId
            +NatureCheckCaseStatus Status
            +string? Notes
            +string? Priority
            +DateTimeOffset CreatedAt
            +DateTimeOffset? AssignedAt
        }

        class Farm {
            +Guid Id
            +string Name
            +string CVR
            +Guid PersonId
            +Guid AddressId
        }

        class Person {
            +Guid Id
            +Guid RoleId
            +string FirstName
            +string LastName
            +string Email
            +bool IsActive
        }
    }

    namespace ArlaNatureConnect.Core.Abstract {
        class INatureCheckCaseRepository {
            <<interface>>
            +GetByIdAsync(Guid id, CancellationToken) Task~NatureCheckCase~
            +GetAllAsync(CancellationToken) Task~IEnumerable~NatureCheckCase~~
            +AddAsync(NatureCheckCase entity, CancellationToken) Task
            +UpdateAsync(NatureCheckCase entity, CancellationToken) Task
            +DeleteAsync(Guid id, CancellationToken) Task
        }

        class IFarmRepository {
            <<interface>>
        }

        class IPersonRepository {
            <<interface>>
            +GetPersonsByRoleAsync(string role, CancellationToken) Task~List~Person~~
        }

        class IAppMessageService {
            <<interface>>
        }

        class IStatusInfoServices {
            <<interface>>
        }

        class INotificationService {
            <<interface>>
            +NotifyConsultantAsync(Guid consultantId, Guid caseId, CancellationToken) Task
        }
    }

    namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts {
        class NavigationViewModelBase {
            <<abstract>>
            +string CurrentNavigationTag
            +List~Person~ AvailablePersons
            +Person? SelectedPerson
        }

        class ViewModelBase {
            <<abstract>>
        }
    }

    %% Inheritance
    ArlaEmployeePageViewModel --|> NavigationViewModelBase
    NavigationViewModelBase --|> ViewModelBase

    %% Service Implementation
    NatureCheckCaseService ..|> INatureCheckCaseService : implements

    %% Dependencies
    ArlaEmployeePageViewModel --> INatureCheckCaseService : uses
    ArlaEmployeePageViewModel --> IAppMessageService : uses
    ArlaEmployeePageViewModel --> IStatusInfoServices : uses
    NatureCheckCaseService --> INatureCheckCaseRepository : uses
    NatureCheckCaseService --> IFarmRepository : uses
    NatureCheckCaseService --> IPersonRepository : uses
    NatureCheckCaseService --> INotificationService : uses
    INatureCheckCaseRepository --> NatureCheckCase : manages
    IFarmRepository --> Farm : manages
    IPersonRepository --> Person : manages
```

**Noter**
- `ArlaEmployeePageViewModel` arver fra `NavigationViewModelBase` og står for UI-flowet: vise gårde/konsulenter og udløse `AssignCaseAsync`.
- `INatureCheckCaseService` og `NatureCheckCaseService` orkestrerer brugssagen og kalder repositories og notifikationsservice.
- Repositories er holdt abstrakte i `ArlaNatureConnect.Core.Abstract` for at matche nuværende arkitektur (EF Core implementationen tilføjes i Infrastructure).
- `IAppMessageService` og `IStatusInfoServices` er eksisterende Core services for feedback og status.
- `INotificationService` repræsenterer en planlagt infrastrukturtjeneste for notifikationer.
