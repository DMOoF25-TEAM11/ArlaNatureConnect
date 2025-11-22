using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

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

        // Try to open and read the file directly with sharing allowed.
        // If the file is temporarily locked, retry until timeout.
        TimeSpan timeout = TimeSpan.FromSeconds(5);
        using CancellationTokenSource cts = new CancellationTokenSource(timeout);
        CancellationToken ct = cts.Token;
        TimeSpan delay = TimeSpan.FromMilliseconds(100);

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                // Allow others to read/write while we read to avoid blocking writers.
                using FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true);
                if (fs.Length == 0) return null;
                byte[] encrypted = new byte[fs.Length];
                int offset = 0;
                while (offset < encrypted.Length)
                {
                    int read = await fs.ReadAsync(encrypted.AsMemory(offset, encrypted.Length - offset)).ConfigureAwait(false);
                    if (read == 0) break;
                    offset += read;
                }
                return await DecryptAsync(encrypted).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw new IOException("Timed out waiting to read the connection file.");
            }
            catch (IOException)
            {
                // File currently locked by a writer -> wait and retry
                try
                {
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw new IOException("Timed out waiting to read the connection file.");
                }
            }
            catch
            {
                throw new InvalidOperationException("Failed to read or decrypt the connection string.");
            }
        }
    }


    public async Task SaveAsync(string connectionString)
    {
        if (!await CanConnectionStringConnect(connectionString).ConfigureAwait(false))
            throw new InvalidOperationException("Cannot connect to the database with the provided connection string.");

        byte[] encrypted = await EncryptAsync(connectionString).ConfigureAwait(false);
        await File.WriteAllBytesAsync(_filePath, encrypted).ConfigureAwait(false);
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
            return DecryptNonWindowsOsAsync(encryptedData);
        }

    }

    private static Task<string?> DecryptNonWindowsOsAsync(byte[] encryptedData)
    {
        try
        {
            Debug.WriteLine("Using AES decryption for connection string on non-Windows OS.");
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

    public async Task<bool> CanConnectionStringConnect(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        { 
            Debug.WriteLine("Connection string is null or empty.");
            return false;
        }

        // Quick validation: ensure a data source is present and that we can open a connection
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            Debug.WriteLine("Connection string is missing a server/data source. Please configure a valid SQL Server instance.");
            return false;
        }

        // Ensure a short timeout for the validation attempt
        int originalTimeout = builder.ConnectTimeout;
        if (originalTimeout <= 0 || originalTimeout > 10)
        {
            builder.ConnectTimeout = 5;
        }

        try
        {
            using SqlConnection conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await conn.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connection string validation failed: {ex}");
            return false;
        }
        return true;
    }
}
