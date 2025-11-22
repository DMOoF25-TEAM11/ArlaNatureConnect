using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using ArlaNatureConnect.Core.Services;

namespace TestCore.Services;

/// <summary>
/// Unit tests for <see cref="ConnectionStringService"/> covering file operations
/// and its encryption/decryption code paths.
/// </summary>
[TestClass]
public sealed class ConnectionStringServiceTests
{
    // Placeholder counter for future tests or diagnostics.
    private static int _instanceCounter;

    private static readonly string _connectionString = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";
    private static readonly string _file = "testConn.dat"; // _instanceCounter placeholder
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArlaNatureConnect");

    public ConnectionStringServiceTests()
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        TryDeleteFiles();
    }

    [TestMethod]
    public async Task ReadAsync_ReturnsNull_WhenNoFile()
    {
        ConnectionStringService _svc = new();
        TryOverrideServiceFile(_svc);

        bool exists = await _svc.ExistsAsync();
        Assert.IsFalse(exists, "Connection file should not exist at start of test.");

        string? read = await _svc.ReadAsync();
        Assert.IsNull(read, "ReadAsync should return null when no file exists.");
    }

    [TestMethod]
    public async Task SaveAsync_Throws_When_CannotConnect()
    {
        // SaveAsync calls CanConnectionStringConnect; ensure it throws when connection cannot be validated.
        ConnectionStringService svc = new();
        TryOverrideServiceFile(svc);

        try
        {
            await svc.SaveAsync("");
            Assert.Fail("SaveAsync should throw InvalidOperationException for empty connection string.");
        }
        catch (InvalidOperationException)
        {
            // expected
        }

        try
        {
            await svc.SaveAsync("Server=;DataSource=;");
            Assert.Fail("SaveAsync should throw InvalidOperationException for invalid connection string.");
        }
        catch (InvalidOperationException)
        {
            // expected
        }
    }

    [TestMethod]
    public async Task WriteEncryptedFile_Then_ReadAsync_Returns_Same_String()
    {
        // Instead of calling SaveAsync (which validates by attempting a DB connection),
        // use the internal EncryptAsync method to produce encrypted bytes and write them
        // to the target file so ReadAsync can decrypt them. This avoids dependency on a DB during tests.
        ConnectionStringService svc = new();
        TryOverrideServiceFile(svc);

        // Use reflection to call the static internal EncryptAsync
        Type type = typeof(ConnectionStringService);
        MethodInfo? encryptMethod = type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static)
                             ?? type.GetMethod("EncryptNonWindowsOsAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "Encrypt method should exist");

        object encryptTaskObj = encryptMethod!.Invoke(null, new object[] { _connectionString })!;
        byte[] encrypted = await (Task<byte[]>)encryptTaskObj;

        // Write the encrypted bytes directly to the service file path
        FieldInfo? field = typeof(ConnectionStringService).GetField("_filePath", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "_filePath field should exist on ConnectionStringService.");
        string? path = field!.GetValue(svc) as string;
        Assert.IsFalse(string.IsNullOrEmpty(path), "Internal file path must not be null or empty.");

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? _dir);
        await File.WriteAllBytesAsync(path, encrypted).ConfigureAwait(false);

        bool exists = await svc.ExistsAsync();
        Assert.IsTrue(exists, "Connection file should exist after writing encrypted bytes.");

        string? read = await svc.ReadAsync();
        Assert.IsNotNull(read, "ReadAsync should return the decrypted connection string.");
        Assert.AreEqual(_connectionString, read);
    }

    [TestMethod]
    public async Task EncryptAsync_DecryptAsync_Roundtrip_ReturnsOriginalString()
    {
        Type type = typeof(ConnectionStringService);

        MethodInfo? encryptMethod = type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "EncryptAsync method should exist");

        MethodInfo? decryptMethod = type.GetMethod("DecryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "DecryptAsync method should exist");

        string original = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        object encryptTaskObj = encryptMethod!.Invoke(null, new object[] { original })!;
        byte[] encrypted = await (Task<byte[]>)encryptTaskObj;

        Assert.IsNotNull(encrypted, "Encrypted bytes should not be null");
        Assert.IsNotEmpty(encrypted, "Encrypted bytes should not be empty");

        object decryptTaskObj = decryptMethod!.Invoke(null, new object[] { encrypted })!;
        string? decrypted = await (Task<string?>)decryptTaskObj;

        Assert.IsNotNull(decrypted, "Decrypted string should not be null");
        Assert.AreEqual(original, decrypted, "Decrypted string should match the original");
    }

    [TestMethod]
    public async Task EncryptNonWindows_DecryptNonWindows_Roundtrip_ReturnsOriginalString()
    {
        Type type = typeof(ConnectionStringService);

        MethodInfo? encryptMethod = type.GetMethod("EncryptNonWindowsOsAsync", BindingFlags.NonPublic | BindingFlags.Static)
                           ?? type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "No suitable encryption method found");

        MethodInfo? decryptMethod = type.GetMethod("DecryptNonWindowsOsAsync", BindingFlags.NonPublic | BindingFlags.Static)
                           ?? type.GetMethod("DecryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "No suitable decryption method found");

        string original = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        object encryptTaskObj = encryptMethod!.Invoke(null, new object[] { original })!;
        byte[] encrypted = await (Task<byte[]>)encryptTaskObj;

        Assert.IsNotNull(encrypted);
        Assert.IsNotEmpty(encrypted);

        object decryptTaskObj = decryptMethod!.Invoke(null, new object[] { encrypted })!;
        string? decrypted = await (Task<string?>)decryptTaskObj;

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(original, decrypted);
    }

    [TestMethod]
    public async Task DecryptNonWindows_ReturnsNull_WhenDecryptionThrows()
    {
        Type type = typeof(ConnectionStringService);

        MethodInfo? decryptMethod = type.GetMethod("DecryptNonWindowsOsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? type.GetMethod("DecryptAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "DecryptNonWindowsOsAsync method should exist");

        using Aes aesTmp = Aes.Create();
        int ivLength = aesTmp.BlockSize / 8;
        byte[] corrupted = new byte[ivLength + 1];
        RandomNumberGenerator.Fill(corrupted);

        object taskObj = decryptMethod!.Invoke(null, new object[] { corrupted })!;
        string? result = await (Task<string?>)taskObj;

        Assert.IsNull(result, "DecryptNonWindowsOsAsync should return null when decryption fails.");
    }

    [TestMethod]
    public async Task CanConnectionStringConnect_ReturnsFalse_ForInvalidInputs()
    {
        ConnectionStringService svc = new();

        bool result1 = await svc.CanConnectionStringConnect(null!);
        Assert.IsFalse(result1);

        bool result2 = await svc.CanConnectionStringConnect("");
        Assert.IsFalse(result2);

        bool result3 = await svc.CanConnectionStringConnect("Server=;Database=;");
        Assert.IsFalse(result3);
    }

    #region Helpers for backing up/restoring/deleting connection file
    private static void TryDeleteFiles()
    {
        while (_instanceCounter >= 0)
        {
            string file = _file.Split('.')[0] + (_instanceCounter--) + ".dat";
            string path = Path.Combine(_dir, file);
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
            }
        }
    }

    private void TryOverrideServiceFile(ConnectionStringService svc)
    {
        try
        {
            FieldInfo? fileNameField = typeof(ConnectionStringService).GetField("_fileName", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo? filePathField = typeof(ConnectionStringService).GetField("_filePath", BindingFlags.Instance | BindingFlags.NonPublic);

            Directory.CreateDirectory(_dir);
            string file = _file.Split('.')[0] + (_instanceCounter++) + ".dat";
            string targetPath = Path.Combine(_dir, file);

            if (fileNameField is not null)
            {
                try { fileNameField.SetValue(svc, file); } catch { }
            }

            if (filePathField is not null)
            {
                try { filePathField.SetValue(svc, targetPath); } catch { }
            }
        }
        catch
        {
        }
    }
    #endregion
}
