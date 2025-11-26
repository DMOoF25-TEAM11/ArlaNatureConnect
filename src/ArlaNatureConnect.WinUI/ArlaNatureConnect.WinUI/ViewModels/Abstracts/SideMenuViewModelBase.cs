using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;

using System.Collections.ObjectModel;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base view-model for side-menu view-models.
/// Inherits ListViewModelBase specialized for Person repository/entity.
/// Exposes AvailablePersons, SelectedPerson and a NavigationCommand usable by side-menu views.
/// </summary>
public abstract partial class SideMenuViewModelBase : ListViewModelBase<IPersonRepository, Person>
{
    #region Fields
    private Person? _selectedPerson;
    private bool _isLoading;
    private INavigationViewModelBase? _navigationViewModel;
    #endregion

    #region Constructors

    /// <summary>
    /// Parameterless ctor for design-time and tests.
    /// </summary>
    protected SideMenuViewModelBase(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository)
        : base(statusInfoServices, appMessageService, repository)
    {
        NavigationCommand = new RelayCommand<object>(OnNavigate, CanNavigate);
        LogoutCommand = new RelayCommand<object>(OnLogout, CanLogout);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Persons available for selection in the side-menu.
    /// </summary>
    public ObservableCollection<Person> AvailablePersons { get; set; } = [];


    /// <summary>
    /// Currently selected person in the side-menu.
    /// </summary>
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            _selectedPerson = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Navigation command that side-menu buttons bind to. Default implementation is a no-op.
    /// Derived view-models may replace this with a command that performs real navigation.
    /// </summary>
    public RelayCommand<object>? NavigationCommand { get; protected set; }
    public RelayCommand<object>? LogoutCommand { get; protected set; }
    #endregion

    #region Load handlers
    public async Task LoadAvailablePersonsAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            // Ensure property change happens on UI thread when possible
            TrySetAvailablePersons([]);
            return;
        }

        IEnumerable<Person> persons = await Repository.GetPersonsByRoleAsync(role);

        // Set AvailablePersons on UI thread when possible so bindings update correctly in WinUI
        TrySetAvailablePersons(persons);
    }
    #endregion

    #region Commands
    /// <summary>
    /// Default navigation handler (no-op). Override in derived classes to perform real navigation.
    /// </summary>
    /// <param name="parameter"></param>
    protected virtual void OnNavigate(object? parameter)
    {
        // no-op in base
    }

    private bool CanNavigate(object? parameter)
    {
        if (parameter == null) return false;
        if (parameter is string s) return !string.IsNullOrWhiteSpace(s);
        if (parameter is System.Delegate) return true;
        return true;
    }

    protected virtual void OnLogout(object? parameter)
    {

    }
    private bool CanLogout(object? parameter)
    {
        return true;
    }
    #endregion

    #region Helpers
    // Add helper methods here if needed
    public void SetHostPageViewModel(object? vm) { _navigationViewModel = vm as INavigationViewModelBase; }

    private void TrySetAvailablePersons(IEnumerable<Person> persons)
    {
        try
        {
            List<Person> availablePersons = [.. persons];
            AvailablePersons.Clear();
            foreach (Person person in availablePersons)
            {
                AvailablePersons.Add(person);
            }
        }
        catch
        {
            // last resort: set directly
            AvailablePersons.Clear();
            foreach (Person person in persons)
            {
                AvailablePersons.Add(person);
            }
        }
    }
    #endregion
}
