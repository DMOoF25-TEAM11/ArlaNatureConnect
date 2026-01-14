# Eksamensforberedelse - Use Case 002 og 002B.2

## 📋 Indholdsfortegnelse
1. [HLD vs LLD Artefakter](#1-hld-vs-lld-artefakter)
2. [Kvalitetskriterier](#2-kvalitetskriterier)
3. [SCRUM Ceremonier](#3-scrum-ceremonier)
4. [Repository og Kodestruktur](#4-repository-og-kodestruktur)
5. [Async/Await](#5-asyncawait)
6. [Unit Tests - AAA Pattern](#6-unit-tests---aaa-pattern)

---

## 1. HLD vs LLD Artefakter

### Hvad er forskellen?

**HLD (High-Level Design)** = "Hvad skal systemet gøre?" - Overblik og arkitektur
**LLD (Low-Level Design)** = "Hvordan gør systemet det?" - Detaljeret implementering

### Konkrete eksempler fra vores projekt:

#### HLD Artefakter (High-Level):

1. **SSD (System Sequence Diagram)** - UC002B.2-SSD.md
   - **Hvad det viser**: Hvordan brugeren interagerer med systemet (sort boks)
   - **Eksempel fra UC002B.2**:
     ```
     ArlaEmployee -> System: assignCase(farmId, consultantId, priority, notes)
     System -> ArlaEmployee: caseAssigned(caseId) eller error
     ```
   - **Hvorfor HLD**: Viser kun "hvad" der sker, ikke "hvordan" systemet gør det internt

2. **OC (Operation Contract)** - UC002B.2-OC.md
   - **Hvad det viser**: Kontrakt for hvad operationen gør (preconditions, postconditions)
   - **Eksempel**:
     ```
     Operation: assignCase(farmId, consultantId, priority, notes)
     Preconditions: Farm eksisterer, Consultant har rigtig rolle
     Postconditions: Ny case oprettet, status = "Assigned"
     ```
   - **Hvorfor HLD**: Beskriver "hvad" operationen gør, ikke "hvordan" den gør det

3. **Domain Model** - UC002B.2-DomainModel.md
   - **Hvad det viser**: Hvilke domæne-objekter der findes (Farm, Person, NatureCheckCase)
   - **Hvorfor HLD**: Viser konceptuelle objekter, ikke implementeringsdetaljer

#### LLD Artefakter (Low-Level):

1. **SD (Sequence Diagram)** - UC002B.2-SD.md
   - **Hvad det viser**: Detaljeret flow mellem konkrete klasser
   - **Eksempel fra UC002B.2**:
     ```
     ViewModel -> Service: AssignCaseAsync(request)
     Service -> FarmRepo: GetByIdAsync(farmId)
     Service -> PersonRepo: GetByIdAsync(consultantId)
     Service -> CaseRepo: AddAsync(natureCheckCase)
     ```
   - **Hvorfor LLD**: Viser præcise klasser, metoder og rækkefølge

2. **DCD (Design Class Diagram)** - UC002B.2-DCD.md
   - **Hvad det viser**: Alle klasser med metoder, properties, dependencies
   - **Eksempel**:
     ```csharp
     class NatureCheckCaseService {
         -IFarmRepository _farmRepository
         +Task<NatureCheckCase> AssignCaseAsync(...)
     }
     ```
   - **Hvorfor LLD**: Viser præcis kode-struktur med interfaces og dependencies

3. **Koden selv** - NatureCheckCaseService.cs
   - **Hvad det viser**: Faktisk implementering
   - **Eksempel**:
     ```csharp
     public async Task<NatureCheckCase> AssignCaseAsync(...)
     {
         Farm? farm = await _farmRepository.GetByIdAsync(...);
         // ... validering ...
         await _natureCheckCaseRepository.AddAsync(entity, ...);
     }
     ```
   - **Hvorfor LLD**: Detaljeret implementering med alle tekniske detaljer

### Checkliste - Kan du forklare?

- [ ] Kan jeg forklare forskellen mellem SSD og SD?
- [ ] Kan jeg pege på et HLD-artefakt og forklare hvorfor det er HLD?
- [ ] Kan jeg pege på et LLD-artefakt og forklare hvorfor det er LLD?
- [ ] Kan jeg forklare hvordan HLD og LLD hænger sammen?

---

## 2. Kvalitetskriterier

### HLD Kvalitet - Hvad gør god HLD?

#### ✅ God HLD Praksis i vores projekt:

1. **SSD viser kun eksterne interaktioner**
   - ✅ UC002B.2-SSD.md: Viser kun `ArlaEmployee -> System`, ikke interne klasser
   - ❌ Dårligt ville være: At vise `ViewModel -> Service -> Repository` i SSD

2. **OC har klare preconditions og postconditions**
   - ✅ UC002B.2-OC.md: Operation er metodekald `assignCase(...)`, ikke fri tekst
   - ✅ Preconditions er konkrete: "Farm eksisterer", "Consultant har Consultant rolle"
   - ❌ Dårligt ville være: "Systemet skal validere" (for vagt)

3. **Domain Model er uafhængig af teknologi**
   - ✅ UC002B.2-DomainModel.md: Viser `NatureCheckCase` som koncept, ikke EF Core entity
   - ❌ Dårligt ville være: At nævne "DbContext" eller "LINQ" i domain model

### LLD Kvalitet - Hvad gør god LLD?

#### ✅ God LLD Praksis i vores projekt:

1. **SD følger Larmann's UML konventioner**
   - ✅ UC002B.2-SD.md: 
     - Alle calls har return arrows (selv void)
     - Activation bars bruger `+`/`-` notation
     - Max 3 niveauer af nested fragments
     - Metodenavne i PascalCase
   - ❌ Dårligt ville være: UI-beskrivelser i stedet for metodekald

2. **DCD viser korrekte dependencies**
   - ✅ UC002B.2-DCD.md:
     - Service afhænger af Repository interfaces (ikke konkrete klasser)
     - ViewModel afhænger af Service interface
     - Klare namespace-separationer
   - ❌ Dårligt ville være: Direkte afhængigheder til konkrete implementeringer

3. **Koden følger SOLID principper**
   - ✅ **Single Responsibility**: `NatureCheckCaseService` håndterer kun case-logik
   - ✅ **Dependency Inversion**: Bruger interfaces (`IFarmRepository`, ikke `FarmRepository`)
   - ✅ **Open/Closed**: Kan udvide med nye repositories uden at ændre service

### Kode Kvalitet - Hvad gør god kode?

#### ✅ God Kode Praksis i vores projekt:

1. **Async/Await korrekt brugt**
   ```csharp
   // ✅ Godt: ConfigureAwait(false) for bedre performance
   Farm? farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken)
       .ConfigureAwait(false);
   ```

2. **Exception handling**
   ```csharp
   // ✅ Godt: Konkrete exceptions med beskrivende beskeder
   if (farm == null)
       throw new InvalidOperationException("Gården findes ikke længere.");
   ```

3. **Null-sikkerhed**
   ```csharp
   // ✅ Godt: Null-coalescing operator
   Person? consultant = await _personRepository.GetByIdAsync(...)
       ?? throw new InvalidOperationException("Konsulent findes ikke.");
   ```

4. **Separation of Concerns**
   - ✅ Service layer: Business logic
   - ✅ Repository layer: Data access
   - ✅ ViewModel layer: UI logic

### Checkliste - Kan du forklare?

- [ ] Kan jeg forklare hvorfor vores SSD er god HLD?
- [ ] Kan jeg forklare hvorfor vores SD er god LLD?
- [ ] Kan jeg pege på 3 eksempler på god kode-kvalitet i vores projekt?
- [ ] Kan jeg forklare SOLID principper med eksempler fra vores kode?

---

## 3. SCRUM Ceremonier

### De 4 vigtigste SCRUM ceremonier:

1. **Sprint Planning**
   - **Hvad**: Planlægger hvad der skal laves i sprinten
   - **Konsekvenser hvis man springer over**: 
     - Uklarhed om hvad der skal laves
     - Forkerte prioriteringer
     - Teamet ved ikke hvad de skal arbejde på

2. **Daily Standup**
   - **Hvad**: Kort møde hver dag (15 min) - "Hvad lavede jeg i går? Hvad laver jeg i dag? Er der blokeringer?"
   - **Konsekvenser hvis man springer over**:
     - Teamet ved ikke hvad hinanden laver
     - Blokeringer opdages for sent
     - Ingen synliggørelse af fremskridt

3. **Sprint Review**
   - **Hvad**: Viser hvad der er lavet i sprinten til stakeholders
   - **Konsekvenser hvis man springer over**:
     - Stakeholders ved ikke hvad der er lavet
     - Ingen feedback på produktet
     - Risiko for at bygge forkert

4. **Sprint Retrospective**
   - **Hvad**: Reflekterer over processen - "Hvad gik godt? Hvad kan forbedres?"
   - **Konsekvenser hvis man springer over**:
     - Gentager samme fejl
     - Ingen kontinuerlig forbedring
     - Teamet lærer ikke af erfaringer

### I forhold til vores Use Cases:

- **UC002B.2 blev planlagt** i Sprint Planning
- **Daily Standups** hjalp med at opdage at DDL/DML/DQL filer skulle opdateres
- **Sprint Review** ville vise den færdige assignment-funktionalitet
- **Retrospective** kunne identificere at vi skulle have tænkt over EF Core fra starten

### Checkliste - Kan du forklare?

- [ ] Kan jeg navngive de 4 SCRUM ceremonier?
- [ ] Kan jeg forklare konsekvenserne ved at springe hver ceremoni over?
- [ ] Kan jeg give eksempler på hvordan ceremonierne hjalp i vores projekt?

---

## 4. Repository og Kodestruktur

### Repository Pattern - Hvad er det?

**Repository Pattern** = En lag der skjuler database-detaljer og giver et simpelt interface til data.

### Vores Repository Struktur:

```
Core.Abstract (Interfaces)
  ├── IRepository<TEntity>          (Base interface)
  ├── IFarmRepository              (Extends IRepository)
  ├── IPersonRepository            (Extends IRepository)
  └── INatureCheckCaseRepository   (Extends IRepository)

Infrastructure.Repositories (Implementations)
  ├── Repository<TEntity>          (Base implementation)
  ├── FarmRepository              (Implements IFarmRepository)
  ├── PersonRepository            (Implements IPersonRepository)
  └── NatureCheckCaseRepository   (Implements INatureCheckCaseRepository)
```

### Hvorfor denne struktur?

1. **Separation of Concerns**
   - Core layer ved ikke om vi bruger EF Core, SQL Server, eller noget andet
   - Infrastructure layer håndterer alle database-detaljer

2. **Testbarhed**
   - Vi kan mocke `IFarmRepository` i tests
   - Service layer kan testes uden database

3. **Fleksibilitet**
   - Kan skifte database-teknologi uden at ændre Core layer
   - Kan tilføje caching, logging, etc. i repository-laget

### Eksempel fra vores kode:

```csharp
// Interface (Core.Abstract)
public interface INatureCheckCaseRepository
{
    Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(
        CancellationToken cancellationToken = default);
    Task<bool> FarmHasActiveCaseAsync(
        Guid farmId, CancellationToken cancellationToken = default);
}

// Implementation (Infrastructure.Repositories)
public class NatureCheckCaseRepository : Repository<NatureCheckCase>, 
    INatureCheckCaseRepository
{
    public async Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(...)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        // EF Core implementation details her
    }
}

// Service bruger interface (Core.Services)
public class NatureCheckCaseService
{
    private readonly INatureCheckCaseRepository _natureCheckCaseRepository;
    
    public NatureCheckCaseService(INatureCheckCaseRepository repository)
    {
        _natureCheckCaseRepository = repository; // Dependency Injection
    }
}
```

### Kodestruktur - Layer Separation:

```
┌─────────────────────────────────────┐
│  WinUI (Presentation Layer)        │
│  - ViewModels                       │
│  - Views                            │
└──────────────┬──────────────────────┘
               │ Uses
┌──────────────▼──────────────────────┐
│  Core (Business Logic Layer)        │
│  - Services                         │
│  - DTOs                             │
│  - Interfaces (Abstract)           │
└──────────────┬──────────────────────┘
               │ Uses
┌──────────────▼──────────────────────┐
│  Infrastructure (Data Access Layer) │
│  - Repository Implementations       │
│  - EF Core Context                  │
│  - Configurations                   │
└─────────────────────────────────────┘
```

### Checkliste - Kan du forklare?

- [ ] Kan jeg forklare Repository Pattern med eksempler fra vores kode?
- [ ] Kan jeg forklare hvorfor vi har interfaces i Core og implementationer i Infrastructure?
- [ ] Kan jeg forklare hvordan Dependency Injection bruges i vores projekt?
- [ ] Kan jeg pege på forskellen mellem Core og Infrastructure layer?

---

## 5. Async/Await

### Hvad er Async/Await?

**Async/Await** = En måde at håndtere asynkron kode på, så UI'en ikke bliver frosset når vi venter på database eller netværk.

### Hvorfor bruger vi det?

1. **Database kald tager tid** - Vi vil ikke fryse UI'en mens vi venter
2. **Bedre brugeroplevelse** - UI'en forbliver responsiv
3. **Skalerbarhed** - Serveren kan håndtere flere requests samtidigt

### Hvor bruger vi Async/Await i vores projekt?

#### 1. Repository Layer - Database kald

```csharp
// NatureCheckCaseRepository.cs - Linje 19-30
public async Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(...)
{
    await using AppDbContext ctx = _factory.CreateDbContext();
    
    // ✅ Hvorfor async: Database kald kan tage tid
    List<NatureCheckCase> allCases = await ctx.NatureCheckCases
        .AsNoTracking()
        .ToListAsync(cancellationToken)  // ← EF Core async metode
        .ConfigureAwait(false);          // ← Bedre performance
    
    return activeCases;
}
```

**Forklaring**: 
- `ToListAsync()` venter på database at returnere data
- `.ConfigureAwait(false)` betyder "du behøver ikke vende tilbage til UI-thread"
- `await using` sikrer at DbContext bliver disposed korrekt

#### 2. Service Layer - Orchestrerer flere repository kald

```csharp
// NatureCheckCaseService.cs - Linje 101-140
public async Task<NatureCheckCase> AssignCaseAsync(...)
{
    // ✅ Hvorfor async: Vi laver flere database kald efter hinanden
    Farm? farm = await _farmRepository.GetByIdAsync(request.FarmId, cancellationToken)
        .ConfigureAwait(false);
    
    Person? consultant = await _personRepository.GetByIdAsync(request.ConsultantId, cancellationToken)
        .ConfigureAwait(false);
    
    Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken)
        .ConfigureAwait(false);
    
    bool hasActiveCase = await _natureCheckCaseRepository.FarmHasActiveCaseAsync(farm.Id, cancellationToken)
        .ConfigureAwait(false);
    
    // Opretter entity
    NatureCheckCase entity = new() { ... };
    
    // ✅ Hvorfor async: Gemmer i database
    await _natureCheckCaseRepository.AddAsync(entity, cancellationToken)
        .ConfigureAwait(false);
    
    return entity;
}
```

**Forklaring**:
- Hvert `await` venter på at database-operationen er færdig
- `ConfigureAwait(false)` bruges fordi vi ikke er i UI-thread
- `cancellationToken` tillader at annullere hvis brugeren afbryder

#### 3. ViewModel Layer - Kalder service og opdaterer UI

```csharp
// ArlaEmployeeAssignNatureCheckViewModel.cs
public async Task AssignNatureCheckCaseAsync()
{
    // ✅ Hvorfor async: Service kald kan tage tid
    try
    {
        NatureCheckCase result = await _natureCheckCaseService
            .AssignCaseAsync(request, cancellationToken);
        
        // Opdaterer UI efter success
        _appMessageService.AddInfoMessage("Natur Check opgave er oprettet");
    }
    catch (Exception ex)
    {
        _appMessageService.AddErrorMessage(ex.Message);
    }
}
```

**Forklaring**:
- UI-thread venter ikke blokerende på service-kaldet
- UI'en kan stadig reagere på bruger-input mens vi venter
- Efter `await` fortsætter koden på UI-thread (fordi vi IKKE bruger ConfigureAwait(false) her)

### ConfigureAwait(false) - Hvornår bruger vi det?

**Regel**: Brug `ConfigureAwait(false)` i kode der IKKE er i UI-layer.

```csharp
// ✅ Service/Repository layer - Brug ConfigureAwait(false)
await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

// ✅ ViewModel layer - IKKE ConfigureAwait(false) (vi vil tilbage til UI-thread)
await _service.DoSomethingAsync();
```

**Hvorfor**:
- `ConfigureAwait(false)` = "Jeg behøver ikke vende tilbage til den samme thread"
- Bedre performance (mindre overhead)
- Undgår deadlocks i nogle situationer

### Fordele og Ulemper:

#### ✅ Fordele:
1. **Responsiv UI** - Brugeren kan stadig interagere mens vi venter
2. **Bedre performance** - Serveren kan håndtere flere requests
3. **Skalerbarhed** - Mindre ressourcer per request

#### ⚠️ Ulemper:
1. **Mere kompleks kode** - Skal håndtere async/await korrekt
2. **Fejlhåndtering** - Exceptions skal håndteres i try/catch
3. **Debugging** - Kan være sværere at debugge async kode

### Checkliste - Kan du forklare?

- [ ] Kan jeg forklare hvorfor vi bruger async/await i repository layer?
- [ ] Kan jeg forklare hvorfor vi bruger async/await i service layer?
- [ ] Kan jeg forklare forskellen mellem med og uden ConfigureAwait(false)?
- [ ] Kan jeg pege på 3 steder i vores kode hvor vi bruger async/await og forklare hvorfor?

---

## 6. Unit Tests - AAA Pattern

### Hvad er AAA Pattern?

**AAA** = **Arrange, Act, Assert** - En struktur for unit tests.

1. **Arrange** = Forbered test-data og mocks
2. **Act** = Kald den metode vi tester
3. **Assert** = Tjek at resultatet er korrekt

### Eksempel fra vores tests:

```csharp
// NatureCheckCaseServiceTests.cs - Linje 182-222
[TestMethod]
public async Task LoadAssignmentContextAsync_WithValidData_ReturnsContext()
{
    CancellationToken cancellationToken = TestContext.CancellationToken;

    // ========== ARRANGE ==========
    // Forbereder test-data
    List<Farm> farms = [
        new() { Id = Guid.NewGuid(), Name = "Farm1", CVR = "123", 
                OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() }
    ];
    List<Person> persons = [
        new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", 
                RoleId = Guid.NewGuid() }
    ];
    List<Person> consultants = [
        new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", 
                RoleId = Guid.NewGuid() }
    ];
    List<Address> addresses = [
        new() { Id = Guid.NewGuid(), Street = "Street1", City = "City1" }
    ];

    // Sætter op mocks (simulerer repository kald)
    _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
        .ReturnsAsync(farms);
    _personRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
        .ReturnsAsync(persons);
    _personRepositoryMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), cancellationToken))
        .ReturnsAsync(consultants);
    _addressRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
        .ReturnsAsync(addresses);
    _natureCheckCaseRepositoryMock.Setup(r => r.GetActiveCasesAsync(cancellationToken))
        .ReturnsAsync([]);

    // ========== ACT ==========
    // Kalder den metode vi tester
    NatureCheckCaseAssignmentContext result = await _service.LoadAssignmentContextAsync(cancellationToken);

    // ========== ASSERT ==========
    // Tjekker at resultatet er korrekt
    Assert.IsNotNull(result);
    Assert.HasCount(1, result.Farms);
    Assert.HasCount(1, result.Consultants);
}
```

### Detaljeret gennemgang:

#### ARRANGE (Forberedelse):

```csharp
// 1. Opretter test-data
List<Farm> farms = [new() { Id = Guid.NewGuid(), Name = "Farm1", ... }];

// 2. Sætter op mocks (simulerer repository)
_farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
    .ReturnsAsync(farms);  // ← Når GetAllAsync kaldes, returner "farms"
```

**Hvad sker der?**
- Vi laver fake data (farms, persons, etc.)
- Vi fortæller mocks hvad de skal returnere når metoder kaldes
- Vi forbereder alt så testen kan køre isoleret (uden rigtig database)

#### ACT (Udførelse):

```csharp
// Kalder den metode vi tester
NatureCheckCaseAssignmentContext result = await _service.LoadAssignmentContextAsync(cancellationToken);
```

**Hvad sker der?**
- Vi kalder den faktiske metode vi vil teste
- Service'en kalder repository mocks (ikke rigtig database)
- Vi gemmer resultatet i `result` variablen

#### ASSERT (Verificering):

```csharp
// Tjekker at resultatet er korrekt
Assert.IsNotNull(result);              // ← Resultatet er ikke null
Assert.HasCount(1, result.Farms);      // ← Der er præcis 1 farm
Assert.HasCount(1, result.Consultants); // ← Der er præcis 1 consultant
```

**Hvad sker der?**
- Vi tjekker at resultatet matcher vores forventninger
- Hvis nogen assertion fejler, fejler testen

### Flere eksempler:

#### Eksempel 2: Test af exception handling

```csharp
// NatureCheckCaseServiceTests.cs - Linje 300-314
[TestMethod]
public async Task AssignCaseAsync_WithNullRequest_ThrowsArgumentNullException()
{
    CancellationToken cancellationToken = TestContext.CancellationToken;

    // ========== ARRANGE ==========
    // Ingen arrange nødvendig - vi tester null input

    // ========== ACT & ASSERT ==========
    // Vi forventer en exception, så vi kombinerer Act og Assert
    try
    {
        await _service.AssignCaseAsync(null!, cancellationToken);
        Assert.Fail("Expected ArgumentNullException");  // ← Hvis vi når her, fejler testen
    }
    catch (ArgumentNullException)
    {
        // Expected - testen passerer
    }
}
```

**Forklaring**:
- Nogle gange kombinerer vi Act og Assert (især ved exception tests)
- Vi forventer at en exception bliver kastet
- Hvis ingen exception kastes, fejler testen

#### Eksempel 3: Test af validation

```csharp
// NatureCheckCaseServiceTests.cs - Linje 371-397
[TestMethod]
public async Task AssignCaseAsync_WhenFarmDoesNotExist_ThrowsInvalidOperationException()
{
    CancellationToken cancellationToken = TestContext.CancellationToken;

    // ========== ARRANGE ==========
    NatureCheckCaseAssignmentRequest request = new()
    {
        FarmId = Guid.NewGuid(),
        ConsultantId = Guid.NewGuid(),
        AssignedByPersonId = Guid.NewGuid()
    };

    // Mock returnerer null (farm findes ikke)
    _farmRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
        .ReturnsAsync((Farm?)null);

    // ========== ACT & ASSERT ==========
    try
    {
        await _service.AssignCaseAsync(request, cancellationToken);
        Assert.Fail("Expected InvalidOperationException");
    }
    catch (InvalidOperationException)
    {
        // Expected - testen passerer
    }
}
```

**Forklaring**:
- Vi tester at systemet håndterer fejl korrekt
- Når farm ikke findes, skal der kastes en exception
- Testen verificerer at den rigtige exception kastes

### Korrekt brug af ACT:

#### ✅ Korrekt - Act er én linje:

```csharp
// Act
NatureCheckCase result = await _service.AssignCaseAsync(request, cancellationToken);
```

#### ❌ Forkert - Act indeholder for meget:

```csharp
// Act - FOR MEGET!
NatureCheckCase result = await _service.AssignCaseAsync(request, cancellationToken);
result.Status = NatureCheckCaseStatus.InProgress;  // ← Dette hører til Arrange eller Assert
await _service.UpdateCaseAsync(result);            // ← Dette er en anden operation
```

**Regel**: Act skal kun kalde den ene metode vi tester, ikke flere operationer.

### Checkliste - Kan du forklare?

- [ ] Kan jeg identificere Arrange, Act og Assert i en test?
- [ ] Kan jeg forklare hvad mocks gør i Arrange-delen?
- [ ] Kan jeg forklare hvorfor Act kun skal være én metodekald?
- [ ] Kan jeg pege på 3 forskellige typer tests (success, exception, validation) i vores projekt?

---

## 📝 Eksamen Tips

### Hvordan forbereder jeg mig?

1. **Læs gennem denne guide** - Sørg for at forstå hver sektion
2. **Gennemgå koden** - Find eksempler på det du læser om
3. **Forklar højt** - Prøv at forklare koncepterne med dine egne ord
4. **Brug checklisterne** - Test at du kan svare på alle spørgsmålene

### Hvad skal jeg kunne forklare mundtligt?

- **HLD vs LLD**: "SSD er HLD fordi det viser kun hvad systemet gør, ikke hvordan. SD er LLD fordi det viser præcise klasser og metoder."
- **Async/Await**: "Vi bruger async/await når vi kalder database fordi det tager tid, og vi vil ikke fryse UI'en. ConfigureAwait(false) bruger vi i service/repository layer for bedre performance."
- **AAA Pattern**: "Arrange er hvor vi forbereder test-data og mocks. Act er hvor vi kalder den metode vi tester. Assert er hvor vi tjekker resultatet."

### Eksempel på god forklaring:

**Spørgsmål**: "Forklar hvorfor vi bruger Repository Pattern i jeres projekt."

**Godt svar**:
"Repository Pattern skjuler database-detaljer fra vores business logic. I vores projekt har vi interfaces i Core layer, som `IFarmRepository`, og implementeringer i Infrastructure layer, som `FarmRepository`. Dette gør det muligt at teste vores services uden en rigtig database, fordi vi kan mocke repository interfaces. Det gør også koden mere fleksibel - hvis vi vil skifte fra EF Core til noget andet, skal vi kun ændre Infrastructure layer, ikke Core layer."

**Dårligt svar**:
"Vi bruger repositories fordi det er god praksis. De håndterer data."

---

## 🎯 Quick Reference

### HLD Artefakter:
- SSD, OC, Domain Model, ER Diagram

### LLD Artefakter:
- SD, DCD, Koden selv, DbScheme

### Async/Await:
- Repository: `await _repo.GetByIdAsync(...).ConfigureAwait(false)`
- Service: `await _service.DoSomethingAsync(...).ConfigureAwait(false)`
- ViewModel: `await _service.DoSomethingAsync(...)` (uden ConfigureAwait)

### AAA Pattern:
- **Arrange**: Test-data + mocks
- **Act**: Én metodekald
- **Assert**: Verificering af resultat

### Repository Pattern:
- Interface i Core.Abstract
- Implementation i Infrastructure.Repositories
- Dependency Injection i Service constructor

---

**God eksamen! 🍀**
