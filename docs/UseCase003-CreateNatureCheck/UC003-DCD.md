```mermaid
classDiagram
    %% Services / ViewModels
    class CreateNatureCheckService {
      +IFarmRepository _farmRepository
      +IPersonRepository _personRepository
      +INatureCheckCaseRepository _natureCheckCaseRepository
      +ICreateNatureCheckRepository _createNatureCheckRepository
      +IEmailService _emailService
      +Task<Guid> CreateNatureCheckAsync(CreateNatureCheck request, CancellationToken ct)
      +Task<List<Farm>> GetFarmsAsync()
      +Task<List<Person>> GetPersonsAsync()
      +Task<List<NatureCheckCase>> GetNatureChecksAsync()
    }

    class ConsultantCreateNatureCheckViewModel {
      +ICreateNatureCheck _createNatureCheckService
      +ObservableCollection<Farm> Farms
      +ObservableCollection<Person> Persons
      +ObservableCollection<NatureCheckCase> NatureChecks
      +RelayCommand RefreshCommand
      +RelayCommand CreateNatureCheckCommand
      +Task LoadAsync()
      +Task CreateNatureCheckAsync()
    }

    %% DTO / Entities
    class CreateNatureCheck {
      +Guid NatureCheckId
      +Guid FarmId
      +Guid PersonId
      +string FarmName
      +int FarmCVR
      +string FarmAddress
      +string ConsultantFirstName
      +string ConsultantLastName
      +DateTimeOffset DateTime
    }

    class NatureCheckCase {
      +Guid Id
      +Guid FarmId
      +Guid ConsultantId
      +NatureCheckCaseStatus Status
      +DateTimeOffset CreatedAt
      +DateTimeOffset? AssignedAt
      +string? Priority
      +string? Notes
      +Farm Farm
      +Person Consultant
    }

    class Farm {
      +Guid Id
      +string Name
      +string CVR
      +Guid AddressId
      +Address Address
    }

    class Person {
      +Guid Id
      +string FirstName
      +string LastName
      +string Email
      +Guid RoleId
      +Guid AddressId
      +Address Address
    }

    class Address {
      +Guid Id
      +string Street
      +string PostalCode
      +string City
      +string Country
    }

    %% Repositories / DB
    class ICreateNatureCheckRepository {
      <<interface>>
      +Task<Guid> CreateNatureCheckAsync(CreateNatureCheck request, CancellationToken ct)
    }

    class INatureCheckCaseRepository {
      <<interface>>
      +Task<IReadOnlyList<NatureCheckCase>> GetAllAsync(...)
      +Task<IReadOnlyList<NatureCheckCase>> GetAssignedCasesForConsultantAsync(Guid consultantId,...)
      +Task<bool> FarmHasActiveCaseAsync(Guid farmId,...)
    }

    class uspCreateNatureCheck {
      <<stored procedure>>
      +@NatureCheckId OUTPUT, @FarmId, @PersonId, @FarmName, @FarmCVR, @FarmAddress, @ConsultantFirstName, @ConsultantLastName, @DateTime
      note left: Inserts into dbo.NatureCheck (Id, Date, FarmId, PersonId, FarmName, FarmCVR, FarmAddress, ConsultantFirstName, ConsultantLastName)
    }

    %% Relations / dependencies
    CreateNatureCheckService --> IFarmRepository : uses
    CreateNatureCheckService --> IPersonRepository : uses
    CreateNatureCheckService --> INatureCheckCaseRepository : uses
    CreateNatureCheckService --> ICreateNatureCheckRepository : calls
    CreateNatureCheckService --> IEmailService : sends email

    ConsultantCreateNatureCheckViewModel --> CreateNatureCheckService : uses
    ConsultantCreateNatureCheckViewModel --> Farm : displays
    ConsultantCreateNatureCheckViewModel --> Person : displays
    ConsultantCreateNatureCheckViewModel --> NatureCheckCase : displays

    ICreateNatureCheckRepository ..> uspCreateNatureCheck : executes
    uspCreateNatureCheck ..> NatureCheckCase : inserts row into dbo.NatureCheck

    NatureCheckCase --> Farm : references
    NatureCheckCase --> Person : references
    Farm --> Address : has
    Person --> Address : has