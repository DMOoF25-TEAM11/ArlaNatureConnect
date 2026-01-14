# UC002 - Komplet Flow: Opret Person (fra UI til Database og tilbage)

## 📍 Oversigt

Dette dokument viser hele processen fra når brugeren udfylder person detaljer og trykker "Tilføj" i UI'en, gennem alle lag, til data bliver gemt i databasen og tilbage til UI'en igen.

---

## 🎯 Start: Brugeren udfylder form og trykker "Tilføj"

### 1. UI Layer (WinUI)
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Controls/SharedUC/`  
**Klasse:** `CRUDPersonUCViewModel`  
**Command:** `AddCommand` (fra base klasse)

**Hvad sker der?**
1. Brugeren udfylder form felter:
   - FirstName, LastName, Email
   - Role (dropdown)
   - Address (Street, City, PostalCode, Country)
   - IsActive (checkbox)
2. Brugeren trykker "Tilføj" knappen
3. UI kalder `AddCommand.Execute(null)`

**Interface/Arv:**
- Arver fra `CRUDViewModelBase<IPersonRepository, Person>` (base klasse)
- Base klasse arver fra `ListViewModelBase`
- Bruger `IPersonRepository` og `IAddressRepository` (dependency injection)

---

## 🔄 ViewModel Layer (WinUI)

### 2. Base Klasse - Command Handler
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/`  
**Klasse:** `CRUDViewModelBase<TRepos, TEntity>`  
**Metode:** `OnAddAsync()` (linje 241)

```csharp
// Hvad sker der:
// 1. Tjekker om AddCommand kan udføres (CanAdd())
// 2. Sætter IsSaving = true (viser loading state)
// 3. Kalder OnAddFormAsync() (override i CRUDPersonUCViewModel)
// 4. Sætter IsSaving = false
```

**Hvad gør metoden?**
- Validerer at command kan udføres (`CanAdd()`)
- Sætter loading state (`IsSaving = true`)
- Kalder den overridede `OnAddFormAsync()` metode
- Håndterer fejl og resetter state

**Arv:**
- `AddCommand` er defineret i base klasse (linje 104)
- `OnAddAsync()` er virtual, så den kan overrides

### 3. Derived Klasse - Form Handler
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Controls/SharedUC/`  
**Klasse:** `CRUDPersonUCViewModel`  
**Metode:** `OnAddFormAsync()` (linje 470)

```csharp
// Hvad sker der:
// 1. Opretter Address objekt fra form felter
// 2. Kalder AddressRepository.AddAsync() for at gemme adresse
// 3. Opretter Person objekt fra form felter
// 4. Kalder PersonRepository.AddAsync() for at gemme person
// 5. Reloader listen og resetter form
```

**Hvad gør metoden?**
- **Step 1:** Opretter `Address` objekt fra UI felter (Street, City, PostalCode, Country)
- **Step 2:** Kalder `_addressRepository.AddAsync(createdAddress)` - gemmer adresse først
- **Step 3:** Får AddressId fra gemt adresse
- **Step 4:** Opretter `Person` objekt med:
  - FirstName, LastName, Email fra UI
  - RoleId fra dropdown
  - AddressId fra gemt adresse
  - IsActive fra checkbox
- **Step 5:** Kalder `Repository.AddAsync(createdPerson)` - gemmer person
- **Step 6:** Reloader person listen (`GetAllAsync()`)
- **Step 7:** Resetter form (`OnResetFormAsync()`)

**Interfaces den bruger:**
- `IPersonRepository` - For person operationer (arver fra base)
- `IAddressRepository` - For adresse operationer (injected i constructor)

**Arv:**
- Overrider `OnAddFormAsync()` fra base klasse
- `Repository` property er af typen `IPersonRepository` (fra base klasse)

---

## 💾 Repository Layer (Infrastructure)

### 4. Core Layer - Repository Interfaces
**Mappe:** `src/ArlaNatureConnect.Core/Abstract/`  
**Interfaces:**
- `IRepository<TEntity>` - Base interface for alle repositories
- `IPersonRepository` - Extends `IRepository<Person>`
- `IAddressRepository` - Extends `IRepository<Address>`

**Hvad er det?**
- Interfaces der definerer hvad repositories skal kunne
- ViewModel kender kun interfaces, ikke konkrete implementeringer

### 5. Infrastructure Layer - Base Repository
**Mappe:** `src/ArlaNatureConnect.Infrastructure/Repositories/`  
**Klasse:** `Repository<TEntity>` (base klasse)  
**Metode:** `AddAsync()` (linje 44)

```csharp
// Hvad sker der:
// 1. Opretter DbContext fra factory
// 2. Tilføjer entity til DbSet (EF Core)
// 3. Gemmer til database (SaveChangesAsync)
// 4. Reloader entity fra database
// 5. Returnerer entity
```

**Hvad gør metoden?**
- Opretter en ny `AppDbContext` (database connection)
- Får `DbSet<TEntity>` fra context (f.eks. `DbSet<Person>`)
- Tilføjer entity til DbSet: `await dbSet.AddAsync(entity, cancellationToken)`
- Gemmer til database: `await ctx.SaveChangesAsync(cancellationToken)`
- Reloader entity: `await dbSet.FindAsync([entity.Id], cancellationToken)`
- Returnerer entity

**Arv:**
- Implementerer `IRepository<TEntity>` interface

### 6. Infrastructure Layer - Concrete Repositories
**Mappe:** `src/ArlaNatureConnect.Infrastructure/Repositories/`  
**Klasser:**
- `PersonRepository` - Arver fra `Repository<Person>`, implementerer `IPersonRepository`
- `AddressRepository` - Arver fra `Repository<Address>`, implementerer `IAddressRepository`

**Hvad gør de?**
- Arver alle CRUD operationer fra base `Repository<TEntity>` klasse
- `PersonRepository` har specifik metode: `GetPersonsByRoleAsync()` (linje 63)
- Implementerer deres specifikke interface

**Arv:**
- Arver fra `Repository<TEntity>`
- Implementerer deres specifikke interface (f.eks. `IPersonRepository`)

---

## 🗄️ Entity Framework Core Layer

### 7. EF Core - DbContext
**Mappe:** `src/ArlaNatureConnect.Infrastructure/Persistence/`  
**Klasse:** `AppDbContext`  
**Hvad er det?**
- EF Core's database context - repræsenterer databasen
- Indeholder `DbSet<TEntity>` properties for hver entity type
- Håndterer database connections og transactions

**DbSets:**
- `DbSet<Person> Persons { get; set; }`
- `DbSet<Address> Addresses { get; set; }`
- `DbSet<Role> Roles { get; set; }`

### 8. EF Core - DbSet Operations
**Hvad sker der?**
- `dbSet.AddAsync(entity)` - Markerer entity som "tilføjet" i EF Core's change tracker
- `ctx.SaveChangesAsync()` - Sender alle ændringer til databasen (INSERT, UPDATE, DELETE)
- `dbSet.FindAsync([id])` - Henter entity fra database baseret på ID

---

## 🗃️ Database Layer

### 9. SQL Server Database
**Hvad sker der?**
- EF Core genererer SQL INSERT statements
- Database eksekverer INSERT statements
- Data bliver gemt i tabellerne:
  - `ADDRESSES` tabel (først)
  - `PERSONS` tabel (efter adresse er gemt)
- Returnerer eventuelle auto-genererede værdier (f.eks. ID'er)

---

## 🔄 Returvej: Fra Database tilbage til UI

### 10. Database → EF Core
- Database returnerer gemt data
- EF Core mapper SQL resultat tilbage til C# entities
- `FindAsync()` returnerer `Address` og `Person` entities med alle properties sat

### 11. EF Core → Repository
- `AddressRepository.AddAsync()` returnerer `Address` entity
- `PersonRepository.AddAsync()` returnerer `Person` entity
- Entities har nu alle database-genererede værdier (ID'er, timestamps, etc.)

### 12. Repository → ViewModel
- `CRUDPersonUCViewModel.OnAddFormAsync()` modtager `Address` og `Person` entities
- Opdaterer `AddressId` property med gemt adresse ID
- Gemmer person med korrekt `AddressId`
- Reloader person listen: `await GetAllAsync()`
- Resetter form: `await OnResetFormAsync()`

### 13. ViewModel → UI
- UI opdateres automatisk via data binding
- Person listen viser den nye person
- Form bliver reset (tom)
- Success besked vises (hvis implementeret)

---

## 📊 Komplet Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ 1. UI LAYER (WinUI)                                         │
│    Mappe: ViewModels/Controls/SharedUC/                      │
│    Klasse: CRUDPersonUCViewModel                             │
│    Command: AddCommand.Execute(null)                        │
│    ↓ kalder                                                 │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. BASE VIEWMODEL (WinUI.Abstracts)                         │
│    Mappe: ViewModels/Abstracts/                              │
│    Klasse: CRUDViewModelBase<IPersonRepository, Person>      │
│    Metode: OnAddAsync()                                      │
│    ↓ kalder                                                 │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. DERIVED VIEWMODEL (WinUI.Controls.SharedUC)              │
│    Mappe: ViewModels/Controls/SharedUC/                      │
│    Klasse: CRUDPersonUCViewModel                             │
│    Metode: OnAddFormAsync()                                  │
│    ↓ kalder (2 gange)                                       │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. REPOSITORY INTERFACES (Core.Abstract)                   │
│    Mappe: Abstract/                                          │
│    Interfaces: IPersonRepository, IAddressRepository         │
│    ↓ implementeret af                                       │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. REPOSITORY IMPLEMENTATIONS (Infrastructure)              │
│    Mappe: Repositories/                                      │
│    Base: Repository<TEntity>                                │
│    Concrete: PersonRepository, AddressRepository            │
│    Metode: AddAsync() (arver fra base)                      │
│    ↓ bruger                                                 │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. EF CORE LAYER                                            │
│    Mappe: Persistence/                                       │
│    Klasse: AppDbContext                                      │
│    DbSet: Persons, Addresses                                │
│    Metoder: AddAsync(), SaveChangesAsync(), FindAsync()     │
│    ↓ genererer SQL                                          │
└─────────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. DATABASE (SQL Server)                                    │
│    Tabeller: PERSONS, ADDRESSES                              │
│    SQL: INSERT INTO ADDRESSES ...                            │
│         INSERT INTO PERSONS ...                               │
│    ↓ returnerer data                                        │
└─────────────────────────────────────────────────────────────┘
                    ↑
        (Samme vej tilbage gennem alle lag)
```

---

## 🔍 Detaljeret Metode Flow

### Eksempel: Opretter ny Person

```csharp
// 1. UI: Brugeren trykker "Tilføj" knap
AddCommand.Execute(null);
//     ↑ kalder

// 2. Base ViewModel: CRUDViewModelBase.OnAddAsync()
if (!CanAdd()) return;  // Validerer at command kan udføres
IsSaving = true;        // Viser loading state
await OnAddFormAsync(); // Kalder overrided metode
//     ↑ kalder

// 3. Derived ViewModel: CRUDPersonUCViewModel.OnAddFormAsync()
// Step 1: Opretter Address
Address createdAddress = new()
{
    Street = AddressStreet,      // Fra UI
    City = AddressCity,          // Fra UI
    PostalCode = AddressPostalCode, // Fra UI
    Country = AddressCountry     // Fra UI
};

// Step 2: Gemmer Address
Address savedAddress = await _addressRepository.AddAsync(createdAddress);
//     ↑ kalder via IAddressRepository interface
AddressId = savedAddress.Id;  // Gemmer AddressId til brug i Person

// Step 3: Opretter Person
Person createdPerson = new()
{
    Id = Guid.NewGuid(),
    FirstName = FirstName,      // Fra UI
    LastName = LastName,        // Fra UI
    Email = Email,              // Fra UI
    RoleId = RoleId,            // Fra UI dropdown
    AddressId = AddressId,      // Fra gemt Address
    IsActive = IsActive         // Fra UI checkbox
};

// Step 4: Gemmer Person
Person savedPerson = await Repository.AddAsync(createdPerson);
//     ↑ kalder via IPersonRepository (fra base klasse)

// Step 5: Reloader liste og resetter form
await GetAllAsync();           // Opdaterer person listen
await OnResetFormAsync();      // Resetter form felter
```

### Repository Flow (Address først, derefter Person):

```csharp
// AddressRepository.AddAsync() (arver fra Repository<Address>)
await using AppDbContext ctx = _factory.CreateDbContext();
DbSet<Address> dbSet = ctx.Set<Address>();
await dbSet.AddAsync(address, cancellationToken);
await ctx.SaveChangesAsync(cancellationToken);
// ↑ SQL: INSERT INTO ADDRESSES (Street, City, PostalCode, Country) VALUES (...)
Address? persisted = await dbSet.FindAsync([address.Id], cancellationToken);
return persisted ?? address;
//     ↑ returnerer Address med ID fra database

// PersonRepository.AddAsync() (arver fra Repository<Person>)
await using AppDbContext ctx = _factory.CreateDbContext();
DbSet<Person> dbSet = ctx.Set<Person>();
await dbSet.AddAsync(person, cancellationToken);
await ctx.SaveChangesAsync(cancellationToken);
// ↑ SQL: INSERT INTO PERSONS (FirstName, LastName, Email, RoleId, AddressId, IsActive) VALUES (...)
Person? persisted = await dbSet.FindAsync([person.Id], cancellationToken);
return persisted ?? person;
//     ↑ returnerer Person med ID fra database
```

---

## 📁 Mapper og Klasser Oversigt

### WinUI Layer
- **Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Controls/SharedUC/`
- **Klasse:** `CRUDPersonUCViewModel`
- **Arver fra:** `CRUDViewModelBase<IPersonRepository, Person>`
- **Bruger:** 
  - `IPersonRepository` (fra base klasse)
  - `IAddressRepository` (injected i constructor)
  - `IRoleRepository` (optional, injected i constructor)

### Base ViewModel Layer
- **Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/`
- **Klasse:** `CRUDViewModelBase<TRepos, TEntity>`
- **Arver fra:** `ListViewModelBase<TRepos, TEntity>`
- **Commands:** `AddCommand`, `SaveCommand`, `DeleteCommand`, `CancelCommand`

### Core Layer
- **Mappe:** `src/ArlaNatureConnect.Core/`
  - **Abstract/** - Repository interfaces
    - `IRepository<TEntity>` (base interface)
    - `IPersonRepository`, `IAddressRepository`, `IRoleRepository`
  - **DTOs/** - Data Transfer Objects (ikke brugt i dette flow)

### Infrastructure Layer
- **Mappe:** `src/ArlaNatureConnect.Infrastructure/`
  - **Repositories/** - Repository implementeringer
    - `Repository<TEntity>` (base klasse)
    - `PersonRepository`, `AddressRepository`, `RoleRepository`
  - **Persistence/** - EF Core
    - `AppDbContext` (database context)
    - `Configurations/` - EF Core konfigurationer

### Domain Layer
- **Mappe:** `src/ArlaNatureConnect.Domain/`
  - **Entities/** - Domain entities
    - `Person`, `Address`, `Role`

---

## 🔗 Interfaces og Arv

### Interface Hierarki:
```
IRepository<TEntity> (base interface)
  ├── IPersonRepository
  ├── IAddressRepository
  └── IRoleRepository
```

### Arv Hierarki:
```
ListViewModelBase<TRepos, TEntity>
  └── CRUDViewModelBase<IPersonRepository, Person>
      └── CRUDPersonUCViewModel

Repository<TEntity> (base klasse)
  ├── PersonRepository : Repository<Person>, IPersonRepository
  ├── AddressRepository : Repository<Address>, IAddressRepository
  └── RoleRepository : Repository<Role>, IRoleRepository
```

### ViewModel Dependencies:
```
CRUDPersonUCViewModel
  ├── IPersonRepository (fra base klasse som Repository property)
  ├── IAddressRepository (injected som _addressRepository)
  └── IRoleRepository? (optional, injected som _roleRepository)
```

---

## ✅ Hvad sker der i hvert lag?

### UI Layer (ViewModel)
- ✅ Samler data fra UI form felter
- ✅ Validerer at data er gyldig (CanAdd())
- ✅ Opretter Address og Person objekter
- ✅ Kalder repositories i korrekt rækkefølge (Address først, derefter Person)
- ✅ Opdaterer UI via data binding
- ✅ Resetter form efter success

### Repository Layer
- ✅ Skjuler database-detaljer
- ✅ Implementerer CRUD operationer
- ✅ Håndterer EF Core operations
- ✅ Returnerer entities med database-genererede værdier

### EF Core Layer
- ✅ Mapper C# entities til SQL
- ✅ Håndterer database connections
- ✅ Eksekverer SQL statements
- ✅ Mapper SQL resultater tilbage til entities

### Database Layer
- ✅ Gemmer data permanent
- ✅ Håndterer transactions
- ✅ Returnerer data

---

## 🧪 Dybdegående Gennemgang af Tests for UC002 Flow (Opret Person)

### Test Setup - Fake Repositories

**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 16-91

**Hvad er Fake Repositories?**
- I stedet for Mocks (Moq), bruger disse tests "Fake" repositories
- Fake repositories er simple in-memory implementeringer
- De gemmer data i en `Store` liste i stedet for database
- Lettere at teste end mocks for integration tests

**Eksempel på Fake Repository:**
```csharp
private sealed class FakePersonRepo : IPersonRepository
{
    public List<Person> Store { get; } = [];  // In-memory storage
    
    public Task<Person> AddAsync(Person entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        Store.Add(entity);  // Gemmer i liste i stedet for database
        return Task.FromResult(entity);
    }
    // ... andre metoder
}
```

**Hvorfor Fake i stedet for Mock?**
- Tester faktisk flow gennem ViewModel
- Verificerer at data faktisk bliver gemt (i Store)
- Lettere at verificere end mocks
- Tester integration mellem ViewModel og repositories

---

### Test 1: Success Flow Test (Happy Path) ⭐

**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 197-237  
**Test navn:** `AddCommand_Creates_Address_And_Person_And_Resets_Form`

#### Detaljeret gennemgang:

##### ARRANGE (Forberedelse) - Linje 200-223

```csharp
// 1. Opretter services og fake repositories
StatusInfoService status = new();
AppMessageService appMsg = new();
FakePersonRepo personRepo = new();      // In-memory person storage
FakeAddressRepo addrRepo = new();        // In-memory address storage
FakeRoleRepo roleRepo = new();          // In-memory role storage

// 2. Opretter test data (Role skal eksistere for validering)
Role r = new() { Id = Guid.NewGuid(), Name = "Employee" };
roleRepo.Store.Add(r);  // Tilføjer rolle til fake repository

// 3. Opretter ViewModel og udfylder form felter
CRUDPersonUCViewModel vm = new(status, appMsg, personRepo, addrRepo, roleRepo)
{
    FirstName = "F1",              // ✅ Gyldig data
    LastName = "L1",               // ✅ Gyldig data
    Email = "e@e.com",             // ✅ Gyldig data
    IsActive = true,                // ✅ Gyldig data
    AddressCity = "City",          // ✅ Gyldig data
    AddressStreet = "Street",      // ✅ Gyldig data
    AddressPostalCode = "P",       // ✅ Gyldig data
    RoleId = r.Id,                 // ✅ Gyldig data (role eksisterer)
    SelectedRole = r               // ✅ Gyldig data
};
```

**Hvad sker der i Arrange?**
- Opretter alle dependencies (services og fake repositories)
- Opretter test data (Role) og tilføjer til fake repository
- Opretter ViewModel med test data
- Udfylder alle form felter med gyldig data

##### ACT (Udførelse) - Linje 225-228

```csharp
// Verificerer at ViewModel er i AddMode
Assert.IsTrue(vm.IsAddMode);
// Verificerer at AddCommand kan udføres
Assert.IsTrue(vm.AddCommand.CanExecute(null));

// Kalder AddCommand (som brugeren ville trykke på)
vm.AddCommand.Execute(null);
```

**Hvad sker der internt?**
1. `AddCommand.Execute(null)` kalder `OnAddAsync()` (base klasse)
2. Base klasse kalder `OnAddFormAsync()` (override i CRUDPersonUCViewModel)
3. ViewModel opretter `Address` objekt og kalder `addrRepo.AddAsync(address)`
4. Fake repository gemmer Address i `Store` liste
5. ViewModel får AddressId fra gemt Address
6. ViewModel opretter `Person` objekt og kalder `personRepo.AddAsync(person)`
7. Fake repository gemmer Person i `Store` liste
8. ViewModel reloader liste og resetter form

**Hvorfor ingen rigtig database?**
- Fake repositories simulerer database operationer
- Vi tester ViewModel logik, ikke database funktionalitet
- Tests kører meget hurtigere uden database
- Lettere at verificere resultater (tjekker Store lister)

##### ASSERT (Verificering) - Linje 230-236

```csharp
// 1. Vent på at async operation er færdig
bool added = await WaitForAsync(() => personRepo.Store.Count > 0, 1000);
// ↑ Vent op til 1 sekund på at person bliver tilføjet

// 2. Verificerer at Person blev gemt
Assert.IsTrue(added, "Person repository should have an added person");
// ↑ Hvis personRepo.Store.Count == 0, fejler testen

// 3. Verificerer at Address blev gemt
Assert.IsNotEmpty(addrRepo.Store, "Address repository should have an added address");
// ↑ Hvis addrRepo.Store er tom, fejler testen

// 4. Verificerer at form blev reset
bool reset = await WaitForAsync(() => vm.FirstName == string.Empty && vm.IsAddMode, 1000);
Assert.IsTrue(reset, "Form should be reset after add completes");
// ↑ Verificerer at form felter er tomme og ViewModel er i AddMode
```

**Hvad tester vi her?**
- ✅ Address bliver oprettet og gemt først
- ✅ Person bliver oprettet med korrekt AddressId
- ✅ Begge entities bliver gemt i repositories
- ✅ Form bliver reset efter success
- ✅ ViewModel skifter tilbage til AddMode

**Hvorfor er denne test vigtig?**
- Dækker det primære use case scenarie (happy path)
- Verificerer at hele flowet virker end-to-end
- Sikrer at Address og Person oprettes i korrekt rækkefølge
- Verificerer at form resetter korrekt
- Hvis denne test fejler, virker hele "opret person" funktionaliteten ikke

**Hvad er `WaitForAsync`?**
- En helper metode der venter på at en betingelse bliver sand
- Nødvendig fordi ViewModel operationer er async
- Timeout på 1 sekund forhindrer at testen hænger for evigt

---

### Test 2: Validation Test (CanAdd)

**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 136-173 (PopulateFormFromPerson test - viser validering)

**Hvad tester denne test?**
- ✅ Form felter bliver korrekt udfyldt fra Person entity
- ✅ SelectedRole bliver sat korrekt
- ✅ Address felter bliver korrekt udfyldt

**Hvorfor vigtig?**
- Sikrer at data mapping virker korrekt
- Verificerer at form kan vises korrekt

---

### Test 3: Load Test (Edit Mode)

**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 175-195  
**Test navn:** `LoadAsync_Sets_SelectedItem_And_Populates_Form`

**Hvad tester denne test?**
- ✅ ViewModel kan loade eksisterende Person
- ✅ Form bliver udfyldt med Person data
- ✅ ViewModel skifter til EditMode

**Hvorfor vigtig?**
- Verificerer at "opdater person" funktionalitet virker
- Sikrer at data kan vises korrekt i form

---

## 📊 Test Coverage Oversigt

### Hvad dækker testene?

| Test | Hvad den tester | Hvorfor vigtig |
|------|----------------|----------------|
| **AddCommand Success** | Hele flowet virker end-to-end | Primær funktionalitet |
| **PopulateFormFromPerson** | Data mapping virker | Form visning |
| **LoadAsync** | Edit mode virker | Opdater funktionalitet |

### Hvad mangler der? (Potentielle fremtidige tests)

1. **Validation Test** - Teste at AddCommand ikke kan udføres med ugyldig data
2. **Error Handling Test** - Teste at fejl håndteres korrekt
3. **Update Test** - Teste at SaveCommand opdaterer eksisterende person
4. **Delete Test** - Teste at DeleteCommand sletter person
5. **Integration Test** - Teste med rigtig database (ikke fake repositories)

---

## 🎯 De 2 mest relevante tests:

### 1. Success Flow Test ⭐
**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 197-237  
**Test navn:** `AddCommand_Creates_Address_And_Person_And_Resets_Form`

**Hvorfor denne test er vigtig:**
- Tester hele flowet fra UI command til repository
- Verificerer at Address og Person oprettes i korrekt rækkefølge
- Sikrer at form resetter korrekt
- Dækker det primære use case scenarie (happy path)

### 2. Load Test (Edit Mode) ⭐
**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Controls/SharedUC/CRUDPersonUCViewModelTests.cs`  
**Linje:** 175-195  
**Test navn:** `LoadAsync_Sets_SelectedItem_And_Populates_Form`

**Hvorfor denne test er vigtig:**
- Verificerer at eksisterende person kan loades
- Sikrer at form kan vises korrekt
- Dækker "opdater person" scenarie

---

## 🔍 Forskelle mellem Person Tests og Farm Tests

### Person Tests (CRUDPersonUCViewModelTests):
- **Bruger Fake Repositories** - Simple in-memory implementeringer
- **Tester ViewModel direkte** - Ingen service layer
- **Integration test stil** - Tester hele flowet gennem ViewModel
- **Tester Address + Person** - To entities oprettes i rækkefølge

### Farm Tests (NatureCheckCaseServiceTests):
- **Bruger Mocks (Moq)** - Simulerer repository adfærd
- **Tester Service Layer** - ViewModel testes ikke
- **Unit test stil** - Tester isoleret service logik
- **Tester Address + Person + Farm** - Tre entities oprettes i rækkefølge

**Hvorfor forskellen?**
- Person flow har ingen service layer - ViewModel kalder repositories direkte
- Farm flow har service layer - Service orchestrerer repositories
- Forskellige test strategier for forskellige arkitekturer

---

## 🎯 Konklusion

**Hele processen:**
1. **UI** → Bruger udfylder form og trykker "Tilføj"
2. **Base ViewModel** → Validerer og kalder OnAddFormAsync()
3. **Derived ViewModel** → Opretter Address, gemmer den, opretter Person, gemmer den
4. **Repository Interfaces** → Definerer kontrakt
5. **Repository Implementations** → Håndterer data access
6. **EF Core** → Mapper til SQL
7. **Database** → Gemmer data (Address først, derefter Person)
8. **Tilbage gennem alle lag** → Data returneres til UI, liste opdateres, form resetter

**Vigtige principper:**
- **Separation of Concerns** - Hvert lag har sin egen ansvar
- **Dependency Inversion** - ViewModel bruger interfaces, ikke konkrete klasser
- **Single Responsibility** - Hver klasse har ét ansvar
- **DRY (Don't Repeat Yourself)** - Base Repository klasse genbruges
- **Inheritance** - CRUDPersonUCViewModel arver funktionalitet fra base klasser

**Forskelle fra Farm flow:**
- **Ingen Service Layer** - ViewModel kalder repositories direkte
- **To repository kald** - Address først, derefter Person
- **Base klasse pattern** - CRUDViewModelBase giver generisk CRUD funktionalitet
- **Command pattern** - AddCommand håndteres af base klasse

---

**Dette er hele flowet fra UI til database og tilbage igen for at oprette en Person! 🎉**
