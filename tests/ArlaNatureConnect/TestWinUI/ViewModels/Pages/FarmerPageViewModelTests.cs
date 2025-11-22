using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Moq;

using System.Runtime.Versioning;

namespace TestWinUI.ViewModels.Pages;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class FarmerPageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private Mock<IPersonRepository> _mockPersonRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private FarmerPageViewModel _viewModel = null!;

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

    [TestMethod]
    public async Task InitializeAsync_CallsRepositoryWithFarmerRoleAndSetsAvailablePersons()
    {
        // Arrange
        var farmer1 = new Person { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "A", IsActive = true };
        var farmer2 = new Person { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "B", IsActive = true };
        var persons = new List<Person> { farmer1, farmer2 };

        _mockPersonRepository.Setup(r => r.GetPersonsByRoleAsync(It.Is<string>(s => s == "Farmer"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        // Act
        await _viewModel.InitializeAsync(new Role { Name = "Farmer" });

        // Assert
        Assert.IsNotNull(_viewModel.AvailablePersons);
        Assert.AreEqual(2, _viewModel.AvailablePersons.Count);
        CollectionAssert.AreEquivalent(persons, _viewModel.AvailablePersons);
    }

    [TestMethod]
    public async Task InitializeAsync_WithNoPersons_SetsEmptyAvailablePersons()
    {
        // Arrange
        _mockPersonRepository.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        // Act
        await _viewModel.InitializeAsync(new Role { Name = "Farmer" });

        // Assert
        Assert.IsNotNull(_viewModel.AvailablePersons);
        Assert.AreEqual(0, _viewModel.AvailablePersons.Count);
    }

    [TestMethod]
    public void ChooseUserCommand_WithValidPerson_SetsSelectedPerson()
    {
        // Arrange
        Person person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Farmer",
            IsActive = true
        };

        // Act
        _viewModel.ChooseUserCommand!.Execute(person);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedPerson);
        Assert.AreEqual(person.Id, _viewModel.SelectedPerson.Id);
        Assert.IsTrue(_viewModel.IsUserSelected);
    }

    [TestMethod]
    public void ChooseUserCommand_WithNullPerson_DoesNotChangeSelectedPerson()
    {
        // Arrange
        Person initialPerson = new Person { Id = Guid.NewGuid(), FirstName = "Initial", LastName = "Person" };
        _viewModel.SelectedPerson = initialPerson;

        // Act
        _viewModel.ChooseUserCommand!.Execute(null);

        // Assert
        Assert.AreEqual(initialPerson.Id, _viewModel.SelectedPerson?.Id);
    }

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

    [TestMethod]
    public void Constructor_SetsDefaultNavigationTagToDashboards()
    {
        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
    }

    [TestMethod]
    public void NavigationCommand_WithValidTag_UpdatesCurrentNavigationTag()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Farms");

        // Assert
        Assert.AreEqual("Farms", _viewModel.CurrentNavigationTag);
    }

    [TestMethod]
    public async Task InitializeAsync_SetsIsLoadingDuringLoad()
    {
        // Arrange
        var tcs = new TaskCompletionSource<List<Person>>();
        _mockPersonRepository.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act
        var initTask = _viewModel.InitializeAsync(new Role { Name = "Farmer" });

        // Assert - should be loading
        Assert.IsTrue(_viewModel.IsLoading);

        // Complete the task
        tcs.SetResult(new List<Person>());
        await initTask;

        Assert.IsFalse(_viewModel.IsLoading);
    }

    [TestMethod]
    public async Task InitializeAsync_WhenRepositoryThrowsException_SetsIsLoadingToFalse()
    {
        // Arrange
        _mockPersonRepository.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        try
        {
            await _viewModel.InitializeAsync(new Role { Name = "Farmer" });
        }
        catch
        {
            // ignore
        }

        Assert.IsFalse(_viewModel.IsLoading);
    }

    [TestMethod]
    public async Task InitializeAsync_WithNullRole_DoesNotThrow()
    {
        // Arrange
        _mockPersonRepository.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        // Act & Assert
        await _viewModel.InitializeAsync(null);

        Assert.IsNotNull(_viewModel.AvailablePersons);
    }
}

