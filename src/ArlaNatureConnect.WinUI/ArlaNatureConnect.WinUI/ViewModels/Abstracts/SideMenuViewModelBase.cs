using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base view-model for side-menu view-models.
/// Inherits ListViewModelBase specialized for Person repository/entity.
/// Exposes AvailablePersons, SelectedPerson and a NavigationCommand usable by side-menu views.
/// </summary>
public abstract partial class SideMenuViewModelBase : ListViewModelBase<IPersonRepository, Person>
{
    #region Types
    // simple model for dynamic navigation buttons
    public sealed class NavItem : INotifyPropertyChanged
    {
        public NavItem(string label, ICommand? command = null)
        {
            Label = label;
            Command = command;
        }

        public string Label { get; }
        public ICommand? Command { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion


    #region Fields
    private Person? _selectedPerson;
    private bool _isLoading;
    protected INavigationViewModelBase? _navigationViewModel;
    private IPersonRepository _repository;
    #endregion

    #region Constructors

    /// <summary>
    /// Parameterless ctor for design-time and tests.
    /// </summary>
    protected SideMenuViewModelBase(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository
        )
        : base(statusInfoServices, appMessageService, repository)
    {
        _repository = repository;
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
            if (value != null)
            {
                RelayCommand<Person>? chooseCommand = _navigationViewModel?.ChooseUserCommand;
                if (chooseCommand != null && chooseCommand.CanExecute(value))
                {
                    chooseCommand.Execute(value);
                }
            }
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

        IEnumerable<Person> persons = await _repository.GetPersonsByRoleAsync(role);

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
        try
        {
            ArgumentNullException.ThrowIfNull(parameter);
            if (_navigationViewModel == null)
                throw new InvalidOperationException("No host navigation view-model set.");


            // If parameter is a UserControl, set it directly as the host page content
            if (parameter is UserControl uc && _navigationViewModel is NavigationViewModelBase hostVm)
            {
                try { uc.DataContext = hostVm; } catch { }
                hostVm.GetType().GetProperty("CurrentContent")?.SetValue(hostVm, uc);
                return;
            }

            // If parameter is a factory that returns a control or element, invoke and assign
            if (parameter is Func<object?> factory && _navigationViewModel is NavigationViewModelBase hostVm2)
            {
                object? obj = null;
                try { obj = factory(); } catch { obj = null; }
                if (obj is FrameworkElement fe)
                {
                    try { fe.DataContext = hostVm2; } catch { }
                }
                if (obj is UserControl userControl)
                {
                    hostVm2.GetType().GetProperty("CurrentContent")?.SetValue(hostVm2, userControl);
                }
                return;
            }

            // Otherwise try forwarding to host navigation command if available
            if (_navigationViewModel is NavigationViewModelBase hostVm3)
            {
                ICommand? hostCmd = hostVm3.NavigationCommand as ICommand;
                if (hostCmd != null && hostCmd.CanExecute(parameter))
                {
                    hostCmd.Execute(parameter);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("OnNavigate failed", ex);
        }

        // No host available or unable to set content: remain a no-op.
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
