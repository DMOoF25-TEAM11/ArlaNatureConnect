# UC001 - Komplet Flow: Login & Role Access (fra UI til Database og tilbage)

## 📍 Oversigt

Dette dokument viser hele processen fra når brugeren åbner applikationen, vælger en rolle, og (hvis nødvendigt) vælger en specifik person, gennem alle lag, til data bliver hentet fra databasen og vist i UI'en.

**⚠️ VIGTIGT:** 
- **SD diagrammet** er en abstraktion der viser hovedflowet. Det skal bruges til eksamen.
- **Faktisk kode** har ekstra detaljer (navigation handling, side menu initialization) som ikke er vist i SD'en.
- Dette dokument viser **både SD'en struktur** (for eksamen) og **den faktiske kode struktur** (for forståelse).

**✅ Hvad matcher 100% mellem SD'en, koden og dette dokument:**
- ✅ **Role selection flow** - SelectRole() → Navigate() → InitializeAsync() matcher præcist
- ✅ **Page initialization** - NavPage.OnNavigatedTo() → ViewModel.InitializeAsync() → AttachSideMenuToMainWindow() matcher
- ✅ **Side menu initialization** - AttachSideMenuToMainWindow() → Side menu ViewModel.InitializeAsync() matcher
- ✅ **Person loading flow** - LoadAvailablePersonsAsync() → GetPersonsByRoleAsync() matcher
- ✅ **Person selection flow** - SelectedPerson setter → ChooseUserCommand.Execute() matcher
- ✅ **Navigation** - Navigation til korrekt side baseret på rolle matcher

**⚠️ Forskelle mellem SD'en og koden (SD'en er abstraktion):**
- SD'en viser `FarmerPageViewModel -> FarmerPageViewModel: AttachSideMenuToMainWindow()` - Dette matcher koden (kaldes fra NavPage.OnNavigatedTo(), linje 58)
- SD'en viser `FarmerPageViewModel -> FarmerPageSideMenuUCViewModel: InitializeAsync()` - Dette matcher koden (kaldes via AttachSideMenuToMainWindow() og side menu control's Loaded event)
- SD'en viser ikke `LoadAvailablePersonsAsync()` kald - Dette er en intern metode i side menu ViewModel
- SD'en viser ikke `TrySetAvailablePersons()` kald - Dette er en intern metode i side menu ViewModel
- SD'en viser `User -> FarmerPageSideMenuUCViewModel: SelectedPerson = person` - Dette matcher koden (via data binding)
- SD'en viser ikke navigation handler detaljer - Koden bruger INavigationHandler interface
- SD'en viser ikke Arla Employee direkte navigation detaljer - Koden har direkte navigation uden person valg

---

## 🎯 Start: Brugeren åbner applikationen

### 1. UI Layer (WinUI)
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/Views/Pages/`  
**View:** `LoginPage.xaml`  
**ViewModel:** `LoginPageViewModel` (linje 23)

**Hvad sker der?**
1. Brugeren åbner applikationen
2. LoginPage vises med tre knapper:
   - "Landmand" (Farmer)
   - "Konsulent" (Consultant)
   - "Arla Medarbejder" (Arla Employee)
3. Brugeren klikker på en rolle knap

**Interface/Arv:**
- ViewModel arver fra `NavigationViewModelBase` (base klasse)
- Bruger `INavigationHandler` (dependency injection)

---

## 🔄 ViewModel Layer (WinUI)

### 2. ViewModel - Role Selection
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Pages/`  
**Klasse:** `LoginPageViewModel`  
**Command:** `SelectRoleCommand` (linje 37)  
**Metode:** `SelectRole()` (linje 75)

**Note:** Dette flow følger SD diagrammet præcist. SD diagrammet viser `SelectRoleCommand.Execute(role)` → `SelectRole(roleName)` → `Navigate(pageType, role)`.

**Faktisk kode struktur (linje 75-117):**
- **Step 1:** Validerer role name (linje 77-80):
  - Hvis `string.IsNullOrWhiteSpace(roleName)`: Returnerer tidligt
- **Step 2:** Opretter Role objekt (som i SD'en, linje 88, 95, 103, 109):
  ```csharp
  SelectedRole = new Role { Name = roleName };
  ```
- **Step 3:** Navigerer baseret på rolle (som i SD'en, linje 83-116):
  - **Farmer/Landmand:** `_navigationHandler.Navigate(typeof(FarmerPage), SelectedRole);` (linje 89)
  - **Consultant/Konsulent:** `_navigationHandler.Navigate(typeof(ConsultantPage), SelectedRole);` (linje 96)
  - **Arla Employee:** `_navigationHandler.Navigate(typeof(ArlaEmployeePage), SelectedRole);` (linje 104)
  - **Administrator:** `_navigationHandler.Navigate(typeof(AdministratorPage), SelectedRole);` (linje 110)

**Interfaces den bruger:**
- `INavigationHandler` - For navigation mellem sider

**Arv:**
- Arver fra `NavigationViewModelBase` (base klasse for navigation)

---

## 🎯 Navigation Handler

### 3. Navigation Handler
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/Services/`  
**Interface:** `INavigationHandler`  
**Implementation:** `NavigationHandler`

**Hvad sker der?**
- Navigation handler modtager page type og parameter (Role objekt)
- Navigation handler navigerer til den korrekte side
- Role objekt sendes med som parameter til den nye side

---

## 🔄 Page Initialization (Farmer/Consultant)

### 4. Page ViewModel - Initialize
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Pages/`  
**Klasser:** `FarmerPageViewModel`, `ConsultantPageViewModel`

**Note:** SD'en viser `FarmerPage -> FarmerPageViewModel: InitializeAsync(role)` - Dette matcher koden:
- `NavPage.OnNavigatedTo()` kalder `ViewModel.InitializeAsync(role)` (linje 54 i NavPage.cs)
- `FarmerPageViewModel.InitializeAsync(role)` returnerer `Task.CompletedTask` (har ingen faktisk implementation, linje 156 i NavigationViewModelBase.cs)
- Efter `InitializeAsync()` kalder `NavPage.OnNavigatedTo()` også `ViewModel.AttachSideMenuToMainWindow()` (linje 58 i NavPage.cs)
- `AttachSideMenuToMainWindow()` opretter side menu control og ViewModel via DI, og kalder `InitializeAsync()` på side menu ViewModel (linje 247-365 i NavigationViewModelBase.cs)

**Hvad sker der? (Følger SD diagrammet præcist)**
1. Page ViewModel oprettes (via dependency injection i Page constructor, linje 21 i FarmerPage.xaml.cs)
2. Page ViewModel sætter side menu types i constructor (linje 30-31 i FarmerPageViewModel.cs)
3. Når Page navigeres til (`NavPage.OnNavigatedTo()`):
   - Kalder `ViewModel.InitializeAsync(role)` (linje 54 i NavPage.cs)
   - Kalder `ViewModel.AttachSideMenuToMainWindow()` (linje 58 i NavPage.cs)
4. `AttachSideMenuToMainWindow()` (linje 247-365 i NavigationViewModelBase.cs):
   - Opretter side menu ViewModel via DI (linje 277)
   - Opretter side menu control (linje 311-313)
   - Sætter DataContext og tilføjer til main window (linje 360-362)
   - Side menu control's `Loaded` event kalder `InitializeAsync()` på ViewModel (linje 66 i FarmerPageSideMenuUC.xaml.cs)

**FarmerPageViewModel (linje 25-35):**
- Ingen `InitializeAsync(role)` metode (i modsætning til SD'en)
- Sætter `SideMenuControlType = typeof(FarmerPageSideMenuUC);` (linje 30)
- Sætter `SideMenuViewModelType = typeof(FarmerPageSideMenuUCViewModel);` (linje 31)
- Navigerer til default view: `NavigateToView(() => new FarmerDashboards());` (linje 33)

**ConsultantPageViewModel (linje 17-83):**
- `InitializeAsync(role)` returnerer `Task.CompletedTask` (linje 85) - ingen faktisk initialization
- Sætter `SideMenuControlType = typeof(ConsultantPageSideMenuUC);` (linje 60, 73)
- Sætter `SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);` (linje 61, 74)
- Opretter `ConsultantNatureCheckViewModel` (hvis service er tilgængelig, linje 77)
- Sætter default content: `SetContent("Dashboards");` (linje 66, 82)

---

## 🔄 Side Menu ViewModel - Load Persons

### 5. Side Menu ViewModel - Initialize
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Controls/SideMenu/`  
**Klasser:** `FarmerPageSideMenuUCViewModel`, `ConsultantPageSideMenuUCViewModel`  
**Base:** `SideMenuViewModelBase`  
**Metode:** `InitializeAsync()` (i base: `LoadAvailablePersonsAsync()` - linje 143)

**Note:** Dette flow følger SD diagrammet præcist. SD diagrammet viser `InitializeAsync()` → `LoadAvailablePersonsAsync(role)` → `GetPersonsByRoleAsync(role)`.

**Faktisk kode struktur:**

**FarmerPageSideMenuUCViewModel.InitializeAsync() (linje 123-136):**
- **Step 1:** Sætter loading state: `IsLoading = true;` (linje 125)
- **Step 2:** Kalder base metode (som i SD'en, linje 129):
  - `await LoadAvailablePersonsAsync(RoleName.Farmer);`
- **Step 3:** Sætter loading state: `IsLoading = false;` (linje 133)

**Note:** SD'en viser `FarmerPageViewModel -> FarmerPageSideMenuUCViewModel: InitializeAsync()` - Dette matcher koden:
- `AttachSideMenuToMainWindow()` opretter side menu ViewModel og control (linje 247-365 i NavigationViewModelBase.cs)
- Side menu control's `Loaded` event kalder `InitializeAsync()` på ViewModel (linje 66 i FarmerPageSideMenuUC.xaml.cs)
- Side menu ViewModel kalder også `InitializeAsync()` i sin constructor (fire-and-forget, linje 50 i FarmerPageSideMenuUCViewModel.cs), men det primære kald er fra control's Loaded event

**SideMenuViewModelBase.LoadAvailablePersonsAsync() (linje 143-156):**
- **Step 1:** Validerer role (linje 145-150):
  - Hvis `string.IsNullOrWhiteSpace(role)`: Rydder persons og returnerer
- **Step 2:** Henter personer fra repository (som i SD'en, linje 152):
  - `IEnumerable<Person> persons = await _repository.GetPersonsByRoleAsync(role);`
- **Step 3:** Opdaterer collection (linje 155):
  - `TrySetAvailablePersons(persons);` (opdaterer `AvailablePersons` collection, linje 265-286 i SideMenuViewModelBase.cs)
  - **Note:** SD'en viser ikke `UpdateAvailablePersons()` kald til Page - Dette matcher koden. Collection opdateres direkte i ViewModel, og UI bindings opdateres automatisk via data binding.

**Interfaces den bruger:**
- `IPersonRepository` - For at hente personer

**Arv:**
- Arver fra `SideMenuViewModelBase` (base klasse for side menus)

---

## 🗄️ Repository Layer (Infrastructure)

### 6. Repository Interface
**Mappe:** `src/ArlaNatureConnect.Core/Abstract/`  
**Interface:** `IPersonRepository`

**Hvad er det?**
- Interface der definerer hvad repository skal kunne
- Side menu ViewModel kender kun interface, ikke konkrete implementeringer

### 7. Repository Implementation
**Mappe:** `src/ArlaNatureConnect.Infrastructure/Repositories/`  
**Klasse:** `PersonRepository`  
**Metode:** `GetPersonsByRoleAsync()` (linje 63)

**Hvad sker der?**
- Repository kalder EF Core `DbContext` for at hente data
- EF Core oversætter LINQ query til SQL
- SQL køres mod databasen
- Resultater mappes tilbage til C# entities

---

## 🗄️ EF Core Layer

### 8. EF Core - DbContext
**Mappe:** `src/ArlaNatureConnect.Infrastructure/Persistence/`  
**Klasse:** `AppDbContext`  
**DbSet:** `Persons`

**Hvad sker der?**
- EF Core oversætter LINQ query til SQL
- SQL køres mod SQL Server databasen
- Resultater mappes tilbage til C# entities

**SQL der genereres (approksimativt):**
```sql
SELECT p.* FROM PERSONS p
INNER JOIN ROLES r ON p.RoleId = r.Id
WHERE r.Name = @RoleName
AND p.IsActive = 1
ORDER BY p.FirstName, p.LastName;
```

---

## 🗃️ Database Layer

### 9. SQL Server Database
**Tabeller:** `PERSONS`, `ROLES`

**Hvad sker der?**
- SQL query køres mod databasen
- Data returneres som result set
- EF Core mapper result set til C# entities

---

## 🔄 Tilbage gennem lagene (Person Loading)

### 10. Data flyder tilbage gennem lagene

1. **Database → EF Core:**
   - SQL result set returneres
   - EF Core mapper til C# entities

2. **EF Core → Repository:**
   - Entities returneres til repository
   - Repository returnerer `IEnumerable<Person>` til ViewModel

3. **Repository → Side Menu ViewModel:**
   - Side menu ViewModel modtager personer
   - Side menu ViewModel opdaterer `AvailablePersons` collection
   - UI bindings opdateres automatisk
   - Dropdown vises med personer

4. **Side Menu ViewModel → UI:**
   - UI viser dropdown med personer
   - Loading indicator forsvinder

---

## 🔄 Person Selection Flow

### 11. Brugeren vælger person

**Note:** SD'en viser `User -> FarmerPage: SelectPerson(person)` og derefter `FarmerPage -> FarmerPageSideMenuUCViewModel: SelectedPerson = person`, men i koden binder UI direkte til `SelectedPerson` property i Side Menu ViewModel. Der er ingen direkte kald fra Page.

**Hvad sker der?**
1. Brugeren vælger en person fra dropdown
2. UI binder direkte til `SelectedPerson` property i Side Menu ViewModel (via data binding)
3. Property setter kaldes automatisk

### 12. Side Menu ViewModel - Person Selection
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/`  
**Klasse:** `SideMenuViewModelBase`  
**Property:** `SelectedPerson` (setter, linje 106)

**Note:** Dette flow følger SD diagrammet præcist. SD diagrammet viser `SelectedPerson = person` → `ChooseUserCommand.Execute(person)` → `UpdateDashboard(person)`.

**Faktisk kode struktur (linje 106-123):**
- **Step 1:** Property setter kaldes (linje 109-112):
  - Sætter field og kalder `OnPropertyChanged()`
- **Step 2:** Tjekker om person er null (linje 113):
  - Hvis `value != null`: Fortsætter
- **Step 3:** Henter ChooseUserCommand (linje 116):
  - `RelayCommand<Person>? chooseCommand = _navigationViewModel?.ChooseUserCommand;`
- **Step 4:** Kalder command (som i SD'en, linje 117-120):
  - Hvis command eksisterer og kan udføres: `chooseCommand.Execute(value);`

### 13. Page ViewModel - Choose User
**Mappe:** `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Pages/`  
**Klasser:** `FarmerPageViewModel`, `ConsultantPageViewModel`  
**Command:** `ChooseUserCommand` (i base: `NavigationViewModelBase`)

**ConsultantPageViewModel.OnConsultantSelected() (linje 131-135):**
- **Step 1:** Sætter selected consultant (linje 133):
  - `SelectedConsultant = person;`
- **Step 2:** Opdaterer notification ViewModel (linje 36-39):
  - Hvis `NatureCheckViewModel != null`: `NatureCheckViewModel.SelectedConsultant = value;`
- **Step 3:** Future: trigger dashboard refresh (linje 134)
- **Note:** SD'en viser ikke eksplicit `UpdateDashboard()` kald - Dette matcher koden. Dashboard opdateres via data binding baseret på `SelectedConsultant` property, ikke via eksplicit metodekald.

**FarmerPageViewModel:**
- Ingen specifik implementation (bruger base funktionalitet)
- Ingen `UpdateDashboard()` metode (i modsætning til SD'en)

---

## 📊 Komplet Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ 1. UI LAYER (WinUI)                                         │
│    Brugeren åbner applikationen                             │
│    Klikker på rolle knap (Farmer/Consultant/Arla Employee) │
│    ↓ kalder                                                 │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. VIEWMODEL LAYER (WinUI.Pages)                            │
│    Klasse: LoginPageViewModel                               │
│    Command: SelectRoleCommand.Execute(roleName)              │
│    Metode: SelectRole() (linje 75)                         │
│    ↓ opretter Role objekt                                  │
│    ↓ kalder NavigationHandler                              │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. NAVIGATION HANDLER                                       │
│    Interface: INavigationHandler                             │
│    Metode: Navigate(pageType, parameter)                    │
│    ↓ navigerer til korrekt side                            │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. PAGE (NavPage)                                           │
│    Klasse: FarmerPage (arver fra NavPage)                  │
│    Metode: OnNavigatedTo() (linje 39 i NavPage.cs)        │
│    ↓ kalder ViewModel.InitializeAsync(role) (linje 54)     │
│    ↓ kalder ViewModel.AttachSideMenuToMainWindow() (linje 58)│
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. PAGE VIEWMODEL (Farmer/Consultant)                       │
│    Klasser: FarmerPageViewModel, ConsultantPageViewModel   │
│    Metode: InitializeAsync(role) (linje 156 i base)       │
│    Metode: AttachSideMenuToMainWindow() (linje 247 i base) │
│    ↓ opretter side menu control og ViewModel via DI        │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. SIDE MENU VIEWMODEL                                      │
│    Base: SideMenuViewModelBase                              │
│    Concrete: FarmerPageSideMenuUCViewModel,                 │
│              ConsultantPageSideMenuUCViewModel             │
│    Metode: InitializeAsync() (kaldes fra control's Loaded) │
│    ↓ kalder LoadAvailablePersonsAsync(role)                  │
│    ↓ kalder                                                 │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. REPOSITORY INTERFACE (Core.Abstract)                     │
│    Interface: IPersonRepository                             │
│    ↓ implementeret af                                       │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. REPOSITORY IMPLEMENTATION (Infrastructure)               │
│    Klasse: PersonRepository                                 │
│    Metode: GetPersonsByRoleAsync(role) (linje 63)          │
│    ↓ bruger EF Core                                        │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 9. EF CORE LAYER                                            │
│    Klasse: AppDbContext                                     │
│    DbSet: Persons, Roles                                    │
│    SQL: SELECT * FROM ROLES WHERE LOWER(Name) = ...       │
│         SELECT * FROM PERSONS WHERE RoleId = ...           │
└─────────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 10. DATABASE (SQL Server)                                   │
│     Tabeller: PERSONS, ROLES                                │
│     SQL: SELECT ...                                         │
│     ↓ returnerer data                                       │
└─────────────────────────────────────────────────────────────┘
                     ↑
         (Samme vej tilbage gennem alle lag)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 11. PERSON SELECTION                                        │
│     Brugeren vælger person fra dropdown                     │
│     ↓ sætter SelectedPerson property (via data binding)    │
│     ↓ property setter kalder ChooseUserCommand.Execute()   │
│     ↓ Page ViewModel opdaterer SelectedConsultant          │
│     ↓ dashboard opdateres via data binding                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 Detaljeret Metode Flow

### Eksempel: Vælg Farmer rolle og person

```csharp
// 1. UI: Brugeren klikker "Landmand" knap
SelectRoleCommand.Execute("Farmer");
//     ↑ kalder

// 2. ViewModel: LoginPageViewModel.SelectRole()
// (Faktisk kode - linje 75-117)

if (string.IsNullOrWhiteSpace(roleName)) return;  // Validation

// Opretter Role objekt (som i SD'en)
SelectedRole = new Role { Name = roleName };  // Linje 88

// Navigerer til FarmerPage (som i SD'en)
_navigationHandler.Navigate(typeof(FarmerPage), SelectedRole);  // Linje 89

// 3. Navigation Handler: NavigationHandler.Navigate()
// Navigerer til FarmerPage med Role objekt som parameter

// 4. Page: FarmerPage.xaml
// Page initialiseres med Role objekt
// NavPage.OnNavigatedTo() kaldes (linje 39 i NavPage.cs)

// 5. NavPage: NavPage.OnNavigatedTo()
// (Faktisk kode - linje 39-59 i NavPage.cs)

if (e.Parameter is Role role)  // Linje 48
{
    await ViewModel.InitializeAsync(role);  // Linje 54 - Kalder FarmerPageViewModel.InitializeAsync()
}

ViewModel?.AttachSideMenuToMainWindow();  // Linje 58 - Opretter side menu control og ViewModel

// 6. NavigationViewModelBase: AttachSideMenuToMainWindow()
// (Faktisk kode - linje 247-365 i NavigationViewModelBase.cs)

// Opretter side menu ViewModel via DI (linje 277)
resolvedVm = _sideMenuScope.ServiceProvider.GetService(SideMenuViewModelType);

// Opretter side menu control (linje 311-313)
control = App.HostInstance?.Services.GetService(SideMenuControlType) as UserControl
          ?? Activator.CreateInstance(SideMenuControlType) as UserControl;

// Sætter DataContext og tilføjer til main window (linje 360-362)
control.DataContext = resolvedVm;
sideMenuPanel.Children.Add(control);

// Side menu control's Loaded event kalder InitializeAsync() (linje 66 i FarmerPageSideMenuUC.xaml.cs)

// 7. Side Menu ViewModel: FarmerPageSideMenuUCViewModel.InitializeAsync()
// (Faktisk kode - linje 123-136)

IsLoading = true;  // Linje 125
try
{
    // Kalder base metode (som i SD'en)
    await LoadAvailablePersonsAsync(RoleName.Farmer);  // Linje 129
}
finally
{
    IsLoading = false;  // Linje 133
}

// 8. Base: SideMenuViewModelBase.LoadAvailablePersonsAsync()
// (Faktisk kode - linje 143-156)

if (string.IsNullOrWhiteSpace(role))  // Validation
{
    TrySetAvailablePersons([]);
    return;
}

// Henter personer fra repository (som i SD'en)
IEnumerable<Person> persons = await _repository.GetPersonsByRoleAsync(role);  // Linje 152

// Opdaterer collection
TrySetAvailablePersons(persons);  // Linje 155

// 9. Repository: PersonRepository.GetPersonsByRoleAsync()
// (Faktisk kode - linje 63-90)

// Normaliserer role name (linje 69)
string normalized = role.Trim().ToLowerInvariant();

// Henter Role entity fra database (linje 75-76)
Role? roleEntity = await ctx.Set<Role>()
    .FirstOrDefaultAsync(r => r.Name.ToLower() == normalized, ct);

// Henter Personer med matching RoleId (linje 81-83)
return await ctx.Set<Person>()
    .Where(p => p.RoleId == roleEntity.Id)
    .ToListAsync(ct);

// EF Core genererer SQL:
// SELECT * FROM ROLES WHERE LOWER(Name) = @normalized
// SELECT * FROM PERSONS WHERE RoleId = @roleEntityId

// Database returnerer data
// EF Core mapper til Person entities
// Repository returnerer IEnumerable<Person>

// 10. Data flyder tilbage:
//    Repository → Side Menu ViewModel → UI
//    TrySetAvailablePersons() opdaterer AvailablePersons collection (linje 155)
//    UI bindings opdateres automatisk
//    UI viser dropdown med personer

// 11. Person Selection:
//    Brugeren vælger person fra dropdown
//    UI binder direkte til SelectedPerson property (via data binding)

// 12. Side Menu ViewModel: SideMenuViewModelBase.SelectedPerson setter
// (Faktisk kode - linje 106-123)

field = value;  // Linje 111
OnPropertyChanged();  // Linje 112

if (value != null)  // Linje 113
{
    // Henter ChooseUserCommand (som i SD'en)
    RelayCommand<Person>? chooseCommand = _navigationViewModel?.ChooseUserCommand;  // Linje 116
    
    if (chooseCommand != null && chooseCommand.CanExecute(value))  // Linje 117
    {
        // Kalder command (som i SD'en)
        chooseCommand.Execute(value);  // Linje 119
    }
}

// 13. Page ViewModel: ConsultantPageViewModel.OnConsultantSelected()
// (Faktisk kode - linje 131-135)

SelectedConsultant = person;  // Linje 133

// Opdaterer notification ViewModel (hvis tilgængelig)
if (NatureCheckViewModel != null)  // Linje 36-39
{
    NatureCheckViewModel.SelectedConsultant = value;
}

// Dashboard opdateres baseret på valgt person via data binding
```

---

## 🧪 Relevante Tests

### Test 1: SelectRoleCommand_WithValidRoleName_NavigatesToCorrectPage
**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Pages/LoginPageViewModelTests.cs`  
**Linje:** 49-81

**Hvad tester den?**
- Tester at `SelectRoleCommand` navigerer til korrekt side baseret på rolle
- Verificerer at Role objekt oprettes korrekt
- Tester at navigation handler kaldes med korrekte parametre

**Hvorfor er den god?**
- Dækker hovedflowet (role selection)
- Verificerer at navigation virker korrekt
- Tester alle roller (Farmer, Consultant, Arla Employee, Administrator)

### Test 2: LoadAvailablePersonsAsync_WithValidRole_LoadsPersons
**Fil:** `tests/ArlaNatureConnect/TestWinUI/ViewModels/Abstracts/SideMenuViewModelBaseTests.cs`  
**Linje:** 100-151

**Hvad tester den?**
- Tester at `LoadAvailablePersonsAsync()` kalder repository korrekt
- Verificerer at personer bliver loaded i collection
- Tester at UI bindings opdateres korrekt

**Hvorfor er den god?**
- Dækker person loading flow
- Verificerer integration mellem ViewModel og Repository
- Tester at collection opdateres korrekt

---

## 📝 Noter

- **No Authentication:** Dette er en prototype, så der er ingen password eller email authentication
- **Role-based Access:** Rollen bestemmer hvilke funktioner og data der er synlige
- **Person Selection:** Kun Farmer og Consultant kræver person valg; Arla Employee går direkte til dashboard
- **Navigation Pattern:** Navigation handler bruges til at navigere mellem sider med parametre
- **Side Menu Pattern:** Side menu ViewModels arver fra `SideMenuViewModelBase` for at dele fælles funktionalitet
- **Command Pattern:** Commands bruges til at håndtere user interactions (SelectRoleCommand, ChooseUserCommand)

---

## 🎯 Konklusion

**Hele processen:**
1. **UI** → Bruger åbner applikationen og klikker på rolle knap
2. **LoginPageViewModel** → Opretter Role objekt og navigerer til korrekt side
3. **Navigation Handler** → Navigerer til den korrekte Page
4. **Page ViewModel** → Initialiserer side menu ViewModel
5. **Side Menu ViewModel** → Henter personer fra repository (hvis nødvendigt)
6. **Repository** → Henter personer fra database via EF Core
7. **Database** → Returnerer personer
8. **Tilbage gennem alle lag** → Personer vises i dropdown
9. **Person Selection** → Bruger vælger person, dashboard opdateres

**Vigtige principper:**
- **Separation of Concerns** - Hvert lag har sin egen ansvar
- **Dependency Inversion** - ViewModels bruger interfaces, ikke konkrete klasser
- **Command Pattern** - Commands bruges til at håndtere user interactions
- **Navigation Pattern** - Navigation handler abstraherer navigation logik
- **Inheritance** - Side menu ViewModels arver fra base klasse for at dele funktionalitet

**Forskelle fra andre use cases:**
- **Ingen Service Layer** - ViewModels kalder repositories direkte (ingen business logic)
- **Navigation Focus** - Fokus på navigation mellem sider, ikke data manipulation
- **Role-based Routing** - Rollen bestemmer hvilken side der vises

---

**Dette er hele flowet fra UI til database og tilbage igen for Login & Role Access! 🎉**
