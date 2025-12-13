using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls
{
    public class ConsultantCreateNatureCheckViewModel : ViewModelBase
    {
        private readonly ICreateNatureCheck _createNatureCheckService;

        public ObservableCollection<Farm> Farms { get; } = new();
        public ObservableCollection<Person> Persons { get; } = new();

        // This is what the table binds to
        public ObservableCollection<NatureCheckCase> NatureChecks { get; } = new();

        private Farm? _selectedFarm;
        public Farm? SelectedFarm
        {
            get => _selectedFarm;
            set
            {
                if (_selectedFarm == value)
                    return;

                _selectedFarm = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(FarmNameDisplay));
                OnPropertyChanged(nameof(FarmCVRDisplay));
                OnPropertyChanged(nameof(FarmAddressDisplay));

                UpdateCommandsCanExecute();
            }
        }

        private Person? _selectedConsultant;
        public Person? SelectedConsultant
        {
            get => _selectedConsultant;
            set
            {
                if (_selectedConsultant == value)
                    return;

                _selectedConsultant = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(ConsultantFirstNameDisplay));
                OnPropertyChanged(nameof(ConsultantLastNameDisplay));

                UpdateCommandsCanExecute();
            }
        }

        // Date for the nature check
        private DateTimeOffset? _selectedDate = DateTimeOffset.Now;
        public DateTimeOffset? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value)
                    return;

                _selectedDate = value;
                OnPropertyChanged();

                UpdateCommandsCanExecute();
            }
        }

        // Time for the nature check
        private TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (_selectedTime == value)
                    return;

                _selectedTime = value;
                OnPropertyChanged();

                UpdateCommandsCanExecute();
            }
        }

        // ---- Display-only properties for the XAML bindings ----

        public string FarmNameDisplay => SelectedFarm?.Name ?? string.Empty;

        public string FarmCVRDisplay => SelectedFarm?.CVR ?? string.Empty;

        public string FarmAddressDisplay
        {
            get
            {
                if (SelectedFarm?.Address is null)
                    return string.Empty;

                return $"{SelectedFarm.Address.Street}, {SelectedFarm.Address.PostalCode} {SelectedFarm.Address.City}";
            }
        }

        public string ConsultantFirstNameDisplay => SelectedConsultant?.FirstName ?? string.Empty;

        public string ConsultantLastNameDisplay => SelectedConsultant?.LastName ?? string.Empty;

        // ---- Commands ----

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateNatureCheckCommand { get; }

        public ConsultantCreateNatureCheckViewModel(ICreateNatureCheck createNatureCheckService)
        {
            _createNatureCheckService = createNatureCheckService
                ?? throw new ArgumentNullException(nameof(createNatureCheckService));

            // make Refresh load everything, including the table
            RefreshCommand = new RelayCommand(
                execute: () => _ = LoadAsync(),
                canExecute: () => true);

            CreateNatureCheckCommand = new RelayCommand(
                execute: () => _ = CreateNatureCheckAsync(),
                canExecute: CanCreateNatureCheck);

            // initial load should also load NatureChecks
            _ = LoadAsync();
        }

        private bool CanCreateNatureCheck()
        {
            return SelectedFarm != null
                && SelectedConsultant != null
                && SelectedDate != null;
        }

        private void UpdateCommandsCanExecute()
        {
            CreateNatureCheckCommand.RaiseCanExecuteChanged();
        }

        private async Task LoadAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadAsync: started");

                var farms = await _createNatureCheckService.GetFarmsAsync().ConfigureAwait(true);
                System.Diagnostics.Debug.WriteLine($"LoadAsync: GetFarmsAsync returned {farms?.Count ?? 0} items");
                Farms.Clear();
                foreach (var f in farms)
                    Farms.Add(f);

                var persons = await _createNatureCheckService.GetPersonsAsync().ConfigureAwait(true);
                System.Diagnostics.Debug.WriteLine($"LoadAsync: GetPersonsAsync returned {persons?.Count ?? 0} items");
                Persons.Clear();
                foreach (var p in persons)
                    Persons.Add(p);

                // REAL DB call again
                var checks = await _createNatureCheckService.GetNatureChecksAsync().ConfigureAwait(true);
                System.Diagnostics.Debug.WriteLine($"LoadAsync: GetNatureChecksAsync returned {checks?.Count ?? 0} items");
                NatureChecks.Clear();
                foreach (var c in checks)
                    NatureChecks.Add(c);

                UpdateCommandsCanExecute();

                System.Diagnostics.Debug.WriteLine($"LoadAsync: finished. Collections sizes: Farms={Farms.Count}, Persons={Persons.Count}, NatureChecks={NatureChecks.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadAsync FAILED: " + ex);
            }
        }

        private async Task CreateNatureCheckAsync()
        {
            if (!CanCreateNatureCheck())
                return;

            var date = SelectedDate ?? DateTimeOffset.Now;
            var dateTimeToUse = date.Date + SelectedTime;

            var request = new CreateNatureCheck
            {
                NatureCheckId = Guid.Empty,
                FarmId = SelectedFarm!.Id,
                PersonId = SelectedConsultant!.Id,
                FarmName = SelectedFarm.Name,
                FarmCVR = int.TryParse(SelectedFarm.CVR, out var cvr) ? cvr : 0,
                FarmAddress = SelectedFarm.Address != null
                    ? $"{SelectedFarm.Address.Street}, {SelectedFarm.Address.PostalCode} {SelectedFarm.Address.City}"
                    : string.Empty,
                ConsultantFirstName = SelectedConsultant.FirstName,
                ConsultantLastName = SelectedConsultant.LastName,
                DateTime = dateTimeToUse
            };

            await _createNatureCheckService.CreateNatureCheckAsync(request).ConfigureAwait(true);

            await LoadAsync().ConfigureAwait(true);
        }
    }
}
