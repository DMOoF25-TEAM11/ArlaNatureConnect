using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Collections.ObjectModel;
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
    public const string LABEL_ADDRESS = "Adresse";
    public const string LABEL_FARMS = "Antal Gårde";

    #endregion
    #region Fields
    // Repository for data access

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
    public static string LabelAddress => LABEL_ADDRESS;
    public static string LabelFarms => LABEL_FARMS;

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
        }
    }

    // New display properties for Role (name) and Address (postal + street)
    public string RoleDisplay
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string AddressDisplay
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            OnPropertyChanged();
        }
    } = string.Empty;

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
    public CRUDPersonUCViewModel(IStatusInfoServices statusInfoServices,
                                 IAppMessageService appMessageService,
                                 IPersonRepository repository)
        : base(statusInfoServices, appMessageService, repository, false) // disable base auto-load
    {
        Repository = repository;
        //_personQueryService = personQueryService;

        SortCommand = new RelayCommand<string>(OnSortExecuted);

        // Load with navigation properties immediately
        _ = GetAllAsync();
        SelectedEntityChanged += CRUDPersonUCViewModel_SelectedEntityChanged;
    }

    private void CRUDPersonUCViewModel_SelectedEntityChanged(object? sender, Person? e)
    {
        PopulateFormFromPerson(e!);
    }

    #region Load Handlers
    /// <summary>
    /// Loads all persons including related role/navigation properties.
    /// </summary>
    /// <remarks>
    /// Implementation note: this method is left asynchronous so it can be awaited from the UI layer.
    /// It should populate the base <see cref="CRUDViewModelBase{TRepository,TEntity}.Items"/> collection.
    /// </remarks>
    private async Task GetAllAsync(CancellationToken ct = default)
    {
        // Load directly from Repository so derived loader can surface a repository-specific error message
        try
        {
            using (_statusInfoServices.BeginLoadingOrSaving())
            {
                IEnumerable<Person> all = await Repository.GetAllAsync(ct);
                Items.Clear();
                foreach (Person p in all ?? Array.Empty<Person>())
                {
                    Items.Add(p);
                }
                SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            // Surface error to UI via AppMessageService if available
            try { _appMessageService?.AddErrorMessage("Failed to reload persons: " + ex.Message); } catch { }
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
    protected override Task<Person> OnAddFormAsync()
    {
        // Create a new Person instance from view-model fields
        Person p = new()
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
        ArgumentNullException.ThrowIfNull(entity);

        // Use the new helper to populate form fields
        PopulateFormFromPerson(entity);

        // Ensure SelectedItem references the loaded entity so callers (and tests) observe the same instance
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

        // Clear display fields
        RoleDisplay = string.Empty;
        AddressDisplay = string.Empty;

        SelectedItem = null!;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists the current form either by adding a new entity or updating an existing one.
    /// </summary>
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

                    existing.RoleId = RoleId;
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
            await ReloadAsync();

            // Ensure selection persists after reload: restore the saved item instance if present.
            // This prevents the reload from clearing SelectedItem and makes the post-save state
            // consistent for callers (including unit tests) that expect SelectedItem to reference
            // the saved entity.
            //if (savedItem != null)
            //{
            //    // Try to find an existing instance in the refreshed Items collection
            //    Person? match = Items.FirstOrDefault(p => p.Id == savedItem.Id);
            //    if (match != null)
            //    {
            //        // Replace the collection element so the UI receives a Replace notification
            //        int idx = Items.IndexOf(match);
            //        if (idx >= 0)
            //        {
            //            Items[idx] = savedItem;
            //            SelectedItem = savedItem;
            //        }
            //        else
            //        {
            //            // fallback: select the found instance
            //            SelectedItem = match;
            //        }
            //    }
            //    else
            //    {
            //        // Not present in refreshed list - add and select so UI shows the item
            //        Items.Add(savedItem);
            //        SelectedItem = savedItem;
            //    }
            //}
        }
    }

    #endregion
    #region Helpers
    /// <summary>
    /// Populates the form-bound fields from the provided Person instance.
    /// RoleDisplay is set to the role's Name and AddressDisplay is set to "PostalCode, Street".
    /// This method is public so callers (for example tests or other view-models) can reuse the mapping.
    /// </summary>
    /// <param name="person">Person to populate the form from.</param>
    public void PopulateFormFromPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        Id = person.Id;
        RoleId = person.RoleId;
        AddressId = person.AddressId;
        FirstName = person.FirstName ?? string.Empty;
        LastName = person.LastName ?? string.Empty;
        Email = person.Email ?? string.Empty;
        IsActive = person.IsActive;

        // Map role and address to display strings used by the form
        RoleDisplay = person.Role?.Name ?? string.Empty;
        AddressDisplay = person.Address != null
            ? string.Concat(person.Address.PostalCode ?? string.Empty, ", ", person.Address.Street ?? string.Empty)
            : string.Empty;
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

    #endregion

    /// <summary>
    /// Public helper to reload the person list. The view can call this when it becomes visible to ensure
    /// the Items collection is refreshed from the repository (fixes cases where navigation returns to a view
    /// with stale/empty items).
    /// </summary>
    public Task ReloadAsync(CancellationToken ct = default)
    {
        return GetAllAsync(ct);
    }
}