using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Commands;

namespace ArlaNatureConnect.WinUI.ViewModels;

public class NatureCheckViewModel : ViewModelBase
{
    private readonly ICreateNatureCheck _createNatureCheckService;

    public ObservableCollection<Farm> Farms { get; } = new();
    public ObservableCollection<Person> Persons { get; } = new();
    public ObservableCollection<NatureCheckCase> NatureChecks { get; } = new();

    private Farm? _selectedFarm;
    public Farm? SelectedFarm
    {
        get => _selectedFarm;
        set
        {
            if (_selectedFarm == value) return;
            _selectedFarm = value;
            OnPropertyChanged(); // SelectedFarm

            if (value != null)
            {
                NewNatureCheck.FarmId = value.Id;
                NewNatureCheck.FarmName = value.Name;

                if (int.TryParse(value.CVR, out var cvr))
                {
                    NewNatureCheck.FarmCVR = cvr;
                }
                else
                {
                    NewNatureCheck.FarmCVR = 0;
                }

                NewNatureCheck.FarmAddress = value.Address?.ToString() ?? string.Empty;

                OnPropertyChanged(nameof(NewNatureCheck));
            }

            CreateNatureCheckCommand.RaiseCanExecuteChanged();
        }
    }

    private Person? _selectedPerson;
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            if (_selectedPerson == value) return;
            _selectedPerson = value;
            OnPropertyChanged();

            if (value != null)
            {
                NewNatureCheck.PersonId = value.Id;
                NewNatureCheck.ConsultantFirstName = value.FirstName;
                NewNatureCheck.ConsultantLastName = value.LastName;

                OnPropertyChanged(nameof(NewNatureCheck));
            }

            CreateNatureCheckCommand.RaiseCanExecuteChanged();
        }
    }

    private CreateNatureCheck _newNatureCheck = new();
    public CreateNatureCheck NewNatureCheck
    {
        get => _newNatureCheck;
        set
        {
            if (_newNatureCheck == value) return;
            _newNatureCheck = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand LoadCommand { get; }
    public RelayCommand CreateNatureCheckCommand { get; }

    public NatureCheckViewModel(ICreateNatureCheck createNatureCheckService)
    {
        _createNatureCheckService = createNatureCheckService;

        LoadCommand = new RelayCommand(
            execute: async () => await LoadAsync()
        );

        CreateNatureCheckCommand = new RelayCommand(
            execute: async () => await CreateNatureCheckAsync(),
            canExecute: CanCreateNatureCheck
        );

        NewNatureCheck.DateTime = DateTime.Now;
    }

    private bool CanCreateNatureCheck()
        => SelectedFarm != null && SelectedPerson != null;

    public async Task LoadAsync()
    {
        var farms = await _createNatureCheckService.GetFarmsAsync();
        Farms.Clear();
        foreach (var farm in farms)
            Farms.Add(farm);

        var persons = await _createNatureCheckService.GetPersonsAsync();
        Persons.Clear();
        foreach (var person in persons)
            Persons.Add(person);

        var checks = await _createNatureCheckService.GetNatureChecksAsync();
        NatureChecks.Clear();
        foreach (var check in checks)
            NatureChecks.Add(check);
    }

    private async Task CreateNatureCheckAsync()
    {
        NewNatureCheck.NatureCheckId = Guid.NewGuid();
        NewNatureCheck.DateTime = DateTime.Now;

        var id = await _createNatureCheckService.CreateNatureCheckAsync(NewNatureCheck);

        // Reload list
        var checks = await _createNatureCheckService.GetNatureChecksAsync();
        NatureChecks.Clear();
        foreach (var check in checks)
            NatureChecks.Add(check);

        // Reset form
        NewNatureCheck = new CreateNatureCheck
        {
            DateTime = DateTime.Now
        };
        SelectedFarm = null;
        SelectedPerson = null;
    }
}
