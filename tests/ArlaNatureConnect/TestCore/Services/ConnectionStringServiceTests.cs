using System.Reflection;
using System.Security.Cryptography;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringServiceTests"/> class.
    /// Backups any existing connection file and attempts to remove it so tests start
    /// from a clean state.
    /// </summary>
    public ConnectionStringServiceTests()
    {
    }

    /// <summary>
    /// Restores any backed up connection file after each test run.
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        TryDeleteFiles();
    }

    /// <summary>
    /// Tests that <see cref="ConnectionStringService.ReadAsync"/> returns null when no
    /// connection file exists on disk.
    /// </summary>
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

    /// <summary>
    /// Tests that saving a connection string and then reading it back returns the same
    /// string on Windows where DPAPI is available.
    /// This version adds file-level diagnostics so failing runs reveal whether the
    /// file was written and what bytes were produced.
    /// </summary>
    [TestMethod]
    public async Task SaveAsync_Then_ReadAsync_Returns_Same_String_On_Windows()
    {
        ConnectionStringService svc = new();
        TryOverrideServiceFile(svc);

        await svc.SaveAsync(_connectionString);

        // Inspect the private _filePath so we can assert the file actually contains bytes
        FieldInfo? field = typeof(ConnectionStringService).GetField("_filePath", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "_filePath field should exist on ConnectionStringService.");
        string? path = field!.GetValue(svc) as string;
        Assert.IsFalse(string.IsNullOrEmpty(path), "Internal file path must not be null or empty.");

        // File should exist and contain data
        Assert.IsTrue(File.Exists(path), $"Connection file should exist after SaveAsync. Path: {path}");
        byte[] fileBytes = File.ReadAllBytes(path);
        Assert.IsTrue(fileBytes.Length > 0, $"File was created but is empty (length={fileBytes.Length}).");

        // Keep the original assertions
        bool exists = await svc.ExistsAsync();
        Assert.IsTrue(exists, "Connection file should exist after SaveAsync.");

        string? read = await svc.ReadAsync();

        // If ReadAsync returns null, include helpful diagnostics in the failure message.
        if (read is null)
        {
            string firstBytes = BitConverter.ToString(fileBytes.Take(Math.Min(16, fileBytes.Length)).ToArray());
            Assert.Fail($"ReadAsync returned null after SaveAsync. File length={fileBytes.Length}. First bytes: {firstBytes}");
        }

        Assert.AreEqual(_connectionString, read);
    }

    /// <summary>
    /// Verifies the internal <c>EncryptAsync</c> and <c>DecryptAsync</c> (private static)
    /// methods perform a round-trip: encrypting a connection string and then decrypting
    /// yields the original string.
    /// This test uses reflection to access the private static methods.
    /// </summary>
    [TestMethod]
    public async Task EncryptAsync_DecryptAsync_Roundtrip_ReturnsOriginalString()
    {
        Type type = typeof(ConnectionStringService);

        MethodInfo? encryptMethod = type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "EncryptAsync method should exist");

        MethodInfo? decryptMethod = type.GetMethod("DecryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "DecryptAsync method should exist");

        // Use a unique connection string for the test
        string original = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        // Invoke EncryptAsync via reflection and await the returned Task<byte[]>
        object encryptTaskObj = encryptMethod!.Invoke(null, new object[] { original })!;
        byte[] encrypted = await (Task<byte[]>)encryptTaskObj;

        Assert.IsNotNull(encrypted, "Encrypted bytes should not be null");
        Assert.IsTrue(encrypted.Length > 0, "Encrypted bytes should not be empty");

        // Invoke DecryptAsync via reflection and await the returned Task<string?>
        object decryptTaskObj = decryptMethod!.Invoke(null, new object[] { encrypted })!;
        string? decrypted = await (Task<string?>)decryptTaskObj;

        Assert.IsNotNull(decrypted, "Decrypted string should not be null");
        Assert.AreEqual(original, decrypted, "Decrypted string should match the original");
    }

    /// <summary>
    /// Verifies the private static <c>EncryptAsync</c>/<c>DecryptAsync</c> methods perform
    /// a round-trip on non-Windows platforms. This test is intended to exercise the AES
    /// fallback path used on non-Windows OSes.
    /// </summary>
    [TestMethod]
    public async Task EncryptAsync_DecryptAsync_Roundtrip_ReturnsOriginalString_OnNonWindows()
    {
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //{
        //    Assert.Inconclusive("This test targets non-Windows platforms.");
        //    return;
        //}

        Type type = typeof(ConnectionStringService);

        MethodInfo? encryptMethod = type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "EncryptAsync method should exist");

        MethodInfo? decryptMethod = type.GetMethod("DecryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "DecryptAsync method should exist");

        // Use a unique connection string for the test
        var original = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        // Invoke EncryptAsync via reflection and await the returned Task<byte[]>
        object encryptTaskObj = encryptMethod!.Invoke(null, new object[] { original })!;
        byte[] encrypted = await (Task<byte[]>)encryptTaskObj;

        Assert.IsNotNull(encrypted, "Encrypted bytes should not be null");
        Assert.IsTrue(encrypted.Length > 0, "Encrypted bytes should not be empty");

        // Invoke DecryptAsync via reflection and await the returned Task<string?>
        object decryptTaskObj = decryptMethod!.Invoke(null, new object[] { encrypted })!;
        string? decrypted = await (Task<string?>)decryptTaskObj;

        Assert.IsNotNull(decrypted, "Decrypted string should not be null");
        Assert.AreEqual(original, decrypted, "Decrypted string should match the original");
    }

    /// <summary>
    /// Tests the non-Windows-specific encryption/decryption helper methods if present.
    /// The test will fall back to the general <c>EncryptAsync</c>/<c>DecryptAsync</c>
    /// methods if the explicitly named helpers are not available.
    /// </summary>
    [TestMethod]
    public async Task EncryptNonWindowsOsAsync_DecryptNonWinodwsOsAsync_Roundtrip_ReturnsOriginalString()
    {
        // Test specifically for the non-Windows implementations. Skip on Windows.
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //{
        //    Assert.Inconclusive("This test targets non-Windows platforms.");
        //    return;
        //}

        Type type = typeof(ConnectionStringService);

        // Try to find explicitly named non-Windows helper methods; fall back to the generic EncryptAsync/DecryptAsync if not present.
        MethodInfo? encryptMethod = type.GetMethod("EncryptNonWindowsOsAsync", BindingFlags.NonPublic | BindingFlags.Static)
                           ?? type.GetMethod("EncryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(encryptMethod, "No suitable encryption method found");

        MethodInfo? decryptMethod = type.GetMethod("DecryptNonWinodwsOsAsync", BindingFlags.NonPublic | BindingFlags.Static)
                           ?? type.GetMethod("DecryptAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "No suitable decryption method found");

        // Use a unique connection string for the test
        var original = $"Server=localhost;Database=TestDb;Uid=user;Pwd={Guid.NewGuid():N}";

        // Invoke EncryptAsync via reflection and await the returned Task<byte[]>
        var encryptTaskObj = encryptMethod!.Invoke(null, new object[] { original })!;
        var encrypted = await (Task<byte[]>)encryptTaskObj;

        Assert.IsNotNull(encrypted);
        Assert.IsTrue(encrypted.Length > 0);

        // Invoke DecryptAsync via reflection and await the returned Task<string?>
        var decryptTaskObj = decryptMethod!.Invoke(null, new object[] { encrypted })!;
        var decrypted = await (Task<string?>)decryptTaskObj;

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(original, decrypted);
    }

    /// <summary>
    /// Ensures the catch block in <c>DecryptNonWinodwsOsAsync</c> is exercised by passing
    /// corrupted data that will cause the AES decryption to fail and throw. The method
    /// should catch the exception and return null.
    /// </summary>
    [TestMethod]
    public async Task DecryptNonWinodwsOsAsync_ReturnsNull_WhenDecryptionThrows()
    {
        Type type = typeof(ConnectionStringService);

        MethodInfo? decryptMethod = type.GetMethod("DecryptNonWinodwsOsAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(decryptMethod, "DecryptNonWinodwsOsAsync method should exist");

        // Build an input that has an IV sized block (AES block size / 8) plus a small corrupted cipher
        // to provoke a padding/decryption error inside the CryptoStream.
        using Aes aesTmp = Aes.Create();
        int ivLength = aesTmp.BlockSize / 8;
        byte[] corrupted = new byte[ivLength + 1];
        RandomNumberGenerator.Fill(corrupted); // random bytes -> likely invalid padding and will throw

        object taskObj = decryptMethod!.Invoke(null, new object[] { corrupted })!;
        var result = await (Task<string?>)taskObj;

        Assert.IsNull(result, "DecryptNonWinodwsOsAsync should return null when decryption fails.");
    }

    #region Helpers for backing up/restoring/deleting connection file
    /// <summary>
    /// Attempts to delete the connection file if it exists. Any exceptions are swallowed
    /// to avoid interfering with test setup.
    /// </summary>
    /// <param name="_svc">The connection string service instance used to locate the file.</param>
    private static void TryDeleteFiles()
    {
        while (_instanceCounter >= 0)
        {
            string file = _file.Split('.')[0] + (_instanceCounter--) + ".dat"; // unique per instance
            string path = Path.Combine(_dir, file);
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Attempts to set the service's private _fileName and _filePath fields so the service uses our test filename.
    /// This uses reflection and swallows errors so tests remain robust if the private layout changes.
    /// </summary>
    /// <param name="svc">Instance to override.</param>
    private void TryOverrideServiceFile(ConnectionStringService svc)
    {
        try
        {
            FieldInfo? fileNameField = typeof(ConnectionStringService).GetField("_fileName", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo? filePathField = typeof(ConnectionStringService).GetField("_filePath", BindingFlags.Instance | BindingFlags.NonPublic);

            Directory.CreateDirectory(_dir);
            string file = _file.Split('.')[0] + (_instanceCounter++) + ".dat"; // unique per instance
            string targetPath = Path.Combine(_dir, file);

            if (fileNameField is not null)
            {
                try { fileNameField.SetValue(svc, file); } catch { /* ignore */ }
            }

            if (filePathField is not null)
            {
                // Update the internally stored path to point to our test file
                try { filePathField.SetValue(svc, targetPath); } catch { /* ignore */ }
            }
        }
        catch
        {
            // ignore — tests will fall back to default behavior if reflection fails
        }
    }
    #endregion
}
