using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Commands;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Views.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.ViewModels.Abstracts;

/// <summary>
/// Base view-model for side-menu view-models.
/// Inherits <see cref="ListViewModelBase{IPersonRepository, Person}"/> specialized for the <see cref="Person"/> entity.
/// Exposes <see cref="AvailablePersons"/>, <see cref="SelectedPerson"/> and a <see cref="NavigationCommand"/> usable by side-menu views.
/// </summary>
/// <remarks>
/// How to use:
/// - Construct or resolve a concrete side-menu view-model and set it as the DataContext for the side-menu UserControl.
/// - Call <see cref="LoadAvailablePersonsAsync(string)"/> with the desired role to populate <see cref="AvailablePersons"/>.
/// - Bind <see cref="SelectedPerson"/> to the view's selection control and hook up <see cref="NavigationCommand"/> / <see cref="LogoutCommand"/> to buttons.
/// 
/// Why we have it:
/// - Centralizes side-menu behaviour (person loading, selection notification and navigation wiring) so pages can remain thin
///   and share consistent behaviour across roles (Farmer, Consultant, Administrator, etc.).
/// 
/// Example:
/// <example>
/// <code>
/// // In a page or DI-constructed view-model:
/// var sideVm = new FarmerPageSideMenuUCViewModel(statusService, appMsgService, personRepo);
/// mySideMenuControl.DataContext = sideVm;
/// await sideVm.LoadAvailablePersonsAsync("Farmer");
/// // Bindings will update and selecting a person will notify the host page view-model.
/// </code>
/// </example>
/// </remarks>
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
    private INavigationHandler _navigationHandler;
    #endregion

    #region Constructors

    /// <summary>
    /// Parameterless ctor for design-time and tests.
    /// </summary>
    protected SideMenuViewModelBase(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository,
        INavigationHandler navigationHandler
        )
        : base(statusInfoServices, appMessageService, repository)
    {
        _repository = repository;
        _navigationHandler = navigationHandler;
        NavigationCommand = new RelayCommand<object>(OnNavigate, CanNavigate);
        LogoutCommand = new RelayCommand<object>(OnLogout, CanLogout);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Persons available for selection in the side-menu.
    /// </summary>
    public ObservableCollection<Person> AvailablePersons { get; set; } = new();


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
            TrySetAvailablePersons(Enumerable.Empty<Person>());
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
        try
        {
            // Clear selected person and any local state
            SelectedPerson = null;

            // Try to resolve the NavigationHandler from the host DI container and navigate to LoginPage
            NavigationHandler? navigationHandler = App.HostInstance?.Services.GetService<NavigationHandler>();
            if (navigationHandler != null)
            {
                navigationHandler.Navigate(typeof(LoginPage));
                return;
            }

            // Fallback: attempt to get NavigationHandler via GetRequiredService (throws if missing)
            try
            {
                NavigationHandler? required = App.HostInstance?.Services.GetRequiredService<NavigationHandler>();
                required?.Navigate(typeof(LoginPage));
            }
            catch
            {
                // swallow - best effort navigation
            }
        }
        catch
        {
            // swallow - logout is best-effort
        }
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
            // Inline comment: materialize enumerable to avoid deferred execution and potential collection-modification issues
            List<Person> availablePersons = persons.ToList();
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
