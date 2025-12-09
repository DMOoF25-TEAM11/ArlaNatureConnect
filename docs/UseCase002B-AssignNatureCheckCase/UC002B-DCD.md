## UC002B – Design Class Diagram

This diagram shows the main components that collaborate when an Arla employee assigns a Nature Check Case to a consultant. It follows Larmann's UML conventions with proper visibility notation, relationships, and namespace organization.

```mermaid
classDiagram
    direction TB
    %% === DCD – UC002B: Assign Nature Check Case ===

    %% === DOMAIN ENTITIES ===
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
            +Farm Farm
            +Person Consultant
            +Person AssignedByPerson
        }

        class Farm {
            +Guid Id
            +string Name
            +string CVR
            +Guid PersonId
            +Guid AddressId
            +Person Person
            +Address Address
        }

        class Person {
            +Guid Id
            +Guid RoleId
            +Guid AddressId
            +string FirstName
            +string LastName
            +string Email
            +bool IsActive
        }

        class Address {
            +Guid Id
            +string Street
            +string City
            +string PostalCode
            +string Country
        }

        class Role {
            +Guid Id
            +string Name
        }
    }

    %% === DTOs (Data Transfer Objects) ===
    namespace ArlaNatureConnect.Core.DTOs {
        class FarmAssignmentOverviewDto {
            +Guid FarmId
            +string FarmName
            +string Cvr
            +string OwnerFirstName
            +string OwnerLastName
            +string OwnerEmail
            +string Street
            +string City
            +string PostalCode
            +string Country
            +bool HasActiveCase
            +string? AssignedConsultantFirstName
            +string? AssignedConsultantLastName
            +Guid? AssignedConsultantId
            +string? Priority
            +string? Notes
            +string OwnerName
            +string AddressLine
            +string StatusLabel
            +string AssignedConsultantName
        }

        class ConsultantNotificationDto {
            +Guid CaseId
            +Guid FarmId
            +string FarmName
            +DateTimeOffset AssignedAt
            +string? Priority
            +string? Notes
        }

        class NatureCheckCaseAssignmentContext {
            +IReadOnlyList~FarmAssignmentOverviewDto~ Farms
            +IReadOnlyList~Person~ Consultants
        }

        class NatureCheckCaseAssignmentRequest {
            +Guid FarmId
            +Guid ConsultantId
            +Guid AssignedByPersonId
            +string? Notes
            +string? Priority
            +bool AllowDuplicateActiveCase
        }

        class FarmRegistrationRequest {
            +Guid? FarmId
            +string FarmName
            +string Cvr
            +string Street
            +string City
            +string PostalCode
            +string Country
            +string OwnerFirstName
            +string OwnerLastName
            +string OwnerEmail
            +bool OwnerIsActive
        }
    }

    %% === VIEWMODELS (Application Layer) ===
    namespace ArlaNatureConnect.WinUI.ViewModels.Pages {
        class ArlaEmployeeAssignNatureCheckViewModel {
            -INatureCheckCaseService _natureCheckCaseService
            -IAppMessageService _appMessageService
            -IStatusInfoServices _statusInfoServices
            -List~AssignableFarmViewModel~ _allFarms
            -AssignableFarmViewModel? _selectedFarm
            -Person? _selectedConsultant
            -string _farmSearchText
            -string _assignmentNotes
            -string? _selectedPriority
            -Guid? _assignedByPersonId
            +ObservableCollection~AssignableFarmViewModel~ FilteredFarms
            +ObservableCollection~Person~ Consultants
            +AssignableFarmViewModel? SelectedFarm
            +Person? SelectedConsultant
            +string? SelectedPriority
            +string AssignmentNotes
            +Guid? AssignedByPersonId
            +bool IsFarmSelected
            +string AssignCaseButtonText
            +RelayCommand AssignNatureCheckCaseCommand
            +RelayCommand~string~ ApplyStatusFilterCommand
            +RelayCommand ShowFarmEditorCommand
            +RelayCommand DeleteFarmCommand
            +Task InitializeAsync()
            +Task AssignNatureCheckCaseAsync()
            -Task EnsureAssignmentDataAsync()
            -void UpdateFarms(IReadOnlyList~FarmAssignmentOverviewDto~)
            -void UpdateConsultants(IEnumerable~Person~)
        }

        class ConsultantNatureCheckViewModel {
            -INatureCheckCaseService _natureCheckCaseService
            -ObservableCollection~ConsultantNotificationDto~ _notifications
            -int _notificationCount
            -Person? _selectedConsultant
            +ObservableCollection~ConsultantNotificationDto~ Notifications
            +int NotificationCount
            +bool HasNotifications
            +Person? SelectedConsultant
            +Task LoadNotificationsAsync()
        }
    }

    namespace ArlaNatureConnect.WinUI.ViewModels.Items {
        class AssignableFarmViewModel {
            -bool _hasActiveCase
            -string _farmName
            -string _cvr
            -string _ownerFirstName
            -string _ownerLastName
            -string _street
            -string _city
            -string _postalCode
            +Guid FarmId
            +string FarmName
            +string Cvr
            +string OwnerFirstName
            +string OwnerLastName
            +string OwnerName
            +string AddressLine
            +bool HasActiveCase
            +string StatusLabel
            +string? Priority
            +string? PriorityDisplay
            +Guid? AssignedConsultantId
            +string ConsultantDisplay
            +void Apply(FarmAssignmentOverviewDto)
        }
    }

    namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts {
        class ViewModelBase {
            <<abstract>>
            +event PropertyChangedEventHandler? PropertyChanged
            +void OnPropertyChanged(string?)
        }
    }

    %% === SERVICES (Core Layer) ===
    namespace ArlaNatureConnect.Core.Services {
        class INatureCheckCaseService {
            <<interface>>
            +Task~NatureCheckCaseAssignmentContext~ LoadAssignmentContextAsync(CancellationToken) Task
            +Task~NatureCheckCase~ AssignCaseAsync(NatureCheckCaseAssignmentRequest, CancellationToken) Task
            +Task~NatureCheckCase~ UpdateCaseAsync(Guid, NatureCheckCaseAssignmentRequest, CancellationToken) Task
            +Task~Farm~ SaveFarmAsync(FarmRegistrationRequest, CancellationToken) Task
            +Task DeleteFarmAsync(Guid, CancellationToken) Task
            +Task~IReadOnlyList~ConsultantNotificationDto~~ GetNotificationsForConsultantAsync(Guid, CancellationToken) Task
        }

        class NatureCheckCaseService {
            -IFarmRepository _farmRepository
            -IPersonRepository _personRepository
            -IAddressRepository _addressRepository
            -INatureCheckCaseRepository _natureCheckCaseRepository
            -IRoleRepository _roleRepository
            +Task~NatureCheckCaseAssignmentContext~ LoadAssignmentContextAsync(CancellationToken) Task
            +Task~NatureCheckCase~ AssignCaseAsync(NatureCheckCaseAssignmentRequest, CancellationToken) Task
            +Task~NatureCheckCase~ UpdateCaseAsync(Guid, NatureCheckCaseAssignmentRequest, CancellationToken) Task
            +Task~Farm~ SaveFarmAsync(FarmRegistrationRequest, CancellationToken) Task
            +Task DeleteFarmAsync(Guid, CancellationToken) Task
            +Task~IReadOnlyList~ConsultantNotificationDto~~ GetNotificationsForConsultantAsync(Guid, CancellationToken) Task
            -FarmAssignmentOverviewDto CreateFarmOverview(Farm, IDictionary~Guid,Person~, IDictionary~Guid,Address~, bool, IDictionary~Guid,NatureCheckCase~) FarmAssignmentOverviewDto
            -void ValidateFarmRegistration(FarmRegistrationRequest) void
            -Task~Role~ EnsureRoleAsync(string, CancellationToken) Task
        }

        class IAppMessageService {
            <<interface>>
            +void AddErrorMessage(string)
            +void AddInfoMessage(string)
            +void ClearErrorMessages()
            +bool HasErrorMessages
        }

        class IStatusInfoServices {
            <<interface>>
            +IDisposable BeginLoading()
        }
    }

    %% === REPOSITORIES (Core Abstract Layer) ===
    namespace ArlaNatureConnect.Core.Abstract {
        class IRepository~TEntity~ {
            <<interface>>
            +Task~TEntity?~ GetByIdAsync(Guid, CancellationToken) Task
            +Task~IEnumerable~TEntity~~ GetAllAsync(CancellationToken) Task
            +Task AddAsync(TEntity, CancellationToken) Task
            +Task UpdateAsync(TEntity, CancellationToken) Task
            +Task DeleteAsync(Guid, CancellationToken) Task
        }

        class INatureCheckCaseRepository {
            <<interface>>
            +Task~IReadOnlyList~NatureCheckCase~~ GetActiveCasesAsync(CancellationToken) Task
            +Task~bool~ FarmHasActiveCaseAsync(Guid, CancellationToken) Task
            +Task~IReadOnlyList~NatureCheckCase~~ GetAssignedCasesForConsultantAsync(Guid, CancellationToken) Task
            +Task~NatureCheckCase?~ GetActiveCaseForFarmAsync(Guid, CancellationToken) Task
        }

        class IFarmRepository {
            <<interface>>
        }

        class IPersonRepository {
            <<interface>>
            +Task~IEnumerable~Person~~ GetPersonsByRoleAsync(string, CancellationToken) Task
        }

        class IAddressRepository {
            <<interface>>
        }

        class IRoleRepository {
            <<interface>>
            +Task~Role?~ GetByNameAsync(string, CancellationToken) Task
        }
    }

    %% === RELATIONER (Relationships) ===
    %% Inheritance
    ArlaEmployeeAssignNatureCheckViewModel --|> ViewModelBase
    ConsultantNatureCheckViewModel --|> ViewModelBase
    AssignableFarmViewModel --|> ViewModelBase

    %% Interface Implementation
    NatureCheckCaseService ..|> INatureCheckCaseService : implements
    INatureCheckCaseRepository --|> IRepository : extends
    IFarmRepository --|> IRepository : extends
    IPersonRepository --|> IRepository : extends
    IAddressRepository --|> IRepository : extends
    IRoleRepository --|> IRepository : extends

    %% Dependencies (uses)
    ArlaEmployeeAssignNatureCheckViewModel --> INatureCheckCaseService : uses
    ArlaEmployeeAssignNatureCheckViewModel --> IAppMessageService : uses
    ArlaEmployeeAssignNatureCheckViewModel --> IStatusInfoServices : uses
    ConsultantNatureCheckViewModel --> INatureCheckCaseService : uses

    NatureCheckCaseService --> INatureCheckCaseRepository : uses
    NatureCheckCaseService --> IFarmRepository : uses
    NatureCheckCaseService --> IPersonRepository : uses
    NatureCheckCaseService --> IAddressRepository : uses
    NatureCheckCaseService --> IRoleRepository : uses

    %% Repository manages Entities
    INatureCheckCaseRepository --> NatureCheckCase : manages
    IFarmRepository --> Farm : manages
    IPersonRepository --> Person : manages
    IAddressRepository --> Address : manages
    IRoleRepository --> Role : manages

    %% ViewModel uses DTOs
    ArlaEmployeeAssignNatureCheckViewModel --> FarmAssignmentOverviewDto : uses
    ArlaEmployeeAssignNatureCheckViewModel --> NatureCheckCaseAssignmentContext : uses
    ArlaEmployeeAssignNatureCheckViewModel --> NatureCheckCaseAssignmentRequest : uses
    ArlaEmployeeAssignNatureCheckViewModel --> FarmRegistrationRequest : uses
    ConsultantNatureCheckViewModel --> ConsultantNotificationDto : uses

    %% ViewModel Items use DTOs
    AssignableFarmViewModel --> FarmAssignmentOverviewDto : uses

    %% Service uses DTOs
    NatureCheckCaseService --> FarmAssignmentOverviewDto : creates
    NatureCheckCaseService --> ConsultantNotificationDto : creates
    NatureCheckCaseService --> NatureCheckCaseAssignmentContext : creates
    NatureCheckCaseService --> NatureCheckCaseAssignmentRequest : uses
    NatureCheckCaseService --> FarmRegistrationRequest : uses

    %% Domain Entity Relationships (Navigation Properties)
    NatureCheckCase --> Farm : Farm
    NatureCheckCase --> Person : Consultant
    NatureCheckCase --> Person : AssignedByPerson
    Farm --> Person : Person
    Farm --> Address : Address
    Person --> Role : Role
    Person --> Address : Address
```

