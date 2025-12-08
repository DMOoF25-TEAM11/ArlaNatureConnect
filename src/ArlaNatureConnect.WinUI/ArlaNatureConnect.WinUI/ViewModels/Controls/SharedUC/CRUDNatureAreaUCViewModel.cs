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
    private const double EARTH_RADIUS = 6371009;
    private readonly IFarmRepository _farmRepository;
    #endregion
    #region Properties
    // Labels
    public static string LabelSelectAFarm => SELECT_A_FARM;
    public static string LabelSelectFarm => SELECT_FARM;
    public static string LabelNatureAreaDetails => NATURE_AREA_DETAILS;
    public static string LabelNatureAreaName => NATURE_AREA_NAME;
    public static string LabelNatureAreaDescription => NATURE_AREA_DESCRIPTION;
    public static string LabelLongitude => LONGITUDE;
    public static string LabelLatitude => LATITUDE;

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

    public string? NatureAreaDescription
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NatureAreaCoordinate> Coordinates
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NatureAreaSizeKm2));
        }
    } = [];

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

    public double NatureAreaSizeKm2
    {
        get;
        set
        {
            double area = CalculatePolygonAreaKm2(Coordinates);
            if (Math.Abs(field - area) < 0.0001) return;
            field = area;
            OnPropertyChanged();
        }
    }

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
        SelectedEntityChanged += CRUDNatureAreaUCViewModel_SelectedEntityChanged;
    }

    #region Events and Event Handlers
    private void CRUDNatureAreaUCViewModel_SelectedEntityChanged(object? sender, NatureArea? e)
    {
        PopulateFormFromSelectedItem();
        NatureAreaSizeKm2 = CalculatePolygonAreaKm2(Coordinates);
    }
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
    public void PopulateFormFromSelectedItem()
    {
        if (SelectedItem != null)
        {
            NatureAreaName = SelectedItem.Name;
            NatureAreaDescription = SelectedItem.Description;
            Coordinates = SelectedItem.Coordinates;
        }
        else
        {
            NatureAreaName = null;
            NatureAreaDescription = null;
            Coordinates = [];
        }
    }

    private static double CalculatePolygonAreaKm2(IEnumerable<NatureAreaCoordinate> coordinates)
    {
        // Uses the spherical excess formula for area on a sphere (Earth)
        // Assumes coordinates are ordered and form a closed polygon
        const double EarthRadius = 6371.0; // km
        List<NatureAreaCoordinate>? coords = coordinates?.OrderBy(c => c.OrderIndex).ToList();
        if (coords == null || coords.Count < 3) return 0.0;
        double area = 0.0;
        int n = coords.Count;
        for (int i = 0; i < n; i++)
        {
            NatureAreaCoordinate p1 = coords[i];
            NatureAreaCoordinate p2 = coords[(i + 1) % n];
            double lat1 = p1.Latitude * Math.PI / 180.0;
            double lon1 = p1.Longitude * Math.PI / 180.0;
            double lat2 = p2.Latitude * Math.PI / 180.0;
            double lon2 = p2.Longitude * Math.PI / 180.0;
            area += (lon2 - lon1) * (2 + Math.Sin(lat1) + Math.Sin(lat2));
        }
        area = area * EarthRadius * EarthRadius / 2.0;
        return Math.Abs(area); // in km^2
    }


    #endregion
}