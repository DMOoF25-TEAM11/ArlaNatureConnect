using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ArlaNatureConnect.Core.Services;

public class ConnectionStringService : IConnectionStringService
{
    private readonly string _fileName = "conn.dat";
    private readonly string _filePath;

    public ConnectionStringService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string dir = Path.Combine(appData, "ArlaNatureConnect");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, _fileName);
    }

    public Task<bool> ExistsAsync() => Task.FromResult(File.Exists(_filePath));


    public async Task<string?> ReadAsync()
    {
        if (!File.Exists(_filePath)) return null;

        // Wait for the file to be unlocked, but only up to a short timeout.
        // Start a task that polls until the file can be opened for exclusive access.
        Task waitForUnlockTask = WaitUntilFileUnlockedAsync(_filePath);
        Task timeoutTask = Task.Delay(3000);

        Task finished = await Task.WhenAny(waitForUnlockTask, timeoutTask);
        if (finished == timeoutTask)
        {
            throw new IOException("Timed out waiting for connection file to be unlocked.");
        }

        // Try reading with a FileStream using FileShare.ReadWrite to avoid hangs if another process briefly has the file open.
        try
        {
            byte[] encrypted;
            using (FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true))
            {
                if (fs.Length == 0) return null;
                encrypted = new byte[fs.Length];
                int offset = 0;
                while (offset < encrypted.Length)
                {
                    int read = await fs.ReadAsync(encrypted, offset, encrypted.Length - offset);
                    if (read == 0) break;
                    offset += read;
                }
            }

            return await DecryptAsync(encrypted);
        }
        catch
        {
            throw new InvalidOperationException("Failed to read or decrypt the connection string.");
        }
    }


    public async Task SaveAsync(string connectionString)
    {
        byte[] encrypted = await EncryptAsync(connectionString);
        await File.WriteAllBytesAsync(_filePath, encrypted);
    }


    #region Encryption and Decryption
    private static Task<byte[]> EncryptAsync(string connectionString)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(connectionString);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                byte[] encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                return Task.FromResult(encrypted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to protect data", ex);
            }
        }
        else
        {
            return EncryptNonWindowsOsAsync(connectionString);
        }
    }

    private static Task<byte[]> EncryptNonWindowsOsAsync(string connectionString)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(connectionString);
        // Cross-platform fallback using AES with a key derived from user+machine
        try
        {
            byte[] key = DeriveKey();
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using MemoryStream ms = new();
            // Prepend IV
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
            }

            return Task.FromResult(ms.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to encrypt data", ex);
        }
    }


    private static Task<string?> DecryptAsync(byte[] encryptedData)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                byte[] unprotected = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                return Task.FromResult<string?>(Encoding.UTF8.GetString(unprotected));
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }
        else
        {
            return DecryptNonWinodwsOsAsync(encryptedData);
        }

    }

    private static Task<string?> DecryptNonWinodwsOsAsync(byte[] encryptedData)
    {
        try
        {
            byte[] key = DeriveKey();
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // IV is prepended
            if (encryptedData.Length < aes.BlockSize / 8) return Task.FromResult<string?>(null);
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            byte[] cipher = new byte[encryptedData.Length - iv.Length];
            Array.Copy(encryptedData, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipher, 0, cipher.Length);
                cs.FlushFinalBlock();
            }

            return Task.FromResult<string?>(Encoding.UTF8.GetString(ms.ToArray()));
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
    #endregion

    private static byte[] DeriveKey()
    {
        // Derive a256-bit key from user and machine information and application base path.
        // This makes the encrypted data specific to the current user on the current machine.
        StringBuilder sb = new();
        sb.Append(Environment.UserName ?? string.Empty);
        sb.Append('|');
        sb.Append(Environment.MachineName ?? string.Empty);
        sb.Append('|');
        sb.Append(AppDomain.CurrentDomain?.BaseDirectory ?? string.Empty);

        // Use SHA256 to produce a32-byte key. This is deterministic but tied to user+machine+app path.
        return SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // Poll until the file can be opened for exclusive access. Returns when unlocked.
    private static async Task WaitUntilFileUnlockedAsync(string path)
    {
        if (!File.Exists(path)) return;

        while (true)
        {
            try
            {
                // Try to open with no sharing. If another process has an exclusive lock this will throw.
                using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return; // succeeded -> file is not locked
            }
            catch (IOException)
            {
                // file is locked by another process
                await Task.Delay(100);
            }
            catch
            {
                // Other errors (e.g., access denied) - treat as locked for polling purposes
                await Task.Delay(100);
            }
        }
    }
}
