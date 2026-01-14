## UC002 – Sequence Diagram: Create Person (v2)

This sequence diagram shows the detailed interaction flow when an Administrator creates a new Person, following Larmann's UML conventions.

**Notes:**
- All method calls use PascalCase (C# convention).
- All calls have return arrows (including void methods).
- Activation bars show object lifetime using automatic activation/deactivation (+/-).
- ViewModel calls repositories directly (no service layer in UC002).
- Address is created and saved before Person (due to foreign key relationship).

```mermaid
sequenceDiagram
    title UC002 – Sequence Diagram: Create Person

    actor Admin as Administrator
    participant ViewModel as CRUDPersonUCViewModel
    participant AddressRepo as IAddressRepository
    participant PersonRepo as IPersonRepository
    participant StatusService as IStatusInfoServices

    Admin ->>+ ViewModel: OnAddAsync()
    
    alt CanAdd() returns false
        ViewModel -->> Admin: void
    else CanAdd() returns true
        ViewModel ->>+ StatusService: BeginLoadingOrSaving()
        StatusService -->>- ViewModel: IDisposable
        
        ViewModel ->>+ ViewModel: OnAddFormAsync()
        ViewModel ->>+ ViewModel: OnAddAddressFormAsync()
        ViewModel -->>- ViewModel: Address
        
        ViewModel ->>+ AddressRepo: AddAsync(address)
        AddressRepo -->>- ViewModel: Address
        
        ViewModel ->>+ PersonRepo: AddAsync(person)
        PersonRepo -->>- ViewModel: Person
        
        ViewModel ->>+ ViewModel: GetAllAsync()
        ViewModel -->>- ViewModel: void
        
        ViewModel ->>+ ViewModel: OnResetFormAsync()
        ViewModel -->>- ViewModel: void
        
        ViewModel ->> StatusService: Dispose()
        ViewModel -->>- Admin: void
    end
```

---

## UC002 – Sequence Diagram: Update Person (v2)

This sequence diagram shows the detailed interaction flow when an Administrator updates an existing Person, following Larmann's UML conventions.

**Notes:**
- All method calls use PascalCase (C# convention).
- All calls have return arrows (including void methods).
- Activation bars show object lifetime using automatic activation/deactivation (+/-).

```mermaid
sequenceDiagram
    title UC002 – Sequence Diagram: Update Person

    actor Admin as Administrator
    participant ViewModel as CRUDPersonUCViewModel
    participant AddressRepo as IAddressRepository
    participant PersonRepo as IPersonRepository
    participant StatusService as IStatusInfoServices

    Admin ->>+ ViewModel: OnSaveAsync()
    
    alt CanSave() returns false
        ViewModel -->> Admin: void
    else CanSave() returns true
        ViewModel ->>+ StatusService: BeginLoadingOrSaving()
        StatusService -->>- ViewModel: IDisposable
        
        ViewModel ->>+ ViewModel: OnSaveFormAsync()
        
        alt Address needs update
            ViewModel ->>+ AddressRepo: UpdateAsync(address)
            AddressRepo -->>- ViewModel: void
        end
        
        ViewModel ->>+ PersonRepo: UpdateAsync(person)
        PersonRepo -->>- ViewModel: void
        
        ViewModel ->>+ ViewModel: GetAllAsync()
        ViewModel -->>- ViewModel: void
        
        ViewModel ->>+ ViewModel: OnResetFormAsync()
        ViewModel -->>- ViewModel: void
        
        ViewModel ->> StatusService: Dispose()
        ViewModel -->>- Admin: void
    end
```

---

## UC002 – Sequence Diagram: Delete Person (v2)

This sequence diagram shows the detailed interaction flow when an Administrator deletes a Person, following Larmann's UML conventions.

**Notes:**
- All method calls use PascalCase (C# convention).
- All calls have return arrows (including void methods).
- Activation bars show object lifetime using automatic activation/deactivation (+/-).

```mermaid
sequenceDiagram
    title UC002 – Sequence Diagram: Delete Person

    actor Admin as Administrator
    participant ViewModel as CRUDPersonUCViewModel
    participant PersonRepo as IPersonRepository
    participant StatusService as IStatusInfoServices

    Admin ->>+ ViewModel: OnDeleteAsync()
    
    alt CanDelete() returns false
        ViewModel -->> Admin: void
    else CanDelete() returns true
        ViewModel ->>+ StatusService: BeginLoadingOrSaving()
        StatusService -->>- ViewModel: IDisposable
        
        ViewModel ->>+ PersonRepo: DeleteAsync(personId)
        PersonRepo -->>- ViewModel: void
        
        ViewModel ->>+ ViewModel: GetAllAsync()
        ViewModel -->>- ViewModel: void
        
        ViewModel ->> StatusService: Dispose()
        ViewModel -->>- Admin: void
    end
```

---

## UC002 – Sequence Diagram: Load Person List (v2)

This sequence diagram shows the detailed interaction flow when an Administrator loads the list of persons, following Larmann's UML conventions.

**Notes:**
- All method calls use PascalCase (C# convention).
- All calls have return arrows (including void methods).
- Activation bars show object lifetime using automatic activation/deactivation (+/-).
- This is a read-only operation - no data is modified.

```mermaid
sequenceDiagram
    title UC002 – Sequence Diagram: Load Person List

    actor Admin as Administrator
    participant ViewModel as CRUDPersonUCViewModel
    participant PersonRepo as IPersonRepository
    participant StatusService as IStatusInfoServices

    Admin ->>+ ViewModel: InitializeAsync()
    
    ViewModel ->>+ StatusService: BeginLoadingOrSaving()
    StatusService -->>- ViewModel: IDisposable
    
    ViewModel ->>+ PersonRepo: GetAllAsync()
    PersonRepo -->>- ViewModel: List~Person~
    
    ViewModel ->>+ ViewModel: UpdatePersonList(persons)
    ViewModel -->>- ViewModel: void
    
    ViewModel ->> StatusService: Dispose()
    ViewModel -->>- Admin: void
```
