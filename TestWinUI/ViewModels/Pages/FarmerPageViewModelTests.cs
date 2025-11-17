using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Moq;

namespace TestWinUI.ViewModels.Pages;

/// <summary>
/// Unit tests for <see cref="FarmerPageViewModel"/> covering user selection, data loading, and dashboard functionality.
/// 
/// These tests verify:
/// - Initialization with role loads available farmers correctly
/// - AvailablePersons list is filtered by Farmer role and active status
/// - Persons are sorted alphabetically (first name, then last name)
/// - User selection updates SelectedPerson and enables commands
/// - Loading state is managed correctly during async operations
/// - Navigation between content views works correctly
/// </summary>
[TestClass]
public sealed class FarmerPageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private Mock<IPersonRepository> _mockPersonRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private FarmerPageViewModel _viewModel = null!;

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
        _viewModel = new FarmerPageViewModel(
            _mockNavigationHandler.Object,
            _mockPersonRepository.Object,
            _mockRoleRepository.Object);
    }

    /// <summary>
    /// Verifies that InitializeAsync with a Farmer role loads available farmers
    /// from repositories, filters by role and active status, and sorts them alphabetically.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithFarmerRole_LoadsAvailableFarmers()
    {
        // Arrange
        var farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        var consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        var roles = new List<Role> { farmerRole, consultantRole };

        var farmer1 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Bob",
            LastName = "Smith",
            IsActive = true
        };
        var farmer2 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Alice",
            LastName = "Johnson",
            IsActive = true
        };
        var inactiveFarmer = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Inactive",
            LastName = "Farmer",
            IsActive = false
        };
        var consultant = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = consultantRole.Id,
            FirstName = "Consultant",
            LastName = "Person",
            IsActive = true
        };
        var persons = new List<Person> { farmer1, farmer2, inactiveFarmer, consultant };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        var role = new Role { Name = "Farmer" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.AreEqual(2, _viewModel.AvailablePersons.Count);
        // Should be sorted alphabetically: Alice, then Bob
        Assert.AreEqual("Alice", _viewModel.AvailablePersons[0].FirstName);
        Assert.AreEqual("Bob", _viewModel.AvailablePersons[1].FirstName);
        // Should not include inactive farmer or consultant
        Assert.IsFalse(_viewModel.AvailablePersons.Any(p => p.Id == inactiveFarmer.Id));
        Assert.IsFalse(_viewModel.AvailablePersons.Any(p => p.Id == consultant.Id));
    }

    /// <summary>
    /// Verifies that InitializeAsync with "Landmand" (Danish for Farmer) role
    /// correctly identifies and loads farmers, ensuring Danish role names are supported.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithLandmandRole_LoadsAvailableFarmers()
    {
        // Arrange
        var farmerRole = new Role { Id = Guid.NewGuid(), Name = "Landmand" };
        var roles = new List<Role> { farmerRole };
        var farmer = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Test",
            LastName = "Farmer",
            IsActive = true
        };
        var persons = new List<Person> { farmer };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        var role = new Role { Name = "Landmand" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.AreEqual(1, _viewModel.AvailablePersons.Count);
        Assert.AreEqual(farmer.Id, _viewModel.AvailablePersons[0].Id);
    }

    /// <summary>
    /// Verifies that when no Farmer role exists in the repository,
    /// AvailablePersons is set to an empty list without throwing exceptions.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithNoFarmerRole_ReturnsEmptyList()
    {
        // Arrange
        var consultantRole = new Role { Id = Guid.NewGuid(), Name = "Consultant" };
        var roles = new List<Role> { consultantRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var role = new Role { Name = "Farmer" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.AreEqual(0, _viewModel.AvailablePersons.Count);
    }

    /// <summary>
    /// Verifies that when no persons exist in the repository,
    /// AvailablePersons is set to an empty list.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithNoPersons_ReturnsEmptyList()
    {
        // Arrange
        var farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        var roles = new List<Role> { farmerRole };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var role = new Role { Name = "Farmer" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.AreEqual(0, _viewModel.AvailablePersons.Count);
    }

    /// <summary>
    /// Verifies that IsLoading is set to true during data loading and false after completion.
    /// This ensures the UI can show a loading indicator during async operations.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_SetsIsLoadingDuringLoad()
    {
        // Arrange
        var farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        var roles = new List<Role> { farmerRole };

        var tcs = new TaskCompletionSource<IEnumerable<Role>>();
        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        var role = new Role { Name = "Farmer" };

        // Act - start initialization
        var initTask = _viewModel.InitializeAsync(role);

        // Assert - should be loading
        Assert.IsTrue(_viewModel.IsLoading);

        // Complete the task
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
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Farmer",
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
        var initialPerson = new Person { Id = Guid.NewGuid(), FirstName = "Initial", LastName = "Person" };
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
        var farmerRole = new Role { Id = Guid.NewGuid(), Name = "Farmer" };
        var roles = new List<Role> { farmerRole };

        var person1 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Zebra",
            LastName = "Alpha",
            IsActive = true
        };
        var person2 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Alice",
            LastName = "Beta",
            IsActive = true
        };
        var person3 = new Person
        {
            Id = Guid.NewGuid(),
            RoleId = farmerRole.Id,
            FirstName = "Alice",
            LastName = "Alpha",
            IsActive = true
        };
        var persons = new List<Person> { person1, person2, person3 };

        _mockRoleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _mockPersonRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        var role = new Role { Name = "Farmer" };

        // Act
        await _viewModel.InitializeAsync(role);

        // Assert
        Assert.AreEqual(3, _viewModel.AvailablePersons.Count);
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
            new FarmerPageViewModel(
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
            new FarmerPageViewModel(
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
            new FarmerPageViewModel(
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
    /// Verifies that the default navigation tag is set to "Dashboards" when ViewModel is constructed.
    /// This ensures the correct default content view is displayed.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsDefaultNavigationTagToDashboards()
    {
        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
    }

    /// <summary>
    /// Verifies that navigating to a different view updates CurrentNavigationTag.
    /// This ensures the navigation system works correctly for switching content views.
    /// </summary>
    [TestMethod]
    public void NavigationCommand_WithValidTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Farms");

        // Assert
        Assert.AreEqual("Farms", _viewModel.CurrentNavigationTag);
    }
}

