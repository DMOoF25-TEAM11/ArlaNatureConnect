using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;


namespace ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

/// <summary>
/// Generic base UserControl for side-menu user controls.
/// Handles subscribing/unsubscribing to DataContext PropertyChanged and optionally calling
/// a parameterless InitializeAsync() method on the view-model if present.
/// Derived side-menu controls can override hooks to react to VM set / property changes.
/// </summary>
public abstract class SideMenuBaseUC : UserControl
{
    private INotifyPropertyChanged? _previousViewModel;

    protected SideMenuBaseUC()
    {
        DataContextChanged += SideMenuBaseUC_DataContextChanged;
        Unloaded += SideMenuBaseUC_Unloaded;
    }

    private void SideMenuBaseUC_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _previousViewModel = null;
        }
    }

    private async void SideMenuBaseUC_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from previous VM
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Subscribe to new VM if it supports notifications
        if (args.NewValue is INotifyPropertyChanged vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;
            _previousViewModel = vm;

            try
            {
                OnViewModelSet(vm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SideMenuBaseUC.OnViewModelSet threw: {ex}");
            }
        }
        else
        {
            _previousViewModel = null;
        }

        //Regardless of whether the VM implements INotifyPropertyChanged, attempt to invoke
        //a parameterless InitializeAsync() method on the DataContext(if present).
        if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled && args.NewValue != null)
        {
            try
            {
                MethodInfo? mi = args.NewValue.GetType().GetMethod("InitializeAsync", Type.EmptyTypes);
                if (mi != null && typeof(Task).IsAssignableFrom(mi.ReturnType))
                {
                    try
                    {
                        Task? task = (Task?)mi.Invoke(args.NewValue, null);
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                        }
                    }
                    catch (TargetInvocationException tie)
                    {
                        // Unwrap and log inner exception
                        Debug.WriteLine($"SideMenuBaseUC.InitializeAsync invocation failed: {tie.InnerException ?? tie}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"SideMenuBaseUC.InitializeAsync invocation failed: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SideMenuBaseUC reflection error when invoking InitializeAsync: {ex}");
            }
        }
    }

    /// <summary>
    /// Hook invoked when a new view-model is set on the DataContext. Override to react in derived controls.
    /// </summary>
    protected virtual void OnViewModelSet(INotifyPropertyChanged viewModel)
    {
        UpdateButtonStyles();
    }

    /// <summary>
    /// Hook invoked when a property on the current view-model changes. Override to react in derived controls.
    /// </summary>
    protected virtual void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentNavigationTag")
        {
            UpdateButtonStyles();
        }
    }

    protected virtual void UpdateButtonStyles()
    {
        // Implement style updates according to the actual ViewModel used by the page.
        // Keep this minimal to avoid casting errors when different VM types are used.
    }
}
