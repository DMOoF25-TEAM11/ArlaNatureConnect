using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
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

    // Instance properties for XAML binding to label texts
    public string LabelFirstName => LABEL_FIRSTNAME;
    public string LabelLastName => LABEL_LASTNAME;
    public string LabelEmail => LABEL_EMAIL;
    public string LabelIsActive => LABEL_ISACTIVE;
    public string LabelRoleId => LABEL_ROLEID;
    public string LabelAddressId => LABEL_ADDRESSID;

    #region Fields
    private Guid _id;
    private Guid _roleId;
    private Guid _addressId;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private bool _isActive;
    #endregion
    #region Properties

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


    public CRUDPersonUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository)
        : base(statusInfoServices, appMessageService, repository)
    {
    }

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
        base.Entity = entity;

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

        base.Entity = null;

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
                await Repository.AddAsync(toAdd).ConfigureAwait(false);
                // set base entity so callers can access it
                base.Entity = toAdd;
            }
            else
            {
                // Update existing entity
                Person? existing = base.Entity;
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

                await Repository.UpdateAsync(existing).ConfigureAwait(false);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }
}