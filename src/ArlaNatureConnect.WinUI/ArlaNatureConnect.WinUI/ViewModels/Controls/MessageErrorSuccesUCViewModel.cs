using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public partial class MessageErrorSuccesUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IAppMessageService? _appMessageService;
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    // Parameterless constructor required by XAML
    public MessageErrorSuccesUCViewModel()
    {
        // Intentionally left empty so XAML can instantiate this ViewModel.
    }

    // Constructor for DI or runtime composition
    public MessageErrorSuccesUCViewModel(IAppMessageService appMessageService)
    {
        _appMessageService = appMessageService;
        _appMessageService.PropertyChanged += _appMessageService_PropertyChanged;
    }

    private void _appMessageService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAppMessageService.StatusMessages))
        {
            StatusMessage = _appMessageService?.StatusMessages != null
                ? string.Join(Environment.NewLine, _appMessageService.StatusMessages)
                : string.Empty;
        }
        else if (e.PropertyName == nameof(IAppMessageService.ErrorMessages))
        {
            ErrorMessage = _appMessageService?.ErrorMessages != null
                ? string.Join(Environment.NewLine, _appMessageService.ErrorMessages)
                : string.Empty;
        }
    }

    #region Properties
    public string StatusMessage
    {
        get;
        set { field = value; OnPropertyChanged(); }
    } = string.Empty;
    public string ErrorMessage
    {
        get;
        set { field = value; OnPropertyChanged(); }
    } = string.Empty;

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
