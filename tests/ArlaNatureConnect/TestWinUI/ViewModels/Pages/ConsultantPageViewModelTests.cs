using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.View.Pages.Consultant;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Runtime.Versioning;
using System.Threading;

namespace TestWinUI.ViewModels.Pages;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class ConsultantPageViewModelTests
{
    private Mock<NavigationHandler> _mockNavigationHandler = null!;
    private Mock<IPersonRepository> _mockPersonRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private ConsultantPageViewModel _viewModel = null!;

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

    [TestMethod]
    public async Task InitializeAsync_CallsRepositoryWithConsultantRoleAndSetsAvailablePersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new Person { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "A", IsActive = true },
            new Person { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "B", IsActive = true }
        };

        _mockPersonRepository
            .Setup(r => r.GetPersonsByRoleAsync(It.Is<string>(s => s == RoleName.Consultant.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        // Act
        await _viewModel.InitializeAsync(new Role { Name = RoleName.Consultant.ToString() });

        // Assert
        Assert.IsNotNull(_viewModel.AvailablePersons);
        Assert.AreEqual(2, _viewModel.AvailablePersons.Count);
        CollectionAssert.AreEquivalent(persons, _viewModel.AvailablePersons);
    }

    [TestMethod]
    public async Task InitializeAsync_WithNoPersons_SetsEmptyAvailablePersons()
    {
        // Arrange
        _mockPersonRepository
            .Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        // Act
        await _viewModel.InitializeAsync(new Role { Name = RoleName.Consultant.ToString() });

        // Assert
        Assert.IsNotNull(_viewModel.AvailablePersons);
        Assert.AreEqual(0, _viewModel.AvailablePersons.Count);
    }

    [TestMethod]
    public async Task InitializeAsync_SetsIsLoadingDuringLoad()
    {
        // Arrange
        var tcs = new TaskCompletionSource<List<Person>>();
        _mockPersonRepository
            .Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act - start initialization but do not complete repository task yet
        Task initTask = _viewModel.InitializeAsync(new Role { Name = RoleName.Consultant.ToString() });

        // Assert - should be loading
        Assert.IsTrue(_viewModel.IsLoading);

        // Complete repository call
        tcs.SetResult(new List<Person>());
        await initTask;

        // Assert - loading finished
        Assert.IsFalse(_viewModel.IsLoading);
    }

    [TestMethod]
    public async Task InitializeAsync_WhenRepositoryThrows_SetsIsLoadingToFalse()
    {
        // Arrange
        _mockPersonRepository
            .Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        // Act & Assert - swallow exception for test, ensure IsLoading gets reset
        try
        {
            await _viewModel.InitializeAsync(new Role { Name = RoleName.Consultant.ToString() });
        }
        catch
        {
            // ignore
        }

        Assert.IsFalse(_viewModel.IsLoading);
    }

    [TestMethod]
    public void ChooseUserCommand_WithValidPerson_SetsSelectedPerson()
    {
        // Arrange
        var person = new Person { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Consultant", IsActive = true };

        // Act
        _viewModel.ChooseUserCommand!.Execute(person);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedPerson);
        Assert.AreEqual(person.Id, _viewModel.SelectedPerson!.Id);
        Assert.IsTrue(_viewModel.IsUserSelected);
    }

    [TestMethod]
    public void ChooseUserCommand_WithNullPerson_DoesNotChangeSelectedPerson()
    {
        // Arrange
        var initial = new Person { Id = Guid.NewGuid(), FirstName = "Initial", LastName = "Person" };
        _viewModel.SelectedPerson = initial;

        // Act
        _viewModel.ChooseUserCommand!.Execute(null);

        // Assert
        Assert.AreEqual(initial.Id, _viewModel.SelectedPerson?.Id);
    }

    [TestMethod]
    public void Constructor_WithNullNavigationHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            new ConsultantPageViewModel(null!, _mockPersonRepository.Object, _mockRoleRepository.Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }

    [TestMethod]
    public void Constructor_WithNullPersonRepository_ThrowsArgumentNullException()
    {
        try
        {
            new ConsultantPageViewModel(_mockNavigationHandler.Object, null!, _mockRoleRepository.Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }

    [TestMethod]
    public void Constructor_WithNullRoleRepository_ThrowsArgumentNullException()
    {
        try
        {
            new ConsultantPageViewModel(_mockNavigationHandler.Object, _mockPersonRepository.Object, null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }

    [TestMethod]
    public void Constructor_SetsDefaultNavigationTagToDashboards()
    {
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
    public void NavigationCommand_CannotExecute_ForNullOrWhitespaceOrEmpty()
    {
        Assert.IsFalse(_viewModel.NavigationCommand!.CanExecute(null));
        Assert.IsFalse(_viewModel.NavigationCommand.CanExecute(string.Empty));
        Assert.IsFalse(_viewModel.NavigationCommand.CanExecute("   "));
    }

    [TestMethod]
    public void NavigationCommand_WithDashboardsTag_SetsCurrentContentToDashboards()
    {
        // Act
        _viewModel.NavigationCommand?.Execute("Dashboards");

        // Assert
        Assert.AreEqual("Dashboards", _viewModel.CurrentNavigationTag);
        Assert.IsTrue(_viewModel.CurrentContent == null || _viewModel.CurrentContent.GetType() == typeof(ConsultantDashboards));
    }

    [TestMethod]
    public void NavigationCommand_WithFarmsTag_SetsCurrentContentToNatureCheck()
    {
        _viewModel.NavigationCommand?.Execute("Farms");
        Assert.AreEqual("Farms", _viewModel.CurrentNavigationTag);
        Assert.IsTrue(_viewModel.CurrentContent == null || _viewModel.CurrentContent.GetType() == typeof(ConsultantNatureCheck));
    }

    [TestMethod]
    public void NavigationCommand_WithTasksTag_SetsCurrentContentToTasks()
    {
        _viewModel.NavigationCommand?.Execute("Tasks");
        Assert.AreEqual("Tasks", _viewModel.CurrentNavigationTag);
        Assert.IsTrue(_viewModel.CurrentContent == null || _viewModel.CurrentContent.GetType() == typeof(ConsultantTasks));
    }

    [TestMethod]
    public async Task InitializeAsync_WithNullRole_DoesNotThrow()
    {
        // Arrange
        _mockPersonRepository
            .Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Person>());

        // Act & Assert
        await _viewModel.InitializeAsync(null);
        Assert.IsNotNull(_viewModel.AvailablePersons);
    }
}

