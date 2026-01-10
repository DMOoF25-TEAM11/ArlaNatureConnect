/*
    File: UC002B.4-DML.sql
    Purpose: Entity Framework Data Manipulation for Use Case UC002B.4 - Create Farm (Inline)
    Safety: This document shows how Entity Framework Core handles farm creation.
    
    Use Case: UC002B.4 - Create Farm (Inline)
    Description: Shows how EF Core repositories and services create farms, addresses, and persons.
    
    Dependencies: Requires entities from UC001 and UC002
    
    Note: This is documentation showing EF Core approach. Actual data operations use repository methods.
    
    created: 2025-01-XX
    change log:
        - 2025-01-XX: Converted from standard SQL DML to Entity Framework Core repository/service calls
*/

-- ================================================================================================
-- ENTITY FRAMEWORK CORE DATA CREATION
-- ================================================================================================
-- The following shows how EF Core repositories and services create farms.
-- ================================================================================================

/*
    ================================================================================================
    SERVICE LAYER: NatureCheckCaseService.SaveFarmAsync()
    ================================================================================================
    
    // From NatureCheckCaseService.cs
    
    public async Task<Farm> SaveFarmAsync(
        FarmRegistrationRequest request, 
        CancellationToken cancellationToken = default)
    {
        ValidateFarmRegistration(request);
        
        // Check if owner email exists and reuse if Farmer role
        Farm? existingFarm = await _farmRepository.GetByCvrAsync(request.Cvr, cancellationToken);
        if (existingFarm != null && request.FarmId.HasValue)
        {
            // Update existing farm
            // ... (update logic)
            return existingFarm;
        }
        
        // Create new farm
        // 1. Create address
        Address farmAddress = new()
        {
            Id = Guid.NewGuid(),
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country
        };
        await _addressRepository.AddAsync(farmAddress, cancellationToken);
        
        // 2. Ensure Farmer role exists
        Role farmerRole = await EnsureRoleAsync(RoleName.Farmer.ToString(), cancellationToken);
        
        // 3. Check if owner exists by email
        Person? existingOwner = await _personRepository.GetAllAsync(cancellationToken)
            .FirstOrDefaultAsync(p => p.Email == request.OwnerEmail);
        
        Person farmer;
        if (existingOwner != null && existingOwner.RoleId == farmerRole.Id)
        {
            // Reuse existing farmer
            farmer = existingOwner;
        }
        else
        {
            // Create new farmer
            farmer = new Person()
            {
                Id = Guid.NewGuid(),
                RoleId = farmerRole.Id,
                AddressId = farmAddress.Id,
                FirstName = request.OwnerFirstName,
                LastName = request.OwnerLastName,
                Email = request.OwnerEmail,
                IsActive = request.OwnerIsActive
            };
            await _personRepository.AddAsync(farmer, cancellationToken);
        }
        
        // 4. Create farm
        Farm farm = new()
        {
            Id = Guid.NewGuid(),
            Name = request.FarmName,
            CVR = request.Cvr,
            OwnerId = farmer.Id,
            AddressId = farmAddress.Id
        };
        await _farmRepository.AddAsync(farm, cancellationToken);
        
        return farm;
    }
*/

/*
    ================================================================================================
    REPOSITORY LAYER: AddAsync() Methods
    ================================================================================================
    
    // From Repository.cs (base class)
    
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        ctx.Set<TEntity>().Add(entity);
        await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }
    
    EF Core automatically:
    - Generates INSERT SQL statements
    - Handles foreign key relationships
    - Returns entity with database-generated values
*/

/*
    ================================================================================================
    CVR UNIQUENESS VALIDATION
    ================================================================================================
    
    // From FarmRepository.cs
    
    public async Task<Farm?> GetByCvrAsync(string cvr, CancellationToken cancellationToken = default)
    {
        await using AppDbContext ctx = _factory.CreateDbContext();
        return await ctx.Set<Farm>()
            .FirstOrDefaultAsync(f => f.CVR == cvr.Trim(), cancellationToken);
    }
    
    If duplicate CVR is inserted, EF Core throws DbUpdateException.
    Service layer catches exception and provides user-friendly error message.
*/

/*
    ================================================================================================
    KEY DIFFERENCES FROM STANDARD SQL DML
    ================================================================================================
    
    1. No Stored Procedures: Business logic in service layer, not database
    2. No Triggers: Validation handled in service layer
    3. Entity Creation: Create C# objects, EF Core handles SQL INSERT
    4. Multiple Entities: Create Address, Person, Farm separately - EF Core handles relationships
    5. Reuse Logic: Check for existing owner in service layer, not database
    6. Repository Pattern: Data access through repository interfaces, not direct SQL
    7. Change Tracking: EF Core tracks changes and generates SQL automatically
*/

PRINT 'UC002B.4 Entity Framework Core data creation documented.';
GO