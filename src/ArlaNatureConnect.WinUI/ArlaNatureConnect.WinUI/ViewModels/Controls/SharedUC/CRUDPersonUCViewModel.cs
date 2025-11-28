using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

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
/// </remarks>
public class CRUDPersonUCViewModel
    : CRUDViewModelBase<IPersonRepository, Person>
{
    #region constants
    public const string LABEL_FIRSTNAME = "Fornavn";
    public const string LABEL_LASTNAME = "Efternavn";
    public const string LABEL_EMAIL = "Email";
    public const string LABEL_ISACTIVE = "Aktiv";
    public const string LABEL_ROLE = "Rolle";
    public const string LABEL_ADDRESSID = "Adresse";
    public const string LABEL_FARMS = "Antal Gårde";

    #endregion
    #region Fields
    // Repository for data access
    private Guid _id;
    private Guid _roleId;
    private Guid _addressId;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private bool _isActive;

    // Sorting state
    private string? _lastSortProp;
    private bool _lastSortDesc;
    private int _itemCounter = 0;
    #endregion
    #region Properties
    // Instance properties for XAML binding to label texts
    public static string LabelFirstName => LABEL_FIRSTNAME;
    public static string LabelLastName => LABEL_LASTNAME;
    public static string LabelEmail => LABEL_EMAIL;
    public static string LabelIsActive => LABEL_ISACTIVE;
    public static string LabelRoleId => LABEL_ROLE;
    public static string LabelAddressId => LABEL_ADDRESSID;

    public int ItemCounter
    {
        get => _itemCounter++;
        set
        {
            if (_itemCounter == value) return;
            _itemCounter = value;
            OnPropertyChanged();
        }
    }

    // Convenience strongly-typed collection (wraps base Items)
    public System.Collections.ObjectModel.ObservableCollection<Person> Persons => Items;

    public Guid Id
    {
        get => _id;
        set
        {
            if (_id == value) return;
            _id = value;
            OnPropertyChanged();
        }
    }

    public Guid RoleId
    {
        get => _roleId;
        set
        {
            if (_roleId == value) return;
            _roleId = value;
            OnPropertyChanged();
        }
    }

    public Guid AddressId
    {
        get => _addressId;
        set
        {
            if (_addressId == value) return;
            _addressId = value;
            OnPropertyChanged();
        }
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName == value) return;
            _firstName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName == value) return;
            _lastName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (_email == value) return;
            _email = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            OnPropertyChanged();
        }
    }

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
    public CRUDPersonUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository)
        : base(statusInfoServices, appMessageService, repository) // disable base auto-load
    {
        _repository = repository;
        //_personQueryService = personQueryService;

        SortCommand = new RelayCommand<string>(OnSortExecuted);

        // Load with navigation properties immediately
        _ = LoadAllWithRolesAsync();
    }

    /// <summary>
    /// Loads all persons including related role/navigation properties.
    /// </summary>
    /// <remarks>
    /// Implementation note: this method is left asynchronous so it can be awaited from the UI layer.
    /// It should populate the base <see cref="CRUDViewModelBase{TRepository,TEntity}.Items"/> collection.
    /// </remarks>
    private async Task LoadAllWithRolesAsync(CancellationToken ct = default)
    {
        // Inline comment: load persons including related navigation data from repository when implemented.
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
    /// Sorts the bound collection by a specified property path.
    /// </summary>
    /// <param name="prop">Property path to sort by. Supports nested properties (e.g. "Role.Name").</param>
    private void OnSortExecuted(string? prop)
    {
        if (string.IsNullOrEmpty(prop)) return;

        bool descending = (_lastSortProp == prop) ? !_lastSortDesc : false;
        _lastSortProp = prop;
        _lastSortDesc = descending;

        // Inline comment: use reflection-based getter so nested properties can be used when sorting
        List<Person> list = (_lastSortDesc
            ? Items.OrderByDescending(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default)
            : Items.OrderBy(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default))
            .ToList();

        Items.Clear();
        foreach (Person? p in list)
        {
            Items.Add(p);
        }
    }


    #region Overrides of CRUDViewModelBase<Person>
    /// <summary>
    /// Creates a <see cref="Person"/> instance from the current form fields.
    /// </summary>
    protected override Task<Person> OnAddFormAsync()
    {
        // Create a new Person instance from view-model fields
        Person p = new Person
        {
            Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
            RoleId = RoleId,
            AddressId = AddressId,
            FirstName = FirstName ?? string.Empty,
            LastName = LastName ?? string.Empty,
            Email = Email ?? string.Empty,
            IsActive = IsActive
        };

        return Task.FromResult(p);
    }

    /// <summary>
    /// Loads the provided entity into the form fields so the UI can edit it.
    /// </summary>
    /// <param name="entity">The entity to load into the form.</param>
    protected override Task OnLoadFormAsync(Person entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        Id = entity.Id;
        RoleId = entity.RoleId;
        AddressId = entity.AddressId;
        FirstName = entity.FirstName ?? string.Empty;
        LastName = entity.LastName ?? string.Empty;
        Email = entity.Email ?? string.Empty;
        IsActive = entity.IsActive;

        SelectedItem = entity;

        // Ensure calling the protected hook directly switches to edit mode
        IsEditMode = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets the form fields to their default values.
    /// </summary>
    protected override Task OnResetFormAsync()
    {
        // Reset form fields to defaults
        Id = Guid.Empty;
        RoleId = Guid.Empty;
        AddressId = Guid.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        IsActive = false;

        SelectedItem = null;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists the current form either by adding a new entity or updating an existing one.
    /// </summary>
    protected override async Task OnSaveFormAsync()
    {
        try
        {
            IsSaving = true;

            if (IsAddMode)
            {
                Person toAdd = await OnAddFormAsync().ConfigureAwait(false);
                await _repository.AddAsync(toAdd).ConfigureAwait(false);
                // set base entity so callers can access it
                SelectedItem = toAdd;
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

                existing.RoleId = RoleId;
                existing.AddressId = AddressId;
                existing.FirstName = FirstName ?? string.Empty;
                existing.LastName = LastName ?? string.Empty;
                existing.Email = Email ?? string.Empty;
                existing.IsActive = IsActive;

                await _repository.UpdateAsync(existing).ConfigureAwait(false);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }
    #endregion
}