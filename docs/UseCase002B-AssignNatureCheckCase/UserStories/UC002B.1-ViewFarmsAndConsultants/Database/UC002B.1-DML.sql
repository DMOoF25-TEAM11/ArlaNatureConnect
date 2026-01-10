/*
    File: UC002B.1-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.1 - View Farms and Consultants for Assignment
    Safety: This document shows how Entity Framework Core handles data loading for viewing farms and consultants.
    
    Use Case: UC002B.1 - View Farms and Consultants for Assignment
    Description: Shows how EF Core repositories and services load data for displaying farms and consultants.
    No data modification occurs in UC002B.1 - it is a read-only operation.
    
    Dependencies: Requires entities from UC001 and UC002 (Farm, Person, Role, Address, NatureCheckCase)
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA LOADING (Read-Only Operations)
-- ================================================================================================
-- UC002B.1 is a read-only use case. It loads data for display but does not modify data.
-- The following shows how EF Core repositories and services load the required data.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.LoadAssignmentContextAsync()
    ================================================================================================
    
    This is the main entry point for UC002B.1. It orchestrates loading all required data.
*/

/*
    // From NatureCheckCaseService.cs
    
    public async Task<NatureCheckCaseAssignmentContext> LoadAssignmentContextAsync(
        CancellationToken cancellationToken = default)
    {
        // Materialize all results to avoid concurrency issues with shared DbContext
        List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false)).ToList();
        
        List<Person> allPersons = (await _personRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false)).ToList();
        
        List<Person> consultants = (await _personRepository
            .GetPersonsByRoleAsync(RoleName.Consultant.ToString(), cancellationToken)
            .ConfigureAwait(false)).ToList();
        
        List<Address> addresses = (await _addressRepository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false)).ToList();
        
        IReadOnlyList<NatureCheckCase> activeCases = await _natureCheckCaseRepository
            .GetActiveCasesAsync(cancellationToken)
            .ConfigureAwait(false);

        // Build dictionaries for efficient lookups
        Dictionary<Guid, Person> personsById = allPersons.ToDictionary(p => p.Id, p => p);
        Dictionary<Guid, Address> addressesById = addresses.ToDictionary(a => a.Id, a => a);
        
        HashSet<Guid> activeFarmIds = activeCases
            .Select(c => c.FarmId)
            .ToHashSet();

        Dictionary<Guid, NatureCheckCase> activeCasesByFarm = activeCases
            .GroupBy(c => c.FarmId)
            .Select(g => g.OrderByDescending(c => c.AssignedAt ?? c.CreatedAt).First())
            .ToDictionary(c => c.FarmId, c => c);

        // Create DTOs for UI display
        List<FarmAssignmentOverviewDto> overview = farms
            .Select(f => CreateFarmOverview(f, personsById, addressesById, 
                activeFarmIds.Contains(f.Id), activeCasesByFarm))
            .OrderBy(f => f.FarmName)
            .ToList();

        List<Person> sortedConsultants = consultants
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToList();

        return new NatureCheckCaseAssignmentContext(overview, sortedConsultants);
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: Repository Methods Used
    ================================================================================================
*/

/*
    1. IFarmRepository.GetAllAsync()
    
    // From Repository.cs (base class)
    public async Task<IEnumerable<Farm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<Farm>()
            .AsNoTracking()  // Read-only, no change tracking needed
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    
    EF Core automatically includes related entities via AutoInclude configuration:
    - Farm.Owner (Person) - loaded automatically
    - Farm.Address (Address) - loaded automatically
*/

/*
    2. IPersonRepository.GetAllAsync()
    
    // From Repository.cs (base class)
    public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<Person>()
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    
    EF Core automatically includes:
    - Person.Role - loaded automatically
    - Person.Address - loaded automatically
*/

/*
    3. IPersonRepository.GetPersonsByRoleAsync("Consultant")
    
    // From PersonRepository.cs
    public async Task<IEnumerable<Person>> GetPersonsByRoleAsync(
        string role, CancellationToken ct = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        
        // First resolve role entity by name
        Role? roleEntity = await ctx.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == role.Trim().ToLowerInvariant(), ct)
            .ConfigureAwait(false);
        
        if (roleEntity == null)
            return [];
        
        // Then filter persons by RoleId
        return await ctx.Set<Person>()
            .Where(p => p.RoleId == roleEntity.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
    
    EF Core automatically includes:
    - Person.Role - loaded automatically
    - Person.Address - loaded automatically
*/

/*
    4. IAddressRepository.GetAllAsync()
    
    // From Repository.cs (base class)
    public async Task<IEnumerable<Address>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<Address>()
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
*/

/*
    5. INatureCheckCaseRepository.GetActiveCasesAsync()
    
    // From NatureCheckCaseRepository.cs
    public async Task<IReadOnlyList<NatureCheckCase>> GetActiveCasesAsync(
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        
        // Load all cases
        List<NatureCheckCase> allCases = await ctx.NatureCheckCases
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        
        // Filter for active statuses (Assigned or InProgress)
        // EF Core converts Status from string to enum via ValueConverter
        List<NatureCheckCase> activeCases = allCases
            .Where(c => c.Status == NatureCheckCaseStatus.Assigned ||
                       c.Status == NatureCheckCaseStatus.InProgress)
            .ToList();
        
        return activeCases;
    }
*/

/*
    ================================================================================================
    DTO CREATION: FarmAssignmentOverviewDto
    ================================================================================================
    
    // From NatureCheckCaseService.CreateFarmOverview()
    
    private static FarmAssignmentOverviewDto CreateFarmOverview(
        Farm farm,
        Dictionary<Guid, Person> personsById,
        Dictionary<Guid, Address> addressesById,
        bool hasActiveCase,
        Dictionary<Guid, NatureCheckCase> activeCasesByFarm)
    {
        Person? owner = null;
        if (farm.OwnerId != Guid.Empty)
        {
            personsById.TryGetValue(farm.OwnerId, out owner);
        }
        
        Address? address = null;
        if (farm.AddressId != Guid.Empty)
        {
            addressesById.TryGetValue(farm.AddressId, out address);
        }
        
        NatureCheckCase? activeCase = null;
        if (hasActiveCase)
        {
            activeCasesByFarm.TryGetValue(farm.Id, out activeCase);
        }
        
        Person? consultant = null;
        if (activeCase != null && activeCase.ConsultantId != Guid.Empty)
        {
            personsById.TryGetValue(activeCase.ConsultantId, out consultant);
        }
        
        return new FarmAssignmentOverviewDto
        {
            FarmId = farm.Id,
            FarmName = farm.Name,
            Cvr = farm.CVR,
            OwnerFirstName = owner?.FirstName ?? string.Empty,
            OwnerLastName = owner?.LastName ?? string.Empty,
            OwnerEmail = owner?.Email ?? string.Empty,
            Street = address?.Street ?? string.Empty,
            City = address?.City ?? string.Empty,
            PostalCode = address?.PostalCode ?? string.Empty,
            Country = address?.Country ?? string.Empty,
            HasActiveCase = hasActiveCase,
            AssignedConsultantId = consultant?.Id,
            AssignedConsultantFirstName = consultant?.FirstName,
            AssignedConsultantLastName = consultant?.LastName,
            Priority = activeCase?.Priority,
            // Computed properties
            OwnerName = $"{owner?.FirstName} {owner?.LastName}".Trim(),
            AddressLine = $"{address?.PostalCode} {address?.City}, {address?.Street}".Trim(),
            StatusLabel = hasActiveCase ? "Tilføjet" : "Ikke tilføjet",
            AssignedConsultantName = consultant != null 
                ? $"{consultant.FirstName} {consultant.LastName}".Trim() 
                : string.Empty
        };
    }
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No INSERT/UPDATE/DELETE: UC002B.1 is read-only
    2. Repository Pattern: Data access through repository interfaces, not direct SQL
    3. LINQ Queries: EF Core translates LINQ to SQL automatically
    4. Change Tracking: AsNoTracking() used for read-only operations (better performance)
    5. Automatic Includes: Navigation properties loaded automatically via AutoInclude
    6. In-Memory Processing: Some filtering/grouping done in memory after materialization
    7. DTOs: Data transformed to DTOs for UI consumption, not raw entities
*/

PRINT 'UC002B.1 Entity Framework Core data loading documented.';
PRINT 'Note: This is a read-only use case - no data modification occurs.';
GO
