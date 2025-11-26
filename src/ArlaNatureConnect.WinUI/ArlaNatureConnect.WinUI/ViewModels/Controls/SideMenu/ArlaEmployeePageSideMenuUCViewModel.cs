using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

public sealed partial class ArlaEmployeePageSideMenuUCViewModel : SideMenuViewModelBase
{
    public ArlaEmployeePageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository personRepository)
        : base(statusInfoServices, appMessageService, personRepository)
    {
        // Fire-and-forget initialization; exceptions handled inside InitializeAsync
        _ = InitializeAsync();
    }

    /// <summary>
    /// Performs asynchronous initialization for the administrator side-menu view-model.
    /// </summary>
    public async Task InitializeAsync()
    {
        using (_statusInfoServices!.BeginLoading())
        {
            IsLoading = true;
            try
            {
                await LoadAvailablePersonsAsync(RoleName.Employee);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
