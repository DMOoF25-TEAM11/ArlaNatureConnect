using System.Reflection;
using System.Runtime.InteropServices;
using ArlaNatureConnect.Core.Services;

namespace TestCore.Services;

[TestClass]
public sealed class ConnectionStringServiceTests
{
    [TestMethod]
    public async Task ReadAsync_ReturnsNull_WhenNoFile()
    {
        var svc = new ConnectionStringService();

        var exists = await svc.ExistsAsync();
        Assert.IsFalse(exists, "Connection file should not exist at start of test.");

        var read = await svc.ReadAsync();
        Assert.IsNull(read, "ReadAsync should return null when no file exists.");

        // cleanup if a file unexpectedly exists
        TryDeleteFile(svc);
    }

    [TestMethod]
    public async Task SaveAsync_Then_ReadAsync_Returns_Same_String_On_Windows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Inconclusive("This test requires Windows (DataProtection APIs).");
            return;
        }

        var svc = new ConnectionStringService();
        var conn = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        await svc.SaveAsync(conn);

        var exists = await svc.ExistsAsync();
        Assert.IsTrue(exists, "Connection file should exist after SaveAsync.");

        var read = await svc.ReadAsync();
        Assert.IsNotNull(read);
        Assert.AreEqual(conn, read);

        // cleanup
        TryDeleteFile(svc);
    }

    private static void TryDeleteFile(ConnectionStringService svc)
    {
        try
        {
            var field = typeof(ConnectionStringService).GetField("_filePath", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) return;
            var path = field.GetValue(svc) as string;
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}
