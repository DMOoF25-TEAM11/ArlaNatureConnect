using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Enums;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;

public sealed partial class ConsultantPageSideMenuUCViewModel : SideMenuViewModelBase
{
    public ConsultantPageSideMenuUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        IPersonRepository repository,
        INavigationHandler navigationHandler)
        : base(statusInfoServices, appMessageService, repository, navigationHandler)
    {
        // Fire-and-forget initialization; exceptions handled inside InitializeAsync
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        using (_statusInfoServices!.BeginLoadingOrSaving())
        {

            IsLoading = true;
            try
            {
                await LoadAvailablePersonsAsync(RoleName.Consultant);
                if (AvailablePersons.Count == 0)
                    throw new InvalidOperationException("No consultants available.");
            }
            finally
            {
                IsLoading = false;
            }

            await Task.CompletedTask;
        }
    }
}
