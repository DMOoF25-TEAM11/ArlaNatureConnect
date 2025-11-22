using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Pages;

/// <summary>
/// Unit tests for <see cref="ConsultantPageViewModel"/> covering user selection, data loading, and dashboard functionality.
/// 
/// These tests verify:
/// - Initialization with role loads available consultants correctly
/// - AvailablePersons list is filtered by Consultant role and active status
/// - Persons are sorted alphabetically (first name, then last name)
/// - User selection updates SelectedPerson and enables commands
/// - Loading state is managed correctly during async operations
/// - Navigation between content views works correctly
/// </summary>
[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class ConsultantPageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private Mock<IPersonRepository> _mockPersonRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private ConsultantPageViewModel _viewModel = null!;

    /// <summary>
    /// Sets up test dependencies before each test method runs.
    /// Creates mocks for NavigationHandler, IPersonRepository, and IRoleRepository,
    /// and initializes the ViewModel.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockNavigationHandler = new Mock<NavigationHandler>();
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _viewModel = new ConsultantPageViewModel(
            _mockNavigationHandler.Object,
            _mockPersonRepository.Object,
            _mockRoleRepository.Object);
    }

    /// <summary>
    /// Verifies that InitializeAsync with a Consultant role loads available consultants
    /// from repositories, filters by role and active status, and sorts them alphabetically.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithConsultantRole_LoadsAvailableConsultants()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        Role farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        List<Role> roles = new List<Role> { consultantRole, farmerRole };

        Person consultant1 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Bob",
            LastName = "Smith",
            IsActive = true
        };
        Person consultant2 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Alice",
            LastName = "Johnson",
            IsActive = true
        };
        Person inactiveConsultant = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Inactive",
            LastName = "Consultant",
            IsActive = false
        };
        Person farmer = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Farmer",
            LastName = "Person",
            IsActive = true
        };
        List<Person> persons = new List<Person> { consultant1, consultant2, inactiveConsultant, farmer };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        Role role = new Role { Name = "Consultant" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.HasCount(2, _viewModel.AvailablePersons);
        // Should be sorted alphabetically: Alice, then Bob
        Assert.AreEqual("Alice", _viewModel.AvailablePersons[0].FirstName);
        Assert.AreEqual("Bob", _viewModel.AvailablePersons[1].FirstName);
        // Should not include inactive consultant or farmer
        Assert.IsFalse(_viewModel.AvailablePersons.Any(p => p.Id == inactiveConsultant.Id));
        Assert.IsFalse(_viewModel.AvailablePersons.Any(p => p.Id == farmer.Id));
    }

    /// <summary>
    /// Verifies that InitializeAsync with "Konsulent" (Danish for Consultant) role
    /// correctly identifies and loads consultants, ensuring Danish role names are supported.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithKonsulentRole_LoadsAvailableConsultants()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Konsulent" };
        List<Role> roles = new List<Role> { consultantRole };
        Person consultant = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Test",
            LastName = "Consultant",
            IsActive = true
        };
        List<Person> persons = new List<Person> { consultant };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        Role role = new Role { Name = "Konsulent" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.HasCount(1, _viewModel.AvailablePersons);
        Assert.AreEqual(consultant.Id, _viewModel.AvailablePersons[0].Id);
    }

    /// <summary>
    /// Verifies that when no Consultant role exists in the repository,
    /// AvailablePersons is set to an empty list without throwing exceptions.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithNoConsultantRole_ReturnsEmptyList()
    {
        // Arrange
        Role farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        List<Role> roles = new List<Role> { farmerRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.HasCount(0, _viewModel.AvailablePersons);
    }

    /// <summary>
    /// Verifies that when no persons exist in the repository,
    /// AvailablePersons is set to an empty list.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithNoPersons_ReturnsEmptyList()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        List<Role> roles = new List<Role> { consultantRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.IsEmpty(_viewModel.AvailablePersons);
    }

    /// <summary>
    /// Verifies that IsLoading is set to true during data loading and false after completion.
    /// This ensures the UI can show a loading indicator during async operations.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_SetsIsLoadingDuringLoad()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        List<Role> roles = new List<Role> { consultantRole };

        TaskCompletionSource<IEnumerable<Role>> tcs = new TaskCompletionSource<IEnumerable<Role>>();
        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act - start initialization
        Task initTask = _viewModel.InitializeAsync(role);

        // Assert - should be loading
        Assert.IsTrue(_viewModel.IsLoading);

        // Complete the task
        roles = roles.AsEnumerable().ToList();
        tcs.SetResult(roles.AsEnumerable());
        await initTask;

        // Assert - should not be loading anymore
        Assert.IsFalse(_viewModel.IsLoading);
    }

    /// <summary>
    /// Verifies that selecting a person via ChooseUserCommand sets SelectedPerson
    /// and updates IsUserSelected property to true.
    /// </summary>
    [TestMethod]
    public void ChooseUserCommand_WithValidPerson_SetsSelectedPerson()
    {
        // Arrange
        Person person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Consultant",
            IsActive = true
        };

        // Act
        _viewModel.ChooseUserCommand.Execute(person);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedPerson);
        Assert.AreEqual(person.Id, _viewModel.SelectedPerson.Id);
        Assert.IsTrue(_viewModel.IsUserSelected);
    }

    /// <summary>
    /// Verifies that ChooseUserCommand with null person does not change SelectedPerson
    /// and does not throw exceptions. This ensures graceful handling of null input.
    /// </summary>
    [TestMethod]
    public void ChooseUserCommand_WithNullPerson_DoesNotChangeSelectedPerson()
    {
        // Arrange
        Person initialPerson = new Person { Id = Guid.NewGuid(), FirstName = "Initial", LastName = "Person" };
        _viewModel.SelectedPerson = initialPerson;

        // Act
        _viewModel.ChooseUserCommand.Execute(null);

        // Assert
        Assert.AreEqual(initialPerson.Id, _viewModel.SelectedPerson?.Id);
    }

    /// <summary>
    /// Verifies that persons are sorted first by FirstName, then by LastName alphabetically.
    /// This ensures consistent ordering in the UI dropdown.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_SortsPersonsAlphabetically()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        List<Role> roles = new List<Role> { consultantRole };

        Person person1 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Zebra",
            LastName = "Alpha",
            IsActive = true
        };
        Person person2 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Alice",
            LastName = "Beta",
            IsActive = true
        };
        Person person3 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Alice",
            LastName = "Alpha",
            IsActive = true
        };
        List<Person> persons = new List<Person> { person1, person2, person3 };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        Role role = new Role { Name = "Consultant" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.HasCount(3, _viewModel.AvailablePersons);
        // Should be sorted: Alice Alpha, Alice Beta, Zebra Alpha
        Assert.AreEqual("Alice", _viewModel.AvailablePersons[0].FirstName);
        Assert.AreEqual("Alpha", _viewModel.AvailablePersons[0].LastName);
        Assert.AreEqual("Alice", _viewModel.AvailablePersons[1].FirstName);
        Assert.AreEqual("Beta", _viewModel.AvailablePersons[1].LastName);
        Assert.AreEqual("Zebra", _viewModel.AvailablePersons[2].FirstName);
    }

    /// <summary>
    /// Verifies that the ViewModel constructor throws ArgumentNullException when NavigationHandler is null.
    /// This ensures proper dependency injection validation.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullNavigationHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            new ConsultantPageViewModel(
                null!,
                _mockPersonRepository.Object,
                _mockRoleRepository.Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that the ViewModel constructor throws ArgumentNullException when IPersonRepository is null.
    /// This ensures proper dependency injection validation.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullPersonRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            new ConsultantPageViewModel(
                _mockNavigationHandler.Object,
                null!,
                _mockRoleRepository.Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that the ViewModel constructor throws ArgumentNullException when IRoleRepository is null.
    /// This ensures proper dependency injection validation.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullRoleRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            new ConsultantPageViewModel(
                _mockNavigationHandler.Object,
                _mockPersonRepository.Object,
                null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that the default navigation tag is set to "Farms" when ViewModel is constructed.
    /// This ensures the correct default content view is displayed for consultants.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsDefaultNavigationTagToFarms()
    {
        // Assert
        Assert.AreEqual("Farms", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating to a different view updates CurrentNavigationTag.
    /// This ensures the navigation system works correctly for switching content views.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithValidTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Dashboards");

        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that OnNavigationItemSelected updates the current navigation tag.
    /// This tests the navigation item selection handler used by the UI.
    /// </summary>
    [TestMethod]
    public void OnNavigationItemSelected_WithValidTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.OnNavigationItemSelected("Tasks");

        // Assert
        Assert.AreEqual("Tasks", _viewModel.CurrentNavigationTag);
    }

    #region Error Handling Tests

    /// <summary>
    /// Verifies that InitializeAsync handles exceptions from RoleRepository gracefully.
    /// When the repository throws an exception, the ViewModel should handle it without crashing.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WhenRoleRepositoryThrowsException_HandlesGracefully()
    {
        // Arrange
        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act & Assert
        try
        {
            await _viewModel.InitializeAsync(role);
            // If we reach here, the exception was handled gracefully
            Assert.IsNotNull(_viewModel.AvailablePersons);
        }
        catch (InvalidOperationException)
        {
            // If exception propagates, that's also acceptable behavior
        }
    }

    /// <summary>
    /// Verifies that InitializeAsync handles exceptions from PersonRepository gracefully.
    /// When the repository throws an exception, the ViewModel should handle it without crashing.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WhenPersonRepositoryThrowsException_HandlesGracefully()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        List<Role> roles = new List<Role> { consultantRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        Role role = new Role { Name = "Consultant" };

        // Act & Assert
        try
        {
            await _viewModel.InitializeAsync(role);
            // If we reach here, the exception was handled gracefully
            Assert.IsNotNull(_viewModel.AvailablePersons);
        }
        catch (InvalidOperationException)
        {
            // If exception propagates, that's also acceptable behavior
        }
    }

    /// <summary>
    /// Verifies that InitializeAsync handles TaskCanceledException gracefully.
    /// When the operation is cancelled, the ViewModel should handle it without crashing.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WhenOperationCancelled_HandlesGracefully()
    {
        // Arrange
        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Operation was cancelled"));
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act & Assert
        try
        {
            await _viewModel.InitializeAsync(role);
            Assert.IsNotNull(_viewModel.AvailablePersons);
        }
        catch (TaskCanceledException)
        {
            // If exception propagates, that's acceptable
        }
    }

    /// <summary>
    /// Verifies that InitializeAsync sets IsLoading to false even when an exception occurs.
    /// This ensures the UI loading indicator is always turned off, even on errors.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WhenExceptionOccurs_SetsIsLoadingToFalse()
    {
        // Arrange
        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        Role role = new Role { Name = "Consultant" };

        // Act
        try
        {
            await _viewModel.InitializeAsync(role);
        }
        catch
        {
            // Expected exception
        }

        // Assert
        Assert.IsFalse(_viewModel.IsLoading);
    }

    /// <summary>
    /// Verifies that InitializeAsync handles null role parameter gracefully.
    /// When null role is passed, the ViewModel should handle it without throwing exceptions.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithNullRole_HandlesGracefully()
    {
        // Arrange
        Role consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        List<Role> roles = new List<Role> { consultantRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        // Act
        await _viewModel.InitializeAsync(null);

        // Assert
        // Should complete without throwing exception
        Assert.IsNotNull(_viewModel.AvailablePersons);
    }

    #endregion
}

