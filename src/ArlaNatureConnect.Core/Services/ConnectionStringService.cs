using System.Diagnostics;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace ArlaNatureConnect.Core.Services;

public class ConnectionStringService : IConnectionStringService
{
    private readonly string _filePath;
    public ConnectionStringService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ArlaNatureConnect");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "conn.dat");
    }

    public Task<bool> ExistsAsync() => Task.FromResult(File.Exists(_filePath));

    private static async Task<byte[]> EncryptAsync(string connectionString)
    {
        var provider = new DataProtectionProvider("LOCAL=user");
        IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(connectionString, BinaryStringEncoding.Utf8);
        IBuffer protectedBuffer = await provider.ProtectAsync(buffer);
        CryptographicBuffer.CopyToByteArray(protectedBuffer, out byte[]? bytes);
        if (bytes is null) throw new InvalidOperationException("Failed to protect data");
        return bytes;
    }

    public async Task SaveAsync(string connectionString)
    {
        var encrypted = await EncryptAsync(connectionString);
        await File.WriteAllBytesAsync(_filePath, encrypted);
    }

    private static async Task<string?> DecryptAsync(byte[] encryptedData)
    {
        try
        {
            // Use MemoryMarshal to safely create IBuffer from byte[]
            IBuffer buffer = CryptographicBuffer.CreateFromByteArray(encryptedData);
            DataProtectionProvider provider = new DataProtectionProvider();
            IBuffer unprotected = await provider.UnprotectAsync(buffer);
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, unprotected);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> ReadAsync()
    {
        if (!File.Exists(_filePath)) return null;
        byte[] encrypted = await File.ReadAllBytesAsync(_filePath);
#if DEBUG
        // For debugging purposes, allow re-encryption of the file
        var decrypted = await DecryptAsync(encrypted);
        if (decrypted is not null)
        {
            var reEncrypted = await EncryptAsync(decrypted);
            Debug.WriteLine($"*** {this}.ReadAsync() : decrypted connection string: {decrypted}");
        }
        else
        {
            Debug.WriteLine($"*** {this}.ReadAsync() : Failed to decrypt connection string during re-encryption.");
        }
#endif
        return await DecryptAsync(encrypted);
    }
}
