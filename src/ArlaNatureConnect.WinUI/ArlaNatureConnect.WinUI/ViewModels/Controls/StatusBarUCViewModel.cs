using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using System;

namespace ArlaNatureConnect.WinUI.ViewModels.Controls;

public class StatusBarUCViewModel : ViewModelBase
{
    #region Fields
    private readonly IStatusInfoServices _statusInfoServices;
    #endregion

    #region Fields Commands
    #endregion

    #region Event handlers
    #endregion

    public StatusBarUCViewModel(IStatusInfoServices statusInfoServices)
    {
        _statusInfoServices = statusInfoServices ?? throw new ArgumentNullException(nameof(statusInfoServices));
        // subscribe to status changes so the viewmodel can notify the view
        _statusInfoServices.StatusInfoChanged += StatusInfoServices_StatusInfoChanged;
    }

    #region Properties

    // Expose current busy state (maps to service.IsLoading)
    public bool IsBusy => _statusInfoServices.IsLoading;

    // Expose database connection state
    public bool HasDbConnection => _statusInfoServices.HasDbConnection;

    // Simple symbol properties (can be bound to a TextBlock/Glyph) - use emoji to avoid relying on font glyphs
    public string BusySymbol => IsBusy ? "⏳" : string.Empty;
    public string DbConnectionSymbol => HasDbConnection ? "✅" : "❌";

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
    private void StatusInfoServices_StatusInfoChanged(object? sender, EventArgs e)
    {
        // raise property changed for dependent properties
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(HasDbConnection));
        OnPropertyChanged(nameof(BusySymbol));
        OnPropertyChanged(nameof(DbConnectionSymbol));
    }
    #endregion
}