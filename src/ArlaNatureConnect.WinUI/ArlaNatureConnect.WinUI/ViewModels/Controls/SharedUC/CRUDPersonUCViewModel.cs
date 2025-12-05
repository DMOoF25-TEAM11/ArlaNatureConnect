using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

/// <summary>
/// View-model that provides CRUD operations and UI bindings for <see cref="Person"/> entities.
/// </summary>
/// <remarks>
/// Why we have this class:
/// - Encapsulates UI state and behavior for creating, reading, updating and deleting persons.
/// - Keeps UI logic separate from repository/data-access concerns (uses <see cref="IPersonRepository"/>).
/// How to use:
/// - Bind the view to properties on this view-model (for example, `FirstName`, `LastName`, `Email`).
/// - Call the exposed commands and rely on the view-model to update the bound collection `Persons`.
/// - The view-model handles conversion between form fields and the <see cref="Person"/> entity during save/add.
///
/// Inheriting XML documentation with <c>&lt;inheritdoc/&gt;</c>:
/// <para>
/// The <c>&lt;inheritdoc/&gt;</c> tag lets a member inherit documentation from its base declaration. Use it
/// on overrides or interface implementations when the behavior is the same as the base member to avoid
/// duplicating documentation and keep docs consistent.
/// </para>
/// <para>
/// Why use it:
/// - Reduces duplication across derived members.
/// - Keeps documentation synchronized with the original contract.
/// - Makes intent explicit when a derived member does not change the base behavior.
/// </para>
/// <example>
/// Example: inherit documentation for an overridden method
/// <code language="csharp">
/// // base class
/// public abstract class FooBase
/// {
///     /// <summary>Performs an important operation.</summary>
///     public abstract void DoWork();
/// }
///
/// // derived class uses &lt;inheritdoc/&gt; to reuse the base summary
/// public class FooDerived : FooBase
/// {
///     /// &lt;inheritdoc/&gt;
///     public override void DoWork()
///     {
///         // implementation here
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public partial class CRUDPersonUCViewModel
    : CRUDViewModelBase<IPersonRepository, Person>
{
    #region constants
    public const string LABEL_FIRSTNAME = "Fornavn";
    public const string LABEL_LASTNAME = "Efternavn";
    public const string LABEL_EMAIL = "Email";
    public const string LABEL_ISACTIVE = "Aktiv";
    public const string LABEL_ROLE = "Rolle";
    public const string LABEL_ADDRESS_POSTALCODE = "Postnummer";
    public const string LABEL_ADDRESS_CITY = "By";
    public const string LABEL_ADDRESS_STREET = "Vejnavn og nummer";
    public const string LABEL_ADDRESS_COUNTRY = "Land";
    public const string LABEL_ADDRESS = "Adresse";
    public const string LABEL_FARMS = "Antal Gårde";

    #endregion
    #region Fields
    // Repository for data access
    private readonly IRoleRepository? _roleRepository;
    private readonly IAddressRepository? _addressRepository;

    // Sorting state
    private string? _lastSortProp;
    private bool _lastSortDesc;
    #endregion
    #region Properties
    // Instance properties for XAML binding to label texts
    public static string LabelFirstName => LABEL_FIRSTNAME;
    public static string LabelLastName => LABEL_LASTNAME;
    public static string LabelEmail => LABEL_EMAIL;
    public static string LabelIsActive => LABEL_ISACTIVE;
    public static string LabelRole => LABEL_ROLE;
    public static string LabelAddressPostalCode => LABEL_ADDRESS_POSTALCODE;
    public static string LabelAddressCity => LABEL_ADDRESS_CITY;
    public static string LabelAddressStreet => LABEL_ADDRESS_STREET;
    public static string LabelAddressCountry => LABEL_ADDRESS_COUNTRY;
    public static string LabelAddress => LABEL_ADDRESS;
    public static string LabelFarms => LABEL_FARMS;

    /// <summary>
    /// Indicates whether the address-related fields have been changed compared to the selected entity.
    /// Used to enable/disable the save functionality.
    /// </summary>
    public bool IsAddressEntityChanged
    {
        get
        {
            if (SelectedItem?.Address.PostalCode != AddressPostalCode) return true;
            if (SelectedItem?.Address.City != AddressCity) return true;
            if (SelectedItem?.Address.Street != AddressStreet) return true;
            return false;
        }
    }

    /// <summary>
    /// Indicates whether the person-related fields have been changed compared to the selected entity.
    /// Used to enable/disable the save functionality.
    /// </summary>
    public bool IsPersonEntityChanged
    {
        get
        {
            if (SelectedItem?.FirstName != FirstName) return true;
            if (SelectedItem?.LastName != LastName) return true;
            if (SelectedItem?.Email != Email) return true;
            if (SelectedItem?.RoleId != RoleId) return true;
            if (SelectedItem?.IsActive != IsActive) return true;
            if (SelectedItem?.AddressId != AddressId) return true;
            return false;
        }
    }

    public int ItemCounter
    {
        get => field++;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = 0;

    // Convenience strongly-typed collection (wraps base Items)
    public ObservableCollection<Person> Persons => Items;

    public Guid Id
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public Guid RoleId
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public Guid AddressId
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string FirstName
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string LastName
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string Email
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public bool IsActive
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    // Replace RoleDisplay string with a selectable Role combo box backing properties
    public ObservableCollection<Role> Roles { get; } = [];

    public Role? SelectedRole
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RoleId = value?.Id ?? Guid.Empty;
            // Inline comment: synchronize RoleId and RoleDisplay when selection changes
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public string AddressPostalCode
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string AddressCity
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string AddressStreet
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string AddressCountry
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = "Danmark";

    public string AddressDisplay
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    } = string.Empty;

    public string SearchText
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
            ApplySearchFilter();
        }
    } = string.Empty;

    public ObservableCollection<Person> FilteredItems
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<Farm> SelectedPersonFarms { get; } = [];

    public bool IsFarmer => SelectedItem?.Role?.Name?.Equals("Farmer", StringComparison.OrdinalIgnoreCase) == true;

    #endregion
    #region Commands
    public RelayCommand<string>? SortCommand { get; }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="CRUDPersonUCViewModel"/>.
    /// </summary>
    /// <param name="statusInfoServices">Service used for status updates in the UI.</param>
    /// <param name="appMessageService">Service used to show messages to the user.</param>
    /// <param name="repository">Repository used to persist <see cref="Person"/> entities.</param>
    /// <param name="roleRepository">Repository used to load available roles for the dropdown. Optional for tests or callers that don't need roles.</param>
    public CRUDPersonUCViewModel(IStatusInfoServices statusInfoServices,
                                 IAppMessageService appMessageService,
                                 IPersonRepository repository,
                                 IAddressRepository addressRepository,
                                 IRoleRepository? roleRepository = null)
        : base(statusInfoServices, appMessageService, repository, false) // disable base auto-load
    {
        Repository = repository;
        _roleRepository = roleRepository;
        _addressRepository = addressRepository;

        SortCommand = new RelayCommand<string>(OnSortExecuted);

        // Load with navigation properties immediately
        _ = GetAllAsync();
        // Load roles only when a role repository is available (preserves testability)
        if (_roleRepository != null)
        {
            _ = LoadRolesAsync();
        }
        SelectedEntityChanged += CRUDPersonUCViewModel_SelectedEntityChanged;
        Items.CollectionChanged += (s, e) => // Update filtered items and counter when the main collection changes
        {
            ApplySearchFilter();
            OnPropertyChanged(nameof(ItemCounter));
        };
    }

    #region Events 
    private void CRUDPersonUCViewModel_SelectedEntityChanged(object? sender, Person? e)
    {
        PopulateFormFromPerson(e!);
        UpdateSelectedPersonFarms(); // Refresh farms when selected person changes
        OnPropertyChanged(nameof(IsFarmer)); // Notify that IsFarmer may have changed
    }
    #endregion
    #region Load Handlers
    ///// <summary>
    ///// Loads all persons including related role/navigation properties.
    ///// </summary>
    ///// <remarks>
    ///// Implementation note: this method is left asynchronous so it can be awaited from the UI layer.
    ///// It should populate the base <see cref="CRUDViewModelBase{TRepository,TEntity}.Items"/> collection.
    ///// </remarks>
    //private async Task GetAllAsync(CancellationToken ct = default)
    //{
    //    // Load directly from Repository so derived loader can surface a repository-specific error message
    //    try
    //    {
    //        using (_statusInfoServices.BeginLoadingOrSaving())
    //        {
    //            IEnumerable<Owner> all = await Repository.GetAllAsync(ct);
    //            Items.Clear();
    //            foreach (Owner p in all ?? Array.Empty<Owner>())
    //            {
    //                Items.Add(p);
    //            }
    //            SelectedItem = null;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Surface error to UI via AppMessageService if available
    //        try { _appMessageService?.AddErrorMessage("Failed to reload persons: " + ex.Message); } catch { }
    //    }
    //}

    /// <summary>
    /// Public helper to reload the person list. The view can call this when it becomes visible to ensure
    /// the Items collection is refreshed from the repository (fixes cases where navigation returns to a view
    /// with stale/empty items).
    /// </summary>
    public async Task ReloadAsync(CancellationToken ct = default)
    {
        await GetAllAsync(ct); //await reload from repository
        ApplySearchFilter(); // Refresh filtered items after reload
    }

    /// <summary>
    /// Loads available roles into the Roles collection for the UI dropdown.
    /// </summary>
    private async Task LoadRolesAsync(CancellationToken ct = default)
    {
        // Guard: if no role repository was provided (for example in unit tests), skip loading
        if (_roleRepository == null)
            return;

        try
        {
            IEnumerable<Role> all = await _roleRepository.GetAllAsync(ct);
            Roles.Clear();
            foreach (Role r in all ?? [])
            {
                Roles.Add(r);
            }

            // Ensure SelectedRole reflects RoleId if already set
            if (RoleId != Guid.Empty)
            {
                SelectedRole = Roles.FirstOrDefault(r => r.Id == RoleId);
            }
        }
        catch (Exception)
        {
            // Ignore load errors for now; UI will show empty dropdown
        }
    }

    #endregion
    #region Commands

    /// <summary>
    /// Sorts the bound collection by a specified property path.
    /// </summary>
    /// <param name="prop">Property path to sort by. Supports nested properties (e.g. "Role.Name").</param>
    private void OnSortExecuted(string? prop)
    {
        if (string.IsNullOrEmpty(prop)) return;

        bool descending = (_lastSortProp == prop) && !_lastSortDesc;
        _lastSortProp = prop;
        _lastSortDesc = descending;

        // Inline comment: use reflection-based getter so nested properties can be used when sorting
        List<Person> list = [.. (_lastSortDesc
            ? Items.OrderByDescending(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default)
            : Items.OrderBy(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default))];

        Items.Clear();
        foreach (Person? p in list)
        {
            Items.Add(p);
        }
    }
    #endregion
    #region Overrides of CRUDViewModelBase<Person>
    /// <summary>
    /// Creates a <see cref="Person"/> instance from the current form fields.
    /// </summary>
    protected override async Task<Person> OnAddFormAsync()
    {
        Person? savedPerson = null;
        using (_statusInfoServices?.BeginLoadingOrSaving())
        {
            try
            {
                Address createdAddress = await OnAddAddressFormAsync();
                Address savedAddress = await _addressRepository!.AddAsync(createdAddress);
                AddressId = savedAddress.Id;
                Debug.Assert(AddressId != Guid.Empty, "AddressId should not be empty after adding a new address.");
                Debug.WriteLine("New AddressId: " + AddressId);

                Person createdPerson = new()
                {
                    Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
                    RoleId = RoleId,
                    AddressId = AddressId,
                    FirstName = FirstName ?? string.Empty,
                    LastName = LastName ?? string.Empty,
                    Email = Email ?? string.Empty,
                    IsActive = IsActive
                };
                savedPerson = await Repository.AddAsync(createdPerson);
                Debug.Assert(savedPerson != null, "Saved person should not be null after adding.");
                Debug.WriteLine("Created Owner with Id: " + savedPerson.Id);

                await GetAllAsync();
                ApplySearchFilter();
                await OnResetFormAsync();
            }
            finally
            {
                IsSaving = false;
            }
            return savedPerson!;
        }
    }

    private async Task<Address> OnAddAddressFormAsync()
    {
        Address createdAddress = new()
        {
            PostalCode = AddressPostalCode,
            Street = AddressStreet,
            City = AddressCity,
            Country = AddressCountry
        };
        return createdAddress;
    }

    /// <summary>
    /// Loads the provided entity into the form fields so the UI can edit it.
    /// </summary>
    /// <param name="entity">The entity to load into the form.</param>
    protected override async Task OnLoadFormAsync(Person entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Use the new helper to populate form fields
        PopulateFormFromPerson(entity);

        // Ensure SelectedItem references the loaded entity so callers (and tests) observe the same instance
        SelectedItem = entity;

        // Ensure calling the protected hook directly switches to edit mode
        IsEditMode = true;
    }

    /// <summary>
    /// Resets the form fields to their default values.
    /// </summary>
    protected override async Task OnResetFormAsync()
    {
        // Owner fields
        Id = Guid.Empty;
        RoleId = Guid.Empty;
        AddressId = Guid.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        IsActive = false;

        // Address fields
        AddressPostalCode = string.Empty;
        AddressCity = string.Empty;
        AddressStreet = string.Empty;
        AddressCountry = "Danmark";

        // Clear selections
        SelectedItem = null!;
        SelectedRole = null;

        // Switch to add mode
        IsEditMode = false;
    }

    /// <summary>
    /// Persists the current form either by adding a new entity or updating an existing one.
    /// </summary>

    protected override bool CanAdd()
    {
        if (base.CanAdd() == false) return false;
        if (FormFieldsAreValid() == false) return false;
        return true;
    }

    protected override bool CanSave()
    {
        if (base.CanSave() == false) return false;
        if (FormFieldsAreValid() == false) return false;
        if (!IsPersonEntityChanged && !IsAddressEntityChanged) return false;
        return true;
    }
    protected override async Task OnSaveFormAsync()
    {
        using (_statusInfoServices?.BeginLoadingOrSaving())
        {
            Person? savedItem = null;
            try
            {
                IsSaving = true;

                if (IsAddMode)
                {
                    Address createdAddress = await OnAddAddressFormAsync();
                    Address rs = await _addressRepository!.AddAsync(createdAddress);
                    Debug.Assert(rs != null);
                    Debug.WriteLine("Created Address with Id: " + rs.Id);
                    AddressId = rs.Id;

                    Person toAdd = await OnAddFormAsync();
                    await Repository.AddAsync(toAdd);
                    // set base entity so callers can access it
                    SelectedItem = toAdd;
                    savedItem = toAdd;
                }
                else
                {
                    // Update existing entity
                    Person? existing = SelectedItem;
                    if (existing == null)
                    {
                        // nothing to update
                        return;
                    }

                    // Use RoleId as the authoritative source when SelectedRole is not available
                    existing.RoleId = SelectedRole?.Id ?? RoleId;
                    existing.AddressId = AddressId;
                    existing.FirstName = FirstName ?? string.Empty;
                    existing.LastName = LastName ?? string.Empty;
                    existing.Email = Email ?? string.Empty;
                    existing.IsActive = IsActive;

                    await Repository.UpdateAsync(existing);
                    savedItem = existing;
                }
            }
            finally
            {
                IsSaving = false;
            }
            // Reload the list so the ListView is refreshed with the latest data and
            // ensure the reload runs to completion before leaving the save operation.
            await GetAllAsync();
            ApplySearchFilter();
        }
    }

    #endregion
    #region Helpers
    /// <summary>
    /// Populates the form-bound fields from the provided Owner instance.
    /// RoleDisplay is set to the role's Name and AddressDisplay is set to "PostalCode, Street".
    /// This method is public so callers (for example tests or other view-models) can reuse the mapping.
    /// </summary>
    /// <param name="person">Owner to populate the form from.</param>
    public void PopulateFormFromPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        // Form for Owner
        Id = person.Id;
        RoleId = person.RoleId;
        AddressId = person.AddressId;
        FirstName = person.FirstName ?? string.Empty;
        LastName = person.LastName ?? string.Empty;
        Email = person.Email ?? string.Empty;
        IsActive = person.IsActive;
        // Select matching Role in Roles collection when available
        if (person.Role != null)
        {
            SelectedRole = Roles.FirstOrDefault(r => r.Id == person.Role.Id) ?? person.Role;
        }
        else
        {
            SelectedRole = null;
        }

        // Form for Address
        AddressCity = person.Address?.City ?? string.Empty;
        AddressCountry = person.Address?.Country ?? string.Empty;
        AddressPostalCode = person.Address?.PostalCode ?? string.Empty;
        AddressStreet = person.Address?.Street ?? string.Empty;

    }

    /// <summary>
    /// Retrieves a nested property value using a dot-separated property path.
    /// </summary>
    /// <param name="obj">Object to read the property from.</param>
    /// <param name="propertyPath">Dot-separated property path (for example "Role.Name").</param>
    /// <returns>The nested property value or null if any part of the path is not found.</returns>
    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propertyPath)) return null;

        object? current = obj;
        Type type = current.GetType();
        foreach (string part in propertyPath.Split('.'))
        {
            PropertyInfo? prop = type.GetProperty(part);
            if (prop == null) return null;
            current = prop.GetValue(current, null);
            if (current == null) return null;
            type = current.GetType();
        }
        // Inline comment: returns the final nested value (could be primitive or object)
        return current;
    }

    /// <summary>
    /// applies the search filter to the Items collection and updates the FilteredItems collection.
    /// </summary>
    private void ApplySearchFilter()
    {
        FilteredItems.Clear();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (Person person in Items)
            {
                FilteredItems.Add(person);
            }
        }
        else
        {
            string search = SearchText.Trim().ToLowerInvariant();
            foreach (Person person in Items)
            {
                string firstName = person.FirstName?.ToLowerInvariant() ?? string.Empty;
                string lastName = person.LastName?.ToLowerInvariant() ?? string.Empty;
                string email = person.Email?.ToLowerInvariant() ?? string.Empty;
                string fullName = $"{firstName} {lastName}".Trim();

                if (firstName.Contains(search) ||
                    lastName.Contains(search) ||
                    email.Contains(search) ||
                    fullName.Contains(search))
                {
                    FilteredItems.Add(person);
                }
            }
        }
        OnPropertyChanged(nameof(FilteredItems));
    }
    // Updates the SelectedPersonFarms collection based on the currently selected person
    private void UpdateSelectedPersonFarms()
    {
        SelectedPersonFarms.Clear();
        if (SelectedItem?.Farms != null)
        {
            foreach (Farm farm in SelectedItem.Farms)
            {
                SelectedPersonFarms.Add(farm);
            }
        }
    }

    private bool FormFieldsAreValid()
    {
        // Example validation logic; expand as needed
        if (string.IsNullOrWhiteSpace(FirstName)) return false;
        if (string.IsNullOrWhiteSpace(LastName)) return false;
        if (string.IsNullOrWhiteSpace(Email)) return false;
        if (RoleId == Guid.Empty) return false;
        if (IsEditMode && AddressId == Guid.Empty) return false;
        if (string.IsNullOrWhiteSpace(AddressPostalCode)) return false;
        if (string.IsNullOrWhiteSpace(AddressStreet)) return false;
        if (string.IsNullOrWhiteSpace(AddressCity)) return false;
        return true;
    }
    #endregion
}