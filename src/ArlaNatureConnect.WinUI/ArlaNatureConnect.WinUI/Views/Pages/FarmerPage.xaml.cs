using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
/// This page orchestrates the FarmerSideMenu and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class FarmerPage : Page
{
    public FarmerPage()
    {
        InitializeComponent();

    }
}