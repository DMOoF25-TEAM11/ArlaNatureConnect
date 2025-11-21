using System.ComponentModel;

namespace ArlaNatureConnect.Core.Services;

public interface IAppMessageService : INotifyPropertyChanged
{
    string? EntityName { get; set; }
    bool HasStatusMessages { get; }
    bool HasErrorMessages { get; }
    IEnumerable<string> StatusMessages { get; }
    IEnumerable<string> ErrorMessages { get; }

    void AddInfoMessage(string message);
    void AddErrorMessage(string message);
    void ClearErrorMessages();
}
