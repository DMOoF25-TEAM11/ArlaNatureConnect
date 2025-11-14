using System.ComponentModel;

namespace ArlaNatureConnect.Core.Services;

public class AppMessageService : IAppMessageService
{
    #region Fields
    private const int _infoMessageDuration = 3000;

    private const string _entityNamePlaceholder = "{EntityName}";

    private const string _errorPrefix = "Fejl: ";
    protected const string _errorEntityNotFound = _errorPrefix + "Kunne ikke finde " + _entityNamePlaceholder + ".";

    private const string _infoPrefix = "Info: ";
    protected const string _infoDeleted = _infoPrefix + _entityNamePlaceholder + " er slettet.";
    protected const string _infoSaved = _infoPrefix + _entityNamePlaceholder + " er gemt.";

    protected const string _confirmDeleteTitle = "Bekræft sletning af " + _entityNamePlaceholder;
    protected string _confirmDelete = "Er du sikker på, at du vil slette " + _entityNamePlaceholder + "?";

    private IEnumerable<string> _statusMessages = Enumerable.Empty<string>();
    private IEnumerable<string> _errorMessages = Enumerable.Empty<string>();
    #endregion
    #region Properties
    public string? EntityName { get; set; }
    public IEnumerable<string> StatusMessages
    {
        get => _statusMessages;
        private set
        {
            if (value != null)
            {
                _statusMessages = value;
                _ = AutoClearInfoMessageAsync(value);
            }
        }
    }
    public IEnumerable<string> ErrorMessages
    {
        get => _errorMessages;
        private set
        {
            _errorMessages = value ?? Enumerable.Empty<string>();
        }
    }
    public bool HasStatusMessages { get => StatusMessages.Any(); }
    public bool HasErrorMessages { get => ErrorMessages.Any(); }
    #endregion
    #region Event handlers
    public event EventHandler? AppMessageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion
    public AppMessageService()
    {
    }

    public void AddInfoMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(EntityName))
        {
            message = message.Replace(_entityNamePlaceholder, EntityName!);
        }
        StatusMessages = StatusMessages.Append(message);
        OnAppMessageChanged();
    }

    public void AddErrorMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(EntityName))
        {
            message = message.Replace(_entityNamePlaceholder, EntityName!);
        }
        ErrorMessages = ErrorMessages.Append(message);
        OnAppMessageChanged();
    }

    public void ClearErrorMessages()
    {
        ErrorMessages = Enumerable.Empty<string>();
        OnAppMessageChanged();
    }

    protected void OnAppMessageChanged()
    {
        try
        {
            AppMessageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // swallow subscriber exceptions
        }
    }

    private async Task AutoClearInfoMessageAsync(IEnumerable<string> msgs)
    {
        if (HasStatusMessages)
        {
            await Task.Delay(_infoMessageDuration);
            var toRemove = msgs ?? Enumerable.Empty<string>();
            StatusMessages = StatusMessages.Where(m => !toRemove.Contains(m)).ToList();
        }
    }
}

