namespace ArlaNatureConnect.Core.Services;

public interface IConnectionStringService
{
    Task<bool> ExistsAsync();
    Task<bool> CanConnectionStringConnect(string connectionString);
    Task SaveAsync(string connectionString);
    Task<string?> ReadAsync();
}
