using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels;

public partial class MessageErrorSuccesUCViewModel : ViewModelBase
{
    #region Fields
    private string _statusMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private readonly IAppMessageService _appMessageService;
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    public MessageErrorSuccesUCViewModel(IAppMessageService appMessageService)
    {
        _appMessageService = appMessageService ?? throw new ArgumentNullException(nameof(appMessageService));
        _appMessageService.AddErrorMessage("loading");
        //_appMessageService.StatusMessageChanged += OnStatusMessageChanged;
        //_appMessageService.ErrorMessageChanged += OnErrorMessageChanged;
    }

    #region Properties
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool IsSuccessVisible
    {
        get => !string.IsNullOrEmpty(StatusMessage);
    }

    public bool IsErrorVisible
    {
        get => !string.IsNullOrEmpty(ErrorMessage);
    }

    #endregion
    #region Observables Properties
    #endregion
    #region Load handler
    #endregion
    #region Commands
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    #endregion
    #region Helpers
    #endregion
}
