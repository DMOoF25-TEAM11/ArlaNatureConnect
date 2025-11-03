namespace ArlaNatureConnect.Core.Services;

public interface IConnectionStringService
{
    Task<bool> ExistsAsync();

    Task SaveAsync(string connectionString);

    Task<string?> ReadAsync();
}
