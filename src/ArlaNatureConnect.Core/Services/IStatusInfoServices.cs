namespace ArlaNatureConnect.Core.Services;

public interface IStatusInfoServices
{
    bool IsLoading { get; set; }
    bool HasDbConnection { get; set; }

    event EventHandler? StatusInfoChanged;

    IDisposable BeginLoading();
}
