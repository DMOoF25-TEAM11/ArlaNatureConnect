using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Abstract.Services;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

public class CRUDPersonUCViewModel
    : CRUDViewModelBase<IPersonRepository, Person>
{
    #region constants
    public const string LABEL_FIRSTNAME = "Fornavn";
    public const string LABEL_LASTNAME = "Efternavn";
    public const string LABEL_EMAIL = "Email";
    public const string LABEL_ISACTIVE = "Aktiv";
    public const string LABEL_ROLEID = "Rolle Id";
    public const string LABEL_ADDRESSID = "Adresse Id";
    #endregion
    #region Fields
    private readonly IPersonQueryService _personQueryService;

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
    public static string LabelRoleId => LABEL_ROLEID;
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

    public CRUDPersonUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository,
        IPersonQueryService personQueryService)
        : base(statusInfoServices, appMessageService, repository)
    {
        _repository = repository;
        _personQueryService = personQueryService;

        // Initialize sort command
        SortCommand = new RelayCommand<string>(OnSortExecuted);

        // Reload persons including Role navigation properties
        _ = LoadAllWithRolesAsync();
    }

    private async Task LoadAllWithRolesAsync(CancellationToken ct = default)
    {
        try
        {
            var all = await _personQueryService.GetAllWithRolesAsync(ct);

            Items.Clear();
            foreach (var p in all)
            {
                Items.Add(p);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Persons));
        }
        catch (Exception)
        {
            // swallow errors to keep UI responsive; messaging service may be used if needed
        }
    }

    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propertyPath)) return null;

        var current = obj;
        var type = current.GetType();
        foreach (var part in propertyPath.Split('.'))
        {
            var prop = type.GetProperty(part);
            if (prop == null) return null;
            current = prop.GetValue(current, null);
            if (current == null) return null;
            type = current.GetType();
        }
        return current;
    }

    private void OnSortExecuted(string? prop)
    {
        if (string.IsNullOrEmpty(prop)) return;

        bool descending = (_lastSortProp == prop) ? !_lastSortDesc : false;
        _lastSortProp = prop;
        _lastSortDesc = descending;

        var list = (_lastSortDesc
            ? Items.OrderByDescending(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default)
            : Items.OrderBy(p => GetNestedPropertyValue(p, prop), Comparer<object?>.Default))
            .ToList();

        Items.Clear();
        foreach (var p in list)
        {
            Items.Add(p);
        }
    }


    #region Overrides of CRUDViewModelBase<Person>
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

    protected override Task OnLoadFormAsync(Person entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Populate view-model properties from the loaded entity
        Id = entity.Id;
        RoleId = entity.RoleId;
        AddressId = entity.AddressId;
        FirstName = entity.FirstName ?? string.Empty;
        LastName = entity.LastName ?? string.Empty;
        Email = entity.Email ?? string.Empty;
        IsActive = entity.IsActive;

        // Also keep base.Entity in sync
        SelectedItem = entity;

        return Task.CompletedTask;
    }

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