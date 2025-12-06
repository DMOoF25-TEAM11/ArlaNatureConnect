using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using System.Collections.ObjectModel;
using System.Diagnostics;

using static ArlaNatureConnect.WinUI.ViewModels.Constants.ViewModelConstants;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

public partial class CRUDNatureAreaUCViewModel
    : CRUDViewModelBase<INatureAreaRepository, NatureArea>
{
    #region Fields
    private readonly IFarmRepository _farmRepository;
    #endregion
    #region Properties
    // Labels
    public static string LabelSelectAFarm => SELECT_A_FARM;
    public static string LabelSelectFarm => SELECT_FARM;
    public static string LabelNatureAreaDetails => NATURE_AREA_DETAILS;
    public static string LabelNatureAreaName => NATURE_AREA_NAME;

    // form
    public string? NatureAreaName
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    // Selected Farm for filtering Nature Areas
    public Farm? SelectedFarm
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            ReloadItemsForSelectedFarm();
        }
    }

    // Collection of FarmsWhoHaveNatureArea for selection
    public IEnumerable<Farm> FarmsWhoHaveNatureArea
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = [];
    #endregion
    #region Commands
    #endregion

    public CRUDNatureAreaUCViewModel(
        IStatusInfoServices statusInfoServices,
        IAppMessageService appMessageService,
        INatureAreaRepository repository,
        IFarmRepository farmRepository) :
        base(statusInfoServices, appMessageService, repository, false)
    {
        _farmRepository = farmRepository;
        _ = LoadFarmsAsync();
        _ = GetAllAsync();
    }

    #region Events and Event Handlers
    #endregion
    #region Load Handlers
    private async void ReloadItemsForSelectedFarm()
    {
        if (SelectedFarm != null)
        {
            IEnumerable<NatureArea> natureAreas = await Repository.GetByFarmIdAsync(SelectedFarm);
            Items = new ObservableCollection<NatureArea>(natureAreas);
        }
        else
        {
            IEnumerable<NatureArea> natureAreas = await Repository.GetAllAsync();
            Items = new ObservableCollection<NatureArea>(natureAreas);
        }
    }

    private async Task LoadFarmsAsync()
    {
        // Get all farms and all nature areas
        IEnumerable<Farm> farms = (await _farmRepository.GetAllAsync());
        Debug.Assert(farms.Any());
        Debug.WriteLine($"Total farms retrieved: {farms.Count()}");

        IEnumerable<NatureArea> natureAreas = (await Repository.GetAllAsync());
        Debug.Assert(natureAreas.Any());
        Debug.WriteLine($"Total nature areas retrieved: {natureAreas.Count()}");

        // Get farm IDs that have at least one nature area
        HashSet<Guid> farmIdsWithNatureArea = [.. natureAreas.Select(na => na.FarmId)];

        // Filter farms to only those with at least one nature area
        IEnumerable<Farm> filteredFarms = farms.Where(f => farmIdsWithNatureArea.Contains(f.Id));
        Debug.Assert(filteredFarms.Any());
        Debug.WriteLine($"Farms with at least one nature area: {filteredFarms.Count()}");
        FarmsWhoHaveNatureArea = new ObservableCollection<Farm>(filteredFarms);
    }
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