using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;

namespace ArlaNatureConnect.Core.Services;

// Purpose: Implements UC002B orchestration – loading farms/consultants, assigning cases and handling inline farm maintenance.
// Notes: Keeps WinUI lean by validating inputs, preventing duplicates and coordinating repository persistence with clear errors.
public class NatureCheckCaseService : INatureCheckCaseService
{
    #region Fields
    private readonly IFarmRepository _farmRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly INatureCheckCaseRepository _natureCheckCaseRepository;
    private readonly IRoleRepository _roleRepository;
    #endregion

    #region Commands
    /// <summary>
    /// Initializes a new instance of the NatureCheckCaseService class with the specified repository dependencies.
    /// </summary>
    /// <param name="farmRepository">The repository used to access farm data. Cannot be null.</param>
    /// <param name="personRepository">The repository used to access person data. Cannot be null.</param>
    /// <param name="addressRepository">The repository used to access address data. Cannot be null.</param>
    /// <param name="natureCheckCaseRepository">The repository used to access nature check case data. Cannot be null.</param>
    /// <param name="roleRepository">The repository used to access role data. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the repository parameters is null.</exception>
    public NatureCheckCaseService(
        IFarmRepository farmRepository,
        IPersonRepository personRepository,
        IAddressRepository addressRepository,
        INatureCheckCaseRepository natureCheckCaseRepository,
        IRoleRepository roleRepository)
    {
        _farmRepository = farmRepository ?? throw new ArgumentNullException(nameof(farmRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _natureCheckCaseRepository = natureCheckCaseRepository ?? throw new ArgumentNullException(nameof(natureCheckCaseRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    /// <summary>
    /// Asynchronously loads the context required for assigning NatureCheck cases, including farm overviews and
    /// available consultants.
    /// </summary>
    /// <remarks>The returned context includes only active farms and consultants. The operation may involve
    /// multiple data sources and can be cancelled via the provided token.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a NatureCheckCaseAssignmentContext
    /// with farm assignment overviews and a sorted list of consultants.</returns>
    public async Task<NatureCheckCaseAssignmentContext> LoadAssignmentContextAsync(CancellationToken cancellationToken = default)
    {
        // Materialize all results to avoid concurrency issues with shared DbContext
        List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
        List<Person> allPersons = (await _personRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
        List<Person> consultants = (await _personRepository.GetPersonsByRoleAsync(RoleName.Consultant.ToString(), cancellationToken).ConfigureAwait(false)).ToList();
        List<Address> addresses = (await _addressRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
        IReadOnlyList<NatureCheckCase> activeCases = await _natureCheckCaseRepository.GetActiveCasesAsync(cancellationToken).ConfigureAwait(false);

        Dictionary<Guid, Person> personsById = allPersons.ToDictionary(p => p.Id, p => p);
        Dictionary<Guid, Address> addressesById = addresses.ToDictionary(a => a.Id, a => a);
        // GetActiveCasesAsync already filters for Assigned and InProgress, so no need to filter again
        HashSet<Guid> activeFarmIds = activeCases
            .Select(c => c.FarmId)
            .ToHashSet();

        Dictionary<Guid, NatureCheckCase> activeCasesByFarm = activeCases
            .GroupBy(c => c.FarmId)
            .Select(g => g.OrderByDescending(c => c.AssignedAt ?? c.CreatedAt).First())
            .ToDictionary(c => c.FarmId, c => c);

        List<FarmAssignmentOverviewDto> overview = farms
            .Select(f => CreateFarmOverview(f, personsById, addressesById, activeFarmIds.Contains(f.Id), activeCasesByFarm))
            .OrderBy(f => f.FarmName)
            .ToList();

        List<Person> sortedConsultants = consultants
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToList();

        return new NatureCheckCaseAssignmentContext(overview, sortedConsultants);
    }

    /// <summary>
    /// Assigns a new Nature Check case to the specified consultant for the given farm, based on the provided assignment
    /// request.
    /// </summary>
    /// <remarks>The method validates the existence of the farm and consultant, ensures the consultant has the
    /// appropriate role, and checks for active cases on the farm before assignment. If the assignment is successful,
    /// the new case is persisted and returned.</remarks>
    /// <param name="request">The assignment request containing details about the farm, consultant, assignment initiator, and case options.
    /// Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A NatureCheckCase instance representing the newly assigned case.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified farm or consultant does not exist, if the consultant does not have the required role, or
    /// if the farm already has an active case and duplicate active cases are not allowed.</exception>
    public async Task<NatureCheckCase> AssignCaseAsync(NatureCheckCaseAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Farm? farm = await _farmRepository.GetByIdAsync(request.FarmId, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Gården findes ikke længere. Opdater listen og prøv igen.");
        Person? consultant = await _personRepository.GetByIdAsync(request.ConsultantId, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Den valgte konsulent findes ikke længere.");
        ArlaNatureConnect.Domain.Entities.Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(consultantRole?.Name, RoleName.Consultant.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Den valgte person har ikke konsulent-rollen.");
        }

        // Validate AssignedByPersonId - must be a valid GUID
        if (request.AssignedByPersonId == Guid.Empty)
        {
            throw new InvalidOperationException("Du mangler at vælge en gyldig Arla medarbejder.");
        }

        bool hasActiveCase = await _natureCheckCaseRepository.FarmHasActiveCaseAsync(farm.Id, cancellationToken).ConfigureAwait(false);
        if (hasActiveCase && !request.AllowDuplicateActiveCase)
        {
            throw new InvalidOperationException("Gården har allerede en aktiv Natur Check opgave.");
        }

        NatureCheckCase entity = new()
        {
            Id = Guid.NewGuid(),
            FarmId = farm.Id,
            ConsultantId = consultant.Id,
            AssignedByPersonId = request.AssignedByPersonId,
            Status = NatureCheckCaseStatus.Assigned,
            Notes = request.Notes,
            Priority = request.Priority,
            CreatedAt = DateTimeOffset.UtcNow,
            AssignedAt = DateTimeOffset.UtcNow
        };

        await _natureCheckCaseRepository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<NatureCheckCase> UpdateCaseAsync(Guid farmId, NatureCheckCaseAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Find the active case for the farm
        NatureCheckCase? existingCase = await _natureCheckCaseRepository.GetActiveCaseForFarmAsync(farmId, cancellationToken).ConfigureAwait(false);
        if (existingCase == null)
        {
            throw new InvalidOperationException("Der findes ingen aktiv Natur Check opgave for denne gård.");
        }

        // Validate consultant
        Person? consultant = await _personRepository.GetByIdAsync(request.ConsultantId, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Den valgte konsulent findes ikke længere.");
        ArlaNatureConnect.Domain.Entities.Role? consultantRole = await _roleRepository.GetByIdAsync(consultant.RoleId, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(consultantRole?.Name, RoleName.Consultant.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Den valgte person har ikke konsulent-rollen.");
        }

        // Update the case
        existingCase.ConsultantId = consultant.Id;
        existingCase.Priority = request.Priority;
        existingCase.Notes = request.Notes;
        existingCase.AssignedAt = DateTimeOffset.UtcNow; // Update assignment time

        await _natureCheckCaseRepository.UpdateAsync(existingCase, cancellationToken).ConfigureAwait(false);
        return existingCase;
    }

    /// <summary>
    /// Creates a new farm or updates an existing farm asynchronously based on the provided registration request.
    /// </summary>
    /// <remarks>If <paramref name="request"/> contains a farm ID, the method updates the corresponding farm
    /// and its related address and owner information. If no farm ID is provided, a new farm, address, and owner are
    /// created. The operation is performed asynchronously and may involve multiple data repositories.</remarks>
    /// <param name="request">The registration details for the farm to create or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created or updated farm entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified farm to update does not exist.</exception>
    public async Task<Farm> SaveFarmAsync(FarmRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateFarmRegistration(request);

        if (request.FarmId.HasValue)
        {
            Farm? existingFarm = await _farmRepository.GetByIdAsync(request.FarmId.Value, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Kunne ikke finde gården til redigering.");
            if (existingFarm.AddressId != Guid.Empty)
            {
                Address? farmAddress = await _addressRepository.GetByIdAsync(existingFarm.AddressId, cancellationToken).ConfigureAwait(false);
                if (farmAddress != null)
                {
                    farmAddress.Street = request.Street;
                    farmAddress.City = request.City;
                    farmAddress.PostalCode = request.PostalCode;
                    farmAddress.Country = request.Country;
                    await _addressRepository.UpdateAsync(farmAddress, cancellationToken).ConfigureAwait(false);
                }
            }

            if (existingFarm.OwnerId != Guid.Empty)
            {
                Person? farmer = await _personRepository.GetByIdAsync(existingFarm.OwnerId, cancellationToken).ConfigureAwait(false);
                if (farmer != null)
                {
                    farmer.FirstName = request.OwnerFirstName;
                    farmer.LastName = request.OwnerLastName;
                    farmer.Email = request.OwnerEmail;
                    await _personRepository.UpdateAsync(farmer, cancellationToken).ConfigureAwait(false);
                }
            }

            existingFarm.Name = request.FarmName;
            existingFarm.CVR = request.Cvr;
            await _farmRepository.UpdateAsync(existingFarm, cancellationToken).ConfigureAwait(false);

            return existingFarm;
        }
        else
        {
            // Validate that CVR doesn't already exist (if provided)
            if (!string.IsNullOrWhiteSpace(request.Cvr))
            {
                Farm? existingFarm = await _farmRepository.GetByCvrAsync(request.Cvr, cancellationToken).ConfigureAwait(false);
                if (existingFarm != null)
                {
                    throw new InvalidOperationException($"En gård med CVR-nummer '{request.Cvr}' findes allerede i systemet. Vælg et andet CVR-nummer.");
                }
            }

            // Check if person with this email already exists
            Person? existingPerson = await _personRepository.GetByEmailAsync(request.OwnerEmail, cancellationToken).ConfigureAwait(false);
            Person farmer;
            ArlaNatureConnect.Domain.Entities.Role farmerRole = await EnsureRoleAsync(RoleName.Farmer.ToString(), cancellationToken).ConfigureAwait(false);

            if (existingPerson != null)
            {
                // Person exists - verify they have Farmer role
                Role? existingRole = await _roleRepository.GetByIdAsync(existingPerson.RoleId, cancellationToken).ConfigureAwait(false);
                if (existingRole == null || existingRole.Name != RoleName.Farmer.ToString())
                {
                    throw new InvalidOperationException($"En person med e-mail '{request.OwnerEmail}' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen.");
                }
                
                // Use existing person - they can have multiple farms
                farmer = existingPerson;
            }
            else
            {
                // Create new person address (if provided)
                Guid? personAddressId = null;
                if (!string.IsNullOrWhiteSpace(request.Street) || !string.IsNullOrWhiteSpace(request.City))
                {
                    Address personAddress = new()
                    {
                        Id = Guid.NewGuid(),
                        Street = request.Street,
                        City = request.City,
                        PostalCode = request.PostalCode,
                        Country = request.Country
                    };
                    await _addressRepository.AddAsync(personAddress, cancellationToken).ConfigureAwait(false);
                    personAddressId = personAddress.Id;
                }

                // Create new person
                farmer = new()
                {
                    Id = Guid.NewGuid(),
                    RoleId = farmerRole.Id,
                    AddressId = personAddressId ?? Guid.Empty,
                    FirstName = request.OwnerFirstName,
                    LastName = request.OwnerLastName,
                    Email = request.OwnerEmail,
                    IsActive = request.OwnerIsActive
                };
                await _personRepository.AddAsync(farmer, cancellationToken).ConfigureAwait(false);
            }

            // Create farm address
            Address farmAddress = new()
            {
                Id = Guid.NewGuid(),
                Street = request.Street,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country
            };
            await _addressRepository.AddAsync(farmAddress, cancellationToken).ConfigureAwait(false);

            // Create new farm linked to the person (existing or new)
            Farm farm = new()
            {
                Id = Guid.NewGuid(),
                Name = request.FarmName,
                CVR = request.Cvr,
                OwnerId = farmer.Id,
                AddressId = farmAddress.Id
            };
            await _farmRepository.AddAsync(farm, cancellationToken).ConfigureAwait(false);

            return farm;
        }
    }

    public async Task DeleteFarmAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        Farm? farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken).ConfigureAwait(false);
        if (farm == null)
        {
            return;
        }

        bool hasActiveCase = await _natureCheckCaseRepository.FarmHasActiveCaseAsync(farmId, cancellationToken).ConfigureAwait(false);
        if (hasActiveCase)
        {
            throw new InvalidOperationException("Gården har aktive Natur Check opgaver og kan ikke slettes.");
        }

        await _farmRepository.DeleteAsync(farmId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ConsultantNotificationDto>> GetNotificationsForConsultantAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NatureCheckCase> assignedCases = await _natureCheckCaseRepository.GetAssignedCasesForConsultantAsync(consultantId, cancellationToken).ConfigureAwait(false);

        if (assignedCases.Count == 0)
        {
            return Array.Empty<ConsultantNotificationDto>();
        }

        // Get all farm IDs
        HashSet<Guid> farmIds = assignedCases.Select(c => c.FarmId).ToHashSet();

        // Load all farms in one query
        List<Farm> farms = (await _farmRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)).Where(f => farmIds.Contains(f.Id)).ToList();

        Dictionary<Guid, Farm> farmsById = farms.ToDictionary(f => f.Id, f => f);

        List<ConsultantNotificationDto> notifications = new();
        foreach (NatureCheckCase caseEntity in assignedCases)
        {
            if (!farmsById.TryGetValue(caseEntity.FarmId, out Farm? farm))
            {
                continue; // Skip if farm not found
            }

            notifications.Add(new ConsultantNotificationDto
            {
                CaseId = caseEntity.Id,
                FarmId = caseEntity.FarmId,
                FarmName = farm.Name,
                AssignedAt = caseEntity.AssignedAt ?? caseEntity.CreatedAt,
                Priority = caseEntity.Priority,
                Notes = caseEntity.Notes
            });
        }

        return notifications.OrderByDescending(n => n.AssignedAt).ToList();
    }
    #endregion

    #region Helpers
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

        string? consultantFirstName = null;
        string? consultantLastName = null;
        Guid? consultantId = null;
        string? priority = null;
        string? notes = null;

        if (hasActiveCase && activeCasesByFarm.TryGetValue(farm.Id, out NatureCheckCase? activeCase))
        {
            consultantId = activeCase.ConsultantId;
            priority = activeCase.Priority;
            notes = activeCase.Notes;

            if (personsById.TryGetValue(activeCase.ConsultantId, out Person? consultant))
            {
                consultantFirstName = consultant.FirstName;
                consultantLastName = consultant.LastName;
            }
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
            AssignedConsultantFirstName = consultantFirstName,
            AssignedConsultantLastName = consultantLastName,
            AssignedConsultantId = consultantId,
            Priority = priority,
            Notes = notes
        };
    }

    private static void ValidateFarmRegistration(FarmRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FarmName))
        {
            throw new ArgumentException("Gårdens navn skal udfyldes.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Cvr))
        {
            throw new ArgumentException("CVR nummer skal udfyldes.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OwnerFirstName) || string.IsNullOrWhiteSpace(request.OwnerLastName))
        {
            throw new ArgumentException("Landmandens navn skal udfyldes.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OwnerEmail))
        {
            throw new ArgumentException("Landmandens e-mail skal udfyldes.", nameof(request));
        }
    }

    private async Task<ArlaNatureConnect.Domain.Entities.Role> EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        ArlaNatureConnect.Domain.Entities.Role? role = await _roleRepository.GetByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
        return role ?? throw new InvalidOperationException($"Rollen '{roleName}' findes ikke i databasen.");
    }
    #endregion
}

