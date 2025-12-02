using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Items;
using ArlaNatureConnect.WinUI.Helpers;
using static ArlaNatureConnect.WinUI.Helpers.PriorityTranslator;


namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

// Purpose: ViewModel for ArlaEmployeeAssignNatureCheck UserControl - handles farm assignment, filtering, and farm CRUD operations.
// Notes: Separated from ArlaEmployeePageViewModel to follow MVVM pattern where each view has its own ViewModel.
public class ArlaEmployeeAssignNatureCheckViewModel : ViewModelBase
{
    #region Fields
    private readonly INatureCheckCaseService _natureCheckCaseService;
    private readonly IAppMessageService _appMessageService;
    private readonly IStatusInfoServices _statusInfoServices;
    private readonly List<AssignableFarmViewModel> _allFarms = new();
    private AssignableFarmViewModel? _selectedFarm;
    private Person? _selectedConsultant;
    private bool _assignmentLoaded;
    private bool _isBusy;
    private string _farmSearchText = string.Empty;
    private string _assignmentNotes = string.Empty;
    private string? _selectedPriority;
    private FarmStatusFilter _currentStatusFilter = FarmStatusFilter.All;
    private bool _isFarmEditorVisible;
    private bool _isFarmEditMode;
    private Guid? _editingFarmId;
    private Guid? _assignedByPersonId;
    #endregion

    #region Field Commands
    public RelayCommand AssignNatureCheckCaseCommand { get; } // assign selected farm to selected consultant
    public RelayCommand<string> ApplyStatusFilterCommand { get; } // parameter: "All", "Assigned", "Unassigned"
    public RelayCommand<string> ShowFarmEditorCommand { get; } // parameter: "Add" or "Edit"
    public RelayCommand SaveFarmCommand { get; } // save farm from form
    public RelayCommand CancelFarmEditorCommand { get; } // hide farm editor
    public RelayCommand DeleteFarmCommand { get; } // delete selected farm
    #endregion


    #region Properties
    /// <summary>
    /// Gets the collection of farms that match the current filter criteria.
    /// </summary>
    /// <remarks>The collection is updated to reflect changes in filtering and can be observed for changes to
    /// its contents. This property is read-only and cannot be set directly.</remarks>
    public ObservableCollection<AssignableFarmViewModel> FilteredFarms { get; } = new();

    /// <summary>
    /// Gets the collection of consultants associated with this instance.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as additions
    /// or removals of consultants. Modifying the collection will automatically notify any observers subscribed to its
    /// change events.</remarks>
    public ObservableCollection<Person> Consultants { get; } = new();

    /// <summary>
    /// Gets or sets the farm currently selected in the user interface.
    /// </summary>
    /// <remarks>Changing this property updates related commands and notifies property changes, which may
    /// affect UI elements bound to farm selection or farm-related actions.</remarks>
    public AssignableFarmViewModel? SelectedFarm
    {
        get => _selectedFarm;
        set
        {
            if (_selectedFarm == value) return;
            _selectedFarm = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFarmSelected));
            AssignNatureCheckCaseCommand.RaiseCanExecuteChanged();
            DeleteFarmCommand.RaiseCanExecuteChanged();
            ShowFarmEditorCommand.RaiseCanExecuteChanged();

            // When a farm with an active case is selected, populate the form with existing data
            if (value != null && value.HasActiveCase)
            {
                // Set consultant if available
                if (value.AssignedConsultantId.HasValue)
                {
                    SelectedConsultant = Consultants.FirstOrDefault(c => c.Id == value.AssignedConsultantId.Value);
                }

                // Set priority (convert from English to Danish)
                SelectedPriority = ToDanish(value.Priority);

                // Set notes
                AssignmentNotes = value.Notes ?? string.Empty;
            }
            else
            {
                // Clear form when selecting a farm without active case or deselecting
                SelectedConsultant = null;
                SelectedPriority = null;
                AssignmentNotes = string.Empty;
            }
        }
    }
    /// <summary>
    /// Gets a value indicating whether a farm is currently selected.
    /// </summary>
    public bool IsFarmSelected => SelectedFarm != null;

    /// <summary>
    /// Gets or sets the currently selected consultant.
    /// </summary>
    /// <remarks>Changing this property raises the <c>PropertyChanged</c> event and updates the state of the
    /// <c>AssignNatureCheckCaseCommand</c> command. This property may be <see langword="null"/> if no consultant is
    /// selected.</remarks>
    public Person? SelectedConsultant
    {
        get => _selectedConsultant;
        set
        {
            if (_selectedConsultant == value) return;
            _selectedConsultant = value;
            OnPropertyChanged();
            AssignNatureCheckCaseCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the notes or comments associated with the assignment.
    /// </summary>
    public string AssignmentNotes
    {
        get => _assignmentNotes;
        set
        {
            if (_assignmentNotes == value) return;
            _assignmentNotes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of available priority options (displayed in Danish).
    /// </summary>
    public IReadOnlyList<string> PriorityOptions { get; } = new[] { "Lav", "Normal", "Høj", "Haster" };

    /// <summary>
    /// Gets or sets the selected priority for the assignment (in Danish for display).
    /// </summary>
    public string? SelectedPriority
    {
        get => _selectedPriority;
        set
        {
            if (_selectedPriority == value) return;
            _selectedPriority = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Gets or sets the search text used to filter the list of farms.
    /// </summary>
    /// <remarks>Changing this property automatically updates the farm list to match the specified search
    /// criteria.</remarks>
    public string FarmSearchText
    {
        get => _farmSearchText;
        set
        {
            if (_farmSearchText == value) return;
            _farmSearchText = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    /// <summary>
    /// Gets a value indicating whether an operation is currently in progress.
    /// </summary>
    /// <remarks>Use this property to determine if the object is busy performing a task, such as saving or
    /// deleting data. This can be useful for controlling UI elements or preventing concurrent operations. The value is
    /// updated automatically when relevant commands are executed.</remarks>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            AssignNatureCheckCaseCommand.RaiseCanExecuteChanged();
            SaveFarmCommand.RaiseCanExecuteChanged();
            DeleteFarmCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsFarmEditorVisible
    {
        get => _isFarmEditorVisible;
        private set
        {
            if (_isFarmEditorVisible == value) return;
            _isFarmEditorVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsFarmEditMode
    {
        get => _isFarmEditMode;
        private set
        {
            if (_isFarmEditMode == value) return;
            _isFarmEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FarmEditorTitle));
            OnPropertyChanged(nameof(FarmEditorSubmitText));
        }
    }

    public string FarmEditorTitle => IsFarmEditMode ? "Rediger gård" : "Tilføj ny gård";
    public string FarmEditorSubmitText => IsFarmEditMode ? "Opdater gård" : "Tilføj gård";

    public FarmFormModel FarmForm { get; } = new();

    /// <summary>
    /// Sets the person ID of the user assigning the case. This should be set by the parent ViewModel.
    /// </summary>
    public Guid? AssignedByPersonId
    {
        get => _assignedByPersonId;
        set
        {
            if (_assignedByPersonId == value) return;
            _assignedByPersonId = value;
            OnPropertyChanged();
        }
    }
    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the ArlaEmployeeAssignNatureCheckViewModel class with the specified services
    /// required for nature check case assignment and farm management operations.
    /// </summary>
    /// <param name="natureCheckCaseService">The service used to manage and assign nature check cases within the view model. Cannot be null.</param>
    /// <param name="appMessageService">The service responsible for displaying application messages and notifications to the user. Cannot be null.</param>
    /// <param name="statusInfoServices">The service that provides status information and filtering capabilities for nature check cases. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters—natureCheckCaseService, appMessageService, or statusInfoServices—are null.</exception>
    public ArlaEmployeeAssignNatureCheckViewModel(
        INatureCheckCaseService natureCheckCaseService,
        IAppMessageService appMessageService,
        IStatusInfoServices statusInfoServices)
    {
        _natureCheckCaseService = natureCheckCaseService ?? throw new ArgumentNullException(nameof(natureCheckCaseService));
        _appMessageService = appMessageService ?? throw new ArgumentNullException(nameof(appMessageService));
        _statusInfoServices = statusInfoServices ?? throw new ArgumentNullException(nameof(statusInfoServices));

        AssignNatureCheckCaseCommand = new RelayCommand(async () => await AssignNatureCheckCaseAsync(), CanAssignCase);
        ApplyStatusFilterCommand = new RelayCommand<string>(ApplyStatusFilter);
        ShowFarmEditorCommand = new RelayCommand<string>(ShowFarmEditor, CanShowFarmEditor);
        SaveFarmCommand = new RelayCommand(async () => await SaveFarmAsync(), CanSaveFarm);
        CancelFarmEditorCommand = new RelayCommand(HideFarmEditor);
        DeleteFarmCommand = new RelayCommand(async () => await DeleteSelectedFarmAsync(), CanDeleteFarm);
        FarmForm.PropertyChanged += OnFarmFormPropertyChanged;
    }
    #endregion

    #region Load Handler
    /// <summary>
    /// Asynchronously initializes the handler by loading required assignment data.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        await EnsureAssignmentDataAsync();
    }
    #endregion

    #region Commands
    /// <summary>
    /// Assigns a nature check case to the selected farm and consultant asynchronously, if assignment is permitted.
    /// </summary>
    /// <remarks>If assignment is not permitted, the method completes without performing any action. Upon
    /// successful assignment, an informational message is displayed and assignment data is refreshed. If an error
    /// occurs during assignment, an error message is displayed.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AssignNatureCheckCaseAsync()
    {
        if (!CanAssignCase())
        {
            return;
        }

        await ExecuteWithBusyStateAsync(async () =>
        {
            NatureCheckCaseAssignmentRequest request = new()
            {
                FarmId = SelectedFarm!.FarmId,
                ConsultantId = SelectedConsultant!.Id,
                AssignedByPersonId = AssignedByPersonId ?? Guid.Empty,
                Notes = AssignmentNotes,
                Priority = ToEnglish(SelectedPriority),
                AllowDuplicateActiveCase = false
            };

            await _natureCheckCaseService.AssignCaseAsync(request);
            _appMessageService.AddInfoMessage($"Natur Check opgave er oprettet for {SelectedFarm.FarmName}.");

            AssignmentNotes = string.Empty;
            SelectedPriority = null;
            await EnsureAssignmentDataAsync(forceReload: true);
        });
    }

    /// <summary>
    /// Ensures that assignment data is loaded into the current context, optionally forcing a reload of the data.
    /// </summary>
    /// <remarks>If the assignment data is already loaded and <paramref name="forceReload"/> is <see
    /// langword="false"/>, the method returns immediately without reloading. If an error occurs during loading, an
    /// error message is added and the assignment data can be retried on subsequent calls.</remarks>
    /// <param name="forceReload">Specifies whether to force reloading the assignment data even if it has already been loaded. Set to <see
    /// langword="true"/> to reload the data; otherwise, <see langword="false"/> to use cached data if available.</param>
    /// <returns>A task that represents the asynchronous operation of loading assignment data.</returns>
    private async Task EnsureAssignmentDataAsync(bool forceReload = false)
    {
        if (_assignmentLoaded && !forceReload)
        {
            return;
        }

        try
        {
            using IDisposable loading = _statusInfoServices.BeginLoadingOrSaving();

            NatureCheckCaseAssignmentContext context = await _natureCheckCaseService.LoadAssignmentContextAsync();
            UpdateFarms(context.Farms);
            UpdateConsultants(context.Consultants);

            _assignmentLoaded = true;
        }
        catch (Exception ex)
        {
            _appMessageService.AddErrorMessage($"Kunne ikke hente data: {ex.Message}");
            _assignmentLoaded = false; // Reset flag so we can retry
        }
    }

    /// <summary>
    /// Applies a status filter to the current farm view based on the specified filter token.
    /// </summary>
    /// <remarks>If <paramref name="filterToken"/> is not "Assigned" or "Unassigned", the filter defaults to
    /// showing all statuses.</remarks>
    /// <param name="filterToken">A string representing the desired status filter. Valid values are "Assigned", "Unassigned", or <see
    /// langword="null"/> to show all statuses. Comparison is case-insensitive.</param>
    private void ApplyStatusFilter(string? filterToken)
    {
        if (!Enum.TryParse(filterToken, true, out FarmStatusFilter parsed))
        {
            parsed = FarmStatusFilter.All;
        }

        _currentStatusFilter = parsed;
        ApplyFilters();
    }

    /// <summary>
    /// Displays the farm editor interface in either edit or create mode, depending on the specified mode.
    /// </summary>
    /// <remarks>If no farm is selected when "Edit" mode is specified, the editor will not be populated with
    /// farm details. The editor is always made visible after calling this method.</remarks>
    /// <param name="mode">A string that determines the editor mode. Specify "Edit" (case-insensitive) to populate the editor with the
    /// selected farm's details for editing; otherwise, the editor is prepared for creating a new farm.</param>
    private void ShowFarmEditor(string? mode)
    {
        if (string.Equals(mode, "Edit", StringComparison.OrdinalIgnoreCase))
        {
            if (SelectedFarm == null)
            {
                return;
            }

            _editingFarmId = SelectedFarm.FarmId;
            PopulateFormFromFarm(SelectedFarm);
            IsFarmEditMode = true;
        }
        else
        {
            _editingFarmId = null;
            ResetFarmForm();
            IsFarmEditMode = false;
        }

        IsFarmEditorVisible = true;
    }
        
    private void HideFarmEditor()
    {
        IsFarmEditorVisible = false;
        _editingFarmId = null;
        ResetFarmForm();
    }

    /// <summary>
    /// Saves the current farm registration asynchronously if all required fields are valid.
    /// </summary>
    /// <remarks>If the save operation is successful, an informational message is displayed and the farm
    /// editor is hidden. If an error occurs during saving, an error message is displayed. The method does not perform
    /// any action if the farm data is not valid for saving.</remarks>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    private async Task SaveFarmAsync()
    {
        if (!CanSaveFarm())
        {
            return;
        }

        await ExecuteWithBusyStateAsync(async () =>
        {
            FarmRegistrationRequest request = BuildFarmRegistrationRequest();

            await _natureCheckCaseService.SaveFarmAsync(request);
            _appMessageService.AddInfoMessage(_editingFarmId.HasValue ? "Gården er opdateret." : "Ny gård er tilføjet.");

            HideFarmEditor();
            await EnsureAssignmentDataAsync(forceReload: true);
        });
    }

    private async Task DeleteSelectedFarmAsync()
    {
        if (!CanDeleteFarm())
        {
            return;
        }

        bool confirmed = await ContentDialogHelper.ShowConfirmationAsync(
            App.MainWindowXamlRoot,
            "Bekræft sletning",
            $"Er du sikker på at du vil slette {SelectedFarm?.FarmName}?",
            "Slet",
            "Fortryd");

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithBusyStateAsync(async () =>
        {
            await _natureCheckCaseService.DeleteFarmAsync(SelectedFarm!.FarmId);
            _appMessageService.AddInfoMessage($"{SelectedFarm.FarmName} er slettet.");
            await EnsureAssignmentDataAsync(forceReload: true);
        });
    }

    private async Task ExecuteWithBusyStateAsync(Func<Task> operation)
    {
        using IDisposable loading = _statusInfoServices.BeginLoadingOrSaving();

        try
        {
            IsBusy = true;
            await operation();
        }
        catch (Exception ex)
        {
            _appMessageService.AddErrorMessage(ex.Message);
            await ContentDialogHelper.ShowInfoAsync(App.MainWindowXamlRoot, "Fejl", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region CanXXX Command 
    private bool CanAssignCase() => !IsBusy && SelectedFarm != null && SelectedConsultant != null;
    private bool CanShowFarmEditor(string? mode) => string.Equals(mode, "Add", StringComparison.OrdinalIgnoreCase) || SelectedFarm != null;
    private bool CanSaveFarm() => !IsBusy && IsFarmFormValid();
    private bool CanDeleteFarm() => !IsBusy && SelectedFarm != null;
    #endregion

    #region Helpers
    private void UpdateFarms(IReadOnlyList<FarmAssignmentOverviewDto> farms)
    {
        _allFarms.Clear();
        foreach (FarmAssignmentOverviewDto dto in farms)
        {
            _allFarms.Add(new AssignableFarmViewModel(dto));
        }

        ApplyFilters();
        SelectedFarm = FilteredFarms.FirstOrDefault();
    }

    private void UpdateConsultants(IEnumerable<Person> consultants)
    {
        Consultants.Clear();
        foreach (Person person in consultants)
        {
            Consultants.Add(person);
        }

        if (Consultants.Any())
        {
            SelectedConsultant ??= Consultants.First();
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<AssignableFarmViewModel> query = _allFarms.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FarmSearchText))
        {
            string search = FarmSearchText.Trim().ToLowerInvariant();
            query = query.Where(f => f.SearchContent.Contains(search));
        }

        query = _currentStatusFilter switch
        {
            FarmStatusFilter.Assigned => query.Where(f => f.HasActiveCase),
            FarmStatusFilter.Unassigned => query.Where(f => !f.HasActiveCase),
            _ => query
        };

        SyncCollection(FilteredFarms, query);
        if (SelectedFarm != null && !FilteredFarms.Contains(SelectedFarm))
        {
            SelectedFarm = FilteredFarms.FirstOrDefault();
        }
    }

    private void PopulateFormFromFarm(AssignableFarmViewModel farm)
    {
        FarmForm.Name = farm.FarmName;
        FarmForm.Cvr = farm.Cvr;
        FarmForm.Street = farm.Street;
        FarmForm.City = farm.City;
        FarmForm.PostalCode = farm.PostalCode;
        FarmForm.Country = string.IsNullOrWhiteSpace(farm.Country) ? "Danmark" : farm.Country;
        FarmForm.OwnerFirstName = farm.OwnerFirstName;
        FarmForm.OwnerLastName = farm.OwnerLastName;
        FarmForm.OwnerEmail = farm.OwnerEmail;
    }

    private FarmRegistrationRequest BuildFarmRegistrationRequest()
    {
        return new FarmRegistrationRequest
        {
            FarmId = _editingFarmId,
            FarmName = FarmForm.Name,
            Cvr = FarmForm.Cvr,
            Street = FarmForm.Street,
            City = FarmForm.City,
            PostalCode = FarmForm.PostalCode,
            Country = FarmForm.Country,
            OwnerFirstName = FarmForm.OwnerFirstName,
            OwnerLastName = FarmForm.OwnerLastName,
            OwnerEmail = FarmForm.OwnerEmail
        };
    }

    private bool IsFarmFormValid()
    {
        return !string.IsNullOrWhiteSpace(FarmForm.Name)
            && !string.IsNullOrWhiteSpace(FarmForm.Cvr)
            && !string.IsNullOrWhiteSpace(FarmForm.OwnerFirstName)
            && !string.IsNullOrWhiteSpace(FarmForm.OwnerLastName)
            && !string.IsNullOrWhiteSpace(FarmForm.OwnerEmail);
    }

    private static void SyncCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source)
        {
            target.Add(item);
        }
    }

    private void OnFarmFormPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FarmFormModel.Name):
            case nameof(FarmFormModel.Cvr):
            case nameof(FarmFormModel.OwnerFirstName):
            case nameof(FarmFormModel.OwnerLastName):
            case nameof(FarmFormModel.OwnerEmail):
                SaveFarmCommand.RaiseCanExecuteChanged();
                break;
        }
    }

    private void ResetFarmForm() => FarmForm.Reset();

    public class FarmFormModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _cvr = string.Empty;
        private string _street = string.Empty;
        private string _city = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = "Danmark";
        private string _ownerFirstName = string.Empty;
        private string _ownerLastName = string.Empty;
        private string _ownerEmail = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Cvr
        {
            get => _cvr;
            set
            {
                if (_cvr == value) return;
                _cvr = value;
                OnPropertyChanged();
            }
        }

        public string Street
        {
            get => _street;
            set
            {
                if (_street == value) return;
                _street = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (_city == value) return;
                _city = value;
                OnPropertyChanged();
            }
        }

        public string PostalCode
        {
            get => _postalCode;
            set
            {
                if (_postalCode == value) return;
                _postalCode = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (_country == value) return;
                _country = value;
                OnPropertyChanged();
            }
        }

        public string OwnerFirstName
        {
            get => _ownerFirstName;
            set
            {
                if (_ownerFirstName == value) return;
                _ownerFirstName = value;
                OnPropertyChanged();
            }
        }

        public string OwnerLastName
        {
            get => _ownerLastName;
            set
            {
                if (_ownerLastName == value) return;
                _ownerLastName = value;
                OnPropertyChanged();
            }
        }

        public string OwnerEmail
        {
            get => _ownerEmail;
            set
            {
                if (_ownerEmail == value) return;
                _ownerEmail = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Name = string.Empty;
            Cvr = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            Country = "Danmark";
            OwnerFirstName = string.Empty;
            OwnerLastName = string.Empty;
            OwnerEmail = string.Empty;
        }
    }

    private enum FarmStatusFilter
    {
        All,
        Assigned,
        Unassigned
    }
    #endregion
}

