namespace ArlaNatureConnect.Core.Services;

public interface IAppMessageService
{
    string? EntityName { get; set; }
    bool HasStatusMessages { get; }
    bool HasErrorMessages { get; }
    IEnumerable<string> StatusMessages { get; set; }
    IEnumerable<string> ErrorMessages { get; set; }
}
