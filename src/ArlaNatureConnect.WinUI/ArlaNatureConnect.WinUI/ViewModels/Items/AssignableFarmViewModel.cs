using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using static ArlaNatureConnect.WinUI.Helpers.PriorityTranslator;

namespace ArlaNatureConnect.WinUI.ViewModels.Items;

// Purpose: View-model wrapper for FarmAssignmentOverviewDto providing change notifications for the UI list.
// Notes: Keeps WinUI bindings simple while allowing service data to refresh individual items in-place.
public sealed class AssignableFarmViewModel : ViewModelBase
{
    #region Fields
    private bool _hasActiveCase;
    private string _farmName = string.Empty;
    private string _cvr = string.Empty;
    private string _ownerFirstName = string.Empty;
    private string _ownerLastName = string.Empty;
    private string _ownerEmail = string.Empty;
    private string _street = string.Empty;
    private string _city = string.Empty;
    private string _postalCode = string.Empty;
    private string _country = string.Empty;
    private string _addressLine = string.Empty;
    private string _consultantName = string.Empty;
    private Guid? _assignedConsultantId;
    private string? _priority;
    private string? _notes;
    #endregion

    #region Field Commands
    #endregion

    #region Event Handlers
    #endregion

    #region Properties
    public Guid FarmId { get; }
    #endregion

    #region Observable Properties
    public string FarmName
    {
        get => _farmName;
        private set
        {
            if (_farmName == value) return;
            _farmName = value;
            OnPropertyChanged();
        }
    }

    public string Cvr
    {
        get => _cvr;
        private set
        {
            if (_cvr == value) return;
            _cvr = value;
            OnPropertyChanged();
        }
    }

    public string OwnerFirstName
    {
        get => _ownerFirstName;
        private set
        {
            if (_ownerFirstName == value) return;
            _ownerFirstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OwnerName));
        }
    }

    public string OwnerLastName
    {
        get => _ownerLastName;
        private set
        {
            if (_ownerLastName == value) return;
            _ownerLastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OwnerName));
        }
    }

    public string OwnerEmail
    {
        get => _ownerEmail;
        private set
        {
            if (_ownerEmail == value) return;
            _ownerEmail = value;
            OnPropertyChanged();
        }
    }

    public string OwnerName => $"{OwnerFirstName} {OwnerLastName}".Trim();

    public string Street
    {
        get => _street;
        private set
        {
            if (_street == value) return;
            _street = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AddressLine));
        }
    }

    public string City
    {
        get => _city;
        private set
        {
            if (_city == value) return;
            _city = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AddressLine));
        }
    }

    public string PostalCode
    {
        get => _postalCode;
        private set
        {
            if (_postalCode == value) return;
            _postalCode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AddressLine));
        }
    }

    public string Country
    {
        get => _country;
        private set
        {
            if (_country == value) return;
            _country = value;
            OnPropertyChanged();
        }
    }

    public string AddressLine
    {
        get => _addressLine;
        private set
        {
            if (_addressLine == value) return;
            _addressLine = value;
            OnPropertyChanged();
        }
    }

    public string ConsultantName
    {
        get => _consultantName;
        private set
        {
            if (_consultantName == value) return;
            _consultantName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ConsultantDisplay));
        }
    }

    public bool HasActiveCase
    {
        get => _hasActiveCase;
        set
        {
            if (_hasActiveCase == value) return;
            _hasActiveCase = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusLabel));
        }
    }

    public string StatusLabel => HasActiveCase ? "Tilføjet" : "Ikke tilføjet";
    public string ConsultantDisplay => HasActiveCase && !string.IsNullOrWhiteSpace(ConsultantName)
        ? $"Konsulent: {ConsultantName}"
        : string.Empty;

    public Guid? AssignedConsultantId
    {
        get => _assignedConsultantId;
        private set
        {
            if (_assignedConsultantId == value) return;
            _assignedConsultantId = value;
            OnPropertyChanged();
        }
    }

    public string? Priority
    {
        get => _priority;
        private set
        {
            if (_priority == value) return;
            _priority = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PriorityDisplay));
            OnPropertyChanged(nameof(HasPriority));
        }
    }

    public string? Notes
    {
        get => _notes;
        private set
        {
            if (_notes == value) return;
            _notes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the priority display value (converted from English to Danish).
    /// </summary>
    public string? PriorityDisplay => ToDanish(Priority);

    /// <summary>
    /// Gets a value indicating whether priority is set (for visibility binding).
    /// </summary>
    public bool HasPriority => !string.IsNullOrWhiteSpace(Priority);

    public string SearchContent => $"{FarmName} {OwnerName} {Cvr} {Street} {City} {PostalCode}".ToLowerInvariant();
    #endregion

    #region Load Handler
    public AssignableFarmViewModel(FarmAssignmentOverviewDto dto)
    {
        FarmId = dto.FarmId;
        Apply(dto);
    }
    #endregion

    #region Commands
    public void Apply(FarmAssignmentOverviewDto dto)
    {
        FarmName = dto.FarmName;
        Cvr = dto.Cvr;
        OwnerFirstName = dto.OwnerFirstName;
        OwnerLastName = dto.OwnerLastName;
        OwnerEmail = dto.OwnerEmail;
        Street = dto.Street;
        City = dto.City;
        PostalCode = dto.PostalCode;
        Country = dto.Country;
        AddressLine = dto.AddressLine;
        HasActiveCase = dto.HasActiveCase;
        ConsultantName = dto.AssignedConsultantName;
        AssignedConsultantId = dto.AssignedConsultantId;
        Priority = dto.Priority;
        Notes = dto.Notes;
    }
    #endregion

    #region CanXXX Command
    #endregion

    #region OnXXX Command
    #endregion

    #region Helpers
    #endregion
}

