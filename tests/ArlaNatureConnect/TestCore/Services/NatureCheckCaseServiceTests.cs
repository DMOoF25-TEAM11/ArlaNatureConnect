using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;

using Moq;

using System.Runtime.InteropServices;

namespace TestCore.Services;

/// <summary>
/// Tests for NatureCheckCaseService - UC002B
/// Tests all methods, COM error handling, and thread safety.
/// </summary>
[TestClass]
public class NatureCheckCaseServiceTests
{
    public TestContext TestContext { get; set; } = null!;

    private Mock<IFarmRepository> _farmRepositoryMock = null!;
    private Mock<IPersonRepository> _personRepositoryMock = null!;
    private Mock<IAddressRepository> _addressRepositoryMock = null!;
    private Mock<INatureCheckCaseRepository> _natureCheckCaseRepositoryMock = null!;
    private Mock<IRoleRepository> _roleRepositoryMock = null!;
    private NatureCheckCaseService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _natureCheckCaseRepositoryMock = new Mock<INatureCheckCaseRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _service = new(
            _farmRepositoryMock.Object,
            _personRepositoryMock.Object,
            _addressRepositoryMock.Object,
            _natureCheckCaseRepositoryMock.Object,
            _roleRepositoryMock.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when any repository is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullFarmRepository_ThrowsArgumentNullException()
    {
        try
        {
            _ = new NatureCheckCaseService(
                null!,
                _personRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _natureCheckCaseRepositoryMock.Object,
                _roleRepositoryMock.Object);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when person repository is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullPersonRepository_ThrowsArgumentNullException()
    {
        try
        {
            _ = new NatureCheckCaseService(
                _farmRepositoryMock.Object,
                null!,
                _addressRepositoryMock.Object,
                _natureCheckCaseRepositoryMock.Object,
                _roleRepositoryMock.Object);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when address repository is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAddressRepository_ThrowsArgumentNullException()
    {
        try
        {
            _ = new NatureCheckCaseService(
                _farmRepositoryMock.Object,
                _personRepositoryMock.Object,
                null!,
                _natureCheckCaseRepositoryMock.Object,
                _roleRepositoryMock.Object);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when nature check case repository is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullNatureCheckCaseRepository_ThrowsArgumentNullException()
    {
        try
        {
            _ = new NatureCheckCaseService(
                _farmRepositoryMock.Object,
                _personRepositoryMock.Object,
                _addressRepositoryMock.Object,
                null!,
                _roleRepositoryMock.Object);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when role repository is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullRoleRepository_ThrowsArgumentNullException()
    {
        try
        {
            _ = new NatureCheckCaseService(
                _farmRepositoryMock.Object,
                _personRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _natureCheckCaseRepositoryMock.Object,
                null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that constructor initializes service successfully with all valid repositories.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidRepositories_InitializesSuccessfully()
    {
        NatureCheckCaseService service = new(
            _farmRepositoryMock.Object,
            _personRepositoryMock.Object,
            _addressRepositoryMock.Object,
            _natureCheckCaseRepositoryMock.Object,
            _roleRepositoryMock.Object);

        Assert.IsNotNull(service);
    }

    #endregion

    #region LoadAssignmentContextAsync Tests

    /// <summary>
    /// Tests that LoadAssignmentContextAsync returns context with farms and consultants.
    /// </summary>
    [TestMethod]
    public async Task LoadAssignmentContextAsync_WithValidData_ReturnsContext()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        List<Farm> farms =
        [
            new() { Id = Guid.NewGuid(), Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() }
        ];
        List<Person> persons =
        [
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", RoleId = Guid.NewGuid() }
        ];
        List<Person> consultants =
        [
            new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", RoleId = Guid.NewGuid() }
        ];
        List<Address> addresses =
        [
            new() { Id = Guid.NewGuid(), Street = "Street1", City = "City1" }
        ];

        _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(farms);
        _personRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(persons);
        _personRepositoryMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(consultants);
        _addressRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(addresses);
        _natureCheckCaseRepositoryMock.Setup(r => r.GetActiveCasesAsync(cancellationToken))
            .ReturnsAsync([]);

        // Act
        NatureCheckCaseAssignmentContext result = await _service.LoadAssignmentContextAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result.Farms);
        Assert.HasCount(1, result.Consultants);
    }

    /// <summary>
    /// Tests that LoadAssignmentContextAsync handles COMException gracefully.
    /// </summary>
    [TestMethod]
    public async Task LoadAssignmentContextAsync_WhenRepositoryThrowsCOMException_PropagatesException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ThrowsAsync(new COMException("COM error"));

        // Act & Assert
        try
        {
            await _service.LoadAssignmentContextAsync(cancellationToken);
            Assert.Fail("Expected COMException");
        }
        catch (COMException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that LoadAssignmentContextAsync is thread-safe when called concurrently.
    /// </summary>
    [TestMethod]
    public async Task LoadAssignmentContextAsync_IsThreadSafe_WhenCalledConcurrently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        List<Farm> farms = [new() { Id = Guid.NewGuid(), Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() }];
        List<Person> persons = [];
        List<Person> consultants = [];
        List<Address> addresses = [];

        _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(farms);
        _personRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(persons);
        _personRepositoryMock.Setup(r => r.GetPersonsByRoleAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(consultants);
        _addressRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(addresses);
        _natureCheckCaseRepositoryMock.Setup(r => r.GetActiveCasesAsync(cancellationToken))
            .ReturnsAsync([]);

        // Act
        const int threads = 10;
        Task<NatureCheckCaseAssignmentContext>[] tasks = new Task<NatureCheckCaseAssignmentContext>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => _service.LoadAssignmentContextAsync(cancellationToken), cancellationToken);
        }

        NatureCheckCaseAssignmentContext[] results = await Task.WhenAll(tasks);

        // Assert
        Assert.HasCount(threads, results);
        foreach (NatureCheckCaseAssignmentContext result in results)
        {
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Farms);
        }
    }

    #endregion

    #region AssignCaseAsync Tests

    /// <summary>
    /// Tests that AssignCaseAsync throws ArgumentNullException when request is null.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(null!, cancellationToken);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync creates case successfully when all validations pass.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WithValidRequest_CreatesCase()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();
        Guid consultantId = Guid.NewGuid();
        Guid assignedByOwnerId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };
        Person consultant = new() { Id = consultantId, FirstName = "Jane", LastName = "Smith", RoleId = roleId };
        Role role = new() { Id = roleId, Name = "Consultant" };

        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = farmId,
            ConsultantId = consultantId,
            AssignedByPersonId = assignedByOwnerId, // <-- changed property name
            Notes = "Test notes",
            Priority = "High",
            AllowDuplicateActiveCase = false
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _personRepositoryMock.Setup(r => r.GetByIdAsync(consultantId, cancellationToken))
            .ReturnsAsync(consultant);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, cancellationToken))
            .ReturnsAsync(role);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(false);
        _natureCheckCaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<NatureCheckCase>(), cancellationToken))
            .Returns((Task<NatureCheckCase>)Task.CompletedTask);

        // Act
        NatureCheckCase result = await _service.AssignCaseAsync(request, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(farmId, result.FarmId);
        Assert.AreEqual(consultantId, result.ConsultantId);
        Assert.AreEqual(NatureCheckCaseStatus.Assigned, result.Status);
        Assert.AreEqual("High", result.Priority);
        _natureCheckCaseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<NatureCheckCase>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that AssignCaseAsync throws InvalidOperationException when farm does not exist.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WhenFarmDoesNotExist_ThrowsInvalidOperationException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = Guid.NewGuid(),
            ConsultantId = Guid.NewGuid(),
            AssignedByPersonId = Guid.NewGuid()
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync((Farm?)null);

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(request, cancellationToken);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync throws InvalidOperationException when consultant does not exist.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WhenConsultantDoesNotExist_ThrowsInvalidOperationException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        Guid farmId = Guid.NewGuid();
        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = farmId,
            ConsultantId = Guid.NewGuid(),
            AssignedByPersonId = Guid.NewGuid()
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _personRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync((Person?)null);

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(request, cancellationToken);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync throws InvalidOperationException when consultant does not have Consultant role.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WhenConsultantDoesNotHaveConsultantRole_ThrowsInvalidOperationException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        Guid farmId = Guid.NewGuid();
        Guid consultantId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };
        Person consultant = new() { Id = consultantId, FirstName = "Jane", LastName = "Smith", RoleId = roleId };
        Role role = new() { Id = roleId, Name = "Employee" }; // Wrong role

        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = farmId,
            ConsultantId = consultantId,
            AssignedByPersonId = Guid.NewGuid()
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _personRepositoryMock.Setup(r => r.GetByIdAsync(consultantId, cancellationToken))
            .ReturnsAsync(consultant);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, cancellationToken))
            .ReturnsAsync(role);

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(request, cancellationToken);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync throws InvalidOperationException when farm has active case and duplicate is not allowed.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WhenFarmHasActiveCaseAndDuplicateNotAllowed_ThrowsInvalidOperationException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        Guid farmId = Guid.NewGuid();
        Guid consultantId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };
        Person consultant = new() { Id = consultantId, FirstName = "Jane", LastName = "Smith", RoleId = roleId };
        Role role = new() { Id = roleId, Name = "Consultant" };

        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = farmId,
            ConsultantId = consultantId,
            AssignedByPersonId = Guid.NewGuid(),
            AllowDuplicateActiveCase = false
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _personRepositoryMock.Setup(r => r.GetByIdAsync(consultantId, cancellationToken))
            .ReturnsAsync(consultant);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, cancellationToken))
            .ReturnsAsync(role);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(true);

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(request, cancellationToken);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync handles COMException from repository.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_WhenRepositoryThrowsCOMException_PropagatesException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = Guid.NewGuid(),
            ConsultantId = Guid.NewGuid(),
            AssignedByPersonId = Guid.NewGuid()
        };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
            .ThrowsAsync(new COMException("COM error"));

        // Act & Assert
        try
        {
            await _service.AssignCaseAsync(request, cancellationToken);
            Assert.Fail("Expected COMException");
        }
        catch (COMException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that AssignCaseAsync is thread-safe when called concurrently.
    /// </summary>
    [TestMethod]
    public async Task AssignCaseAsync_IsThreadSafe_WhenCalledConcurrently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        Guid farmId = Guid.NewGuid();
        Guid consultantId = Guid.NewGuid();
        Guid assignedByOwnerId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };
        Person consultant = new Person { Id = consultantId, FirstName = "Jane", LastName = "Smith", RoleId = roleId };
        Role role = new Role { Id = roleId, Name = "Consultant" };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _personRepositoryMock.Setup(r => r.GetByIdAsync(consultantId, cancellationToken))
            .ReturnsAsync(consultant);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, cancellationToken))
            .ReturnsAsync(role);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(false);
        _natureCheckCaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<NatureCheckCase>(), cancellationToken))
            .Returns((Task<NatureCheckCase>)Task.CompletedTask);

        NatureCheckCaseAssignmentRequest request = new()
        {
            FarmId = farmId,
            ConsultantId = consultantId,
            AssignedByPersonId = assignedByOwnerId,
            AllowDuplicateActiveCase = true // Allow duplicates for concurrent test
        };

        // Act
        const int threads = 5;
        Task<NatureCheckCase>[] tasks = new Task<NatureCheckCase>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => _service.AssignCaseAsync(request, cancellationToken), cancellationToken);
        }

        NatureCheckCase[] results = await Task.WhenAll(tasks);

        // Assert
        Assert.HasCount(threads, results);
        foreach (NatureCheckCase result in results)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(farmId, result.FarmId);
            Assert.AreEqual(consultantId, result.ConsultantId);
        }
    }

    #endregion

    #region SaveFarmAsync Tests

    /// <summary>
    /// Tests that SaveFarmAsync throws ArgumentNullException when request is null.
    /// </summary>
    [TestMethod]
    public async Task SaveFarmAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Act & Assert
        // Act & Assert
        try
        {
            await _service.SaveFarmAsync(null!, cancellationToken);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that SaveFarmAsync creates new farm successfully.
    /// </summary>
    [TestMethod]
    public async Task SaveFarmAsync_WithNewFarmRequest_CreatesFarm()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        // Arrange
        Guid roleId = Guid.NewGuid();
        Role role = new() { Id = roleId, Name = "Farmer" };

        FarmRegistrationRequest request = new()
        {
            FarmName = "New Farm",
            Cvr = "12345678",
            Street = "Street1",
            City = "City1",
            PostalCode = "1234",
            Country = "Danmark",
            OwnerFirstName = "John",
            OwnerLastName = "Doe",
            OwnerEmail = "john@example.com"
        };

        _roleRepositoryMock.Setup(r => r.GetByNameAsync("Farmer", cancellationToken))
            .ReturnsAsync(role);
        _addressRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Address>(), cancellationToken))
            .Returns((Task<Address>)Task.CompletedTask);
        _personRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Person>(), cancellationToken))
            .Returns((Task<Person>)Task.CompletedTask);
        _farmRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Farm>(), cancellationToken))
            .Returns((Task<Farm>)Task.CompletedTask);

        // Act
        Farm result = await _service.SaveFarmAsync(request, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("New Farm", result.Name);
        Assert.AreEqual("12345678", result.CVR);
        _addressRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Address>(), cancellationToken), Times.Once);
        _personRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Person>(), cancellationToken), Times.Once);
        _farmRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Farm>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SaveFarmAsync throws ArgumentException when farm name is empty.
    /// </summary>
    [TestMethod]
    public async Task SaveFarmAsync_WithEmptyFarmName_ThrowsArgumentException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        FarmRegistrationRequest request = new()
        {
            FarmName = "",
            Cvr = "12345678",
            OwnerFirstName = "John",
            OwnerLastName = "Doe",
            OwnerEmail = "john@example.com"
        };

        // Act & Assert
        try
        {
            await _service.SaveFarmAsync(request, cancellationToken);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that SaveFarmAsync handles COMException from repository.
    /// </summary>
    [TestMethod]
    public async Task SaveFarmAsync_WhenRepositoryThrowsCOMException_PropagatesException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        FarmRegistrationRequest request = new()
        {
            FarmName = "New Farm",
            Cvr = "12345678",
            OwnerFirstName = "John",
            OwnerLastName = "Doe",
            OwnerEmail = "john@example.com"
        };

        _roleRepositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), cancellationToken))
            .ThrowsAsync(new COMException("COM error"));

        // Act & Assert
        try
        {
            await _service.SaveFarmAsync(request, cancellationToken);
            Assert.Fail("Expected COMException");
        }
        catch (COMException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that SaveFarmAsync is thread-safe when called concurrently.
    /// </summary>
    [TestMethod]
    public async Task SaveFarmAsync_IsThreadSafe_WhenCalledConcurrently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid roleId = Guid.NewGuid();
        Role role = new Role { Id = roleId, Name = "Farmer" };

        _roleRepositoryMock.Setup(r => r.GetByNameAsync("Farmer", cancellationToken))
            .ReturnsAsync(role);
        _addressRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Address>(), cancellationToken))
            .Returns((Task<Address>)Task.CompletedTask);
        _personRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Person>(), cancellationToken))
            .Returns((Task<Person>)Task.CompletedTask);
        _farmRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Farm>(), cancellationToken))
            .Returns((Task<Farm>)Task.CompletedTask);

        FarmRegistrationRequest request = new()
        {
            FarmName = "New Farm",
            Cvr = "12345678",
            Street = "Street1",
            City = "City1",
            PostalCode = "1234",
            Country = "Danmark",
            OwnerFirstName = "John",
            OwnerLastName = "Doe",
            OwnerEmail = "john@example.com"
        };

        // Act
        const int threads = 5;
        Task<Farm>[] tasks = new Task<Farm>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => _service.SaveFarmAsync(request, cancellationToken), cancellationToken);
        }

        Farm[] results = await Task.WhenAll(tasks);

        // Assert
        Assert.HasCount(threads, results);
        foreach (Farm result in results)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual("New Farm", result.Name);
        }
    }

    #endregion

    #region DeleteFarmAsync Tests

    /// <summary>
    /// Tests that DeleteFarmAsync deletes farm successfully when no active cases exist.
    /// </summary>
    [TestMethod]
    public async Task DeleteFarmAsync_WithNoActiveCases_DeletesFarm()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();
        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(false);
        _farmRepositoryMock.Setup(r => r.DeleteAsync(farmId, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteFarmAsync(farmId, cancellationToken);

        // Assert
        _farmRepositoryMock.Verify(r => r.DeleteAsync(farmId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteFarmAsync returns silently when farm does not exist.
    /// </summary>
    [TestMethod]
    public async Task DeleteFarmAsync_WhenFarmDoesNotExist_ReturnsSilently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync((Farm?)null);

        // Act
        await _service.DeleteFarmAsync(farmId, cancellationToken);

        // Assert
        _farmRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), cancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that DeleteFarmAsync throws InvalidOperationException when farm has active cases.
    /// </summary>
    [TestMethod]
    public async Task DeleteFarmAsync_WhenFarmHasActiveCases_ThrowsInvalidOperationException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();
        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(true);

        // Act & Assert
        try
        {
            await _service.DeleteFarmAsync(farmId, cancellationToken);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that DeleteFarmAsync handles COMException from repository.
    /// </summary>
    [TestMethod]
    public async Task DeleteFarmAsync_WhenRepositoryThrowsCOMException_PropagatesException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ThrowsAsync(new COMException("COM error"));

        // Act & Assert
        try
        {
            await _service.DeleteFarmAsync(farmId, cancellationToken);
            Assert.Fail("Expected COMException");
        }
        catch (COMException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that DeleteFarmAsync is thread-safe when called concurrently.
    /// </summary>
    [TestMethod]
    public async Task DeleteFarmAsync_IsThreadSafe_WhenCalledConcurrently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid farmId = Guid.NewGuid();
        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        _farmRepositoryMock.Setup(r => r.GetByIdAsync(farmId, cancellationToken))
            .ReturnsAsync(farm);
        _natureCheckCaseRepositoryMock.Setup(r => r.FarmHasActiveCaseAsync(farmId, cancellationToken))
            .ReturnsAsync(false);
        _farmRepositoryMock.Setup(r => r.DeleteAsync(farmId, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        const int threads = 5;
        Task[] tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => _service.DeleteFarmAsync(farmId, cancellationToken), cancellationToken);
        }

        await Task.WhenAll(tasks);

        // Assert
        _farmRepositoryMock.Verify(r => r.DeleteAsync(farmId, cancellationToken), Times.Exactly(threads));
    }

    #endregion

    #region GetNotificationsForConsultantAsync Tests

    /// <summary>
    /// Tests that GetNotificationsForConsultantAsync returns notifications for consultant.
    /// </summary>
    [TestMethod]
    public async Task GetNotificationsForConsultantAsync_WithAssignedCases_ReturnsNotifications()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid consultantId = Guid.NewGuid();
        Guid farmId = Guid.NewGuid();
        Guid caseId = Guid.NewGuid();

        List<NatureCheckCase> cases =
        [
            new()
            {
                Id = caseId,
                FarmId = farmId,
                ConsultantId = consultantId,
                Status = NatureCheckCaseStatus.Assigned,
                Priority = "High",
                Notes = "Test notes",
                AssignedAt = DateTimeOffset.UtcNow
            }
        ];

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        _natureCheckCaseRepositoryMock.Setup(r => r.GetAssignedCasesForConsultantAsync(consultantId, cancellationToken))
            .ReturnsAsync(cases);
        _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync([farm]);

        // Act
        IReadOnlyList<ConsultantNotificationDto> result = await _service.GetNotificationsForConsultantAsync(consultantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual("Farm1", result[0].FarmName);
        Assert.AreEqual("High", result[0].Priority);
    }

    /// <summary>
    /// Tests that GetNotificationsForConsultantAsync returns empty list when no cases exist.
    /// </summary>
    [TestMethod]
    public async Task GetNotificationsForConsultantAsync_WithNoCases_ReturnsEmptyList()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid consultantId = Guid.NewGuid();

        _natureCheckCaseRepositoryMock.Setup(r => r.GetAssignedCasesForConsultantAsync(consultantId, cancellationToken))
            .ReturnsAsync([]);

        // Act
        IReadOnlyList<ConsultantNotificationDto> result = await _service.GetNotificationsForConsultantAsync(consultantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

    /// <summary>
    /// Tests that GetNotificationsForConsultantAsync handles COMException from repository.
    /// </summary>
    [TestMethod]
    public async Task GetNotificationsForConsultantAsync_WhenRepositoryThrowsCOMException_PropagatesException()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid consultantId = Guid.NewGuid();

        _natureCheckCaseRepositoryMock.Setup(r => r.GetAssignedCasesForConsultantAsync(consultantId, cancellationToken))
            .ThrowsAsync(new COMException("COM error"));

        // Act & Assert
        try
        {
            await _service.GetNotificationsForConsultantAsync(consultantId, cancellationToken);
            Assert.Fail("Expected COMException");
        }
        catch (COMException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Tests that GetNotificationsForConsultantAsync is thread-safe when called concurrently.
    /// </summary>
    [TestMethod]
    public async Task GetNotificationsForConsultantAsync_IsThreadSafe_WhenCalledConcurrently()
    {
        CancellationToken cancellationToken = TestContext.CancellationToken;

        // Arrange
        Guid consultantId = Guid.NewGuid();
        Guid farmId = Guid.NewGuid();

        List<NatureCheckCase> cases =
        [
            new()
            {
                Id = Guid.NewGuid(),
                FarmId = farmId,
                ConsultantId = consultantId,
                Status = NatureCheckCaseStatus.Assigned,
                AssignedAt = DateTimeOffset.UtcNow
            }
        ];

        Farm farm = new() { Id = farmId, Name = "Farm1", CVR = "123", OwnerId = Guid.NewGuid(), AddressId = Guid.NewGuid() };

        _natureCheckCaseRepositoryMock.Setup(r => r.GetAssignedCasesForConsultantAsync(consultantId, cancellationToken))
            .ReturnsAsync(cases);
        _farmRepositoryMock.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync([farm]);

        // Act
        const int threads = 10;
        Task<IReadOnlyList<ConsultantNotificationDto>>[] tasks = new Task<IReadOnlyList<ConsultantNotificationDto>>[threads];
        for (int t = 0; t < threads; t++)
        {
            tasks[t] = Task.Run(() => _service.GetNotificationsForConsultantAsync(consultantId, cancellationToken), cancellationToken);
        }

        IReadOnlyList<ConsultantNotificationDto>[] results = await Task.WhenAll(tasks);

        // Assert
        Assert.HasCount(threads, results);
        foreach (IReadOnlyList<ConsultantNotificationDto> result in results)
        {
            Assert.IsNotNull(result);
            Assert.HasCount(1, result);
        }
    }

    #endregion
}

