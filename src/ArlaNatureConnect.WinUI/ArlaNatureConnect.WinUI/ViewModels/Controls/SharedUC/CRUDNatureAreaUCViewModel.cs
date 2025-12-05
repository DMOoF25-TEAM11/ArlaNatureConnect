using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

public partial class CRUDNatureAreaUCViewModel
    : CRUDViewModelBase<INatureAreaRepository, NatureArea>
{
    #region Constants
    #endregion
    #region Fields
    #endregion
    #region Properties
    #endregion
    #region Commands
    #endregion

    public CRUDNatureAreaUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        INatureAreaRepository repository) :
        base(statusInfoServices, appMessageService, repository)
    {
        _statusInfoServices = statusInfoServices;
        _appMessageService = appMessageService;
        //Repository = repository;
    }

    #region Events and Event Handlers
    #endregion
    #region Load Handlers
    #endregion
    #region Command Handlers
    #endregion
    #region Overrides of CRUDViewModelBase
    protected override Task<NatureArea> OnAddFormAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task OnLoadFormAsync(NatureArea entity)
    {
        throw new NotImplementedException();
    }

    protected override Task OnResetFormAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task OnSaveFormAsync()
    {
        throw new NotImplementedException();
    }
    #endregion
    #region Helpers
    #endregion
}