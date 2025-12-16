using System.ComponentModel;

namespace ArlaNatureConnect.Core.Services;

public class AppMessageService : IAppMessageService
{
    #region Fields
    private const int _INFO_MESSAGE_DURATION = 3000;

    private const string _ENTITY_NAME_PLACEHOLDER = "{EntityName}";

    private const string _ERROR_PREFIX = "Fejl: ";
    protected const string _errorEntityNotFound = _ERROR_PREFIX + "Kunne ikke finde " + _ENTITY_NAME_PLACEHOLDER + ".";

    private const string _INFO_PREFIX = "Info: ";
    protected const string _infoDeleted = _INFO_PREFIX + _ENTITY_NAME_PLACEHOLDER + " er slettet.";
    protected const string _infoSaved = _INFO_PREFIX + _ENTITY_NAME_PLACEHOLDER + " er gemt.";

    protected const string _confirmDeleteTitle = "Bekræft sletning af " + _ENTITY_NAME_PLACEHOLDER;
    protected string _confirmDelete = "Er du sikker på, at du vil slette " + _ENTITY_NAME_PLACEHOLDER + "?";
    #endregion
    #region Properties
    public string? EntityName { get; set; }
    public IEnumerable<string> StatusMessages
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;
                _ = AutoClearInfoMessageAsync(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessages)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasStatusMessages)));
            }
        }
    } = [];
    public IEnumerable<string> ErrorMessages
    {
        get;
        private set
        {
            field = value ?? [];
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessages)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrorMessages)));
        }
    } = [];
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
            message = message.Replace(_ENTITY_NAME_PLACEHOLDER, EntityName!);
        }
        StatusMessages = StatusMessages.Append(message);
        OnAppMessageChanged();
    }

    public void AddErrorMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(EntityName))
        {
            message = message.Replace(_ENTITY_NAME_PLACEHOLDER, EntityName!);
        }
        ErrorMessages = ErrorMessages.Append(message);
        OnAppMessageChanged();
    }

    public void ClearErrorMessages()
    {
        ErrorMessages = [];
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
            await Task.Delay(_INFO_MESSAGE_DURATION).ConfigureAwait(false);
            IEnumerable<string> toRemove = msgs ?? [];
            StatusMessages = [.. StatusMessages.Where(m => !toRemove.Contains(m))];
        }
    }
}

