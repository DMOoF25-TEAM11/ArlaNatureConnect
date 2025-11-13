namespace TestInfrastructure;

[TestClass]
public class DependencyInjectionTest
{
    private static readonly string _connectionString = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";
    private static readonly string _file = "testConn.dat"; // _instanceCounter placeholder
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArlaNatureConnect", "tests");

    private static string GetConnFilePath() => Path.Combine(_dir, _file);

    [TestMethod]
    public async Task AddInfrastructure_Registers_Expected_Services()
    {
    }

    [TestMethod]
    public async Task AddInfrastructure_Registers_AppDbContext_When_ConnectionString_Present()
    {
    }
}
