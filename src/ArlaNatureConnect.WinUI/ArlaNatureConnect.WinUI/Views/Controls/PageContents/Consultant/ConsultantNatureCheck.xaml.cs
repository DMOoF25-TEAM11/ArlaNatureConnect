using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.WinUI.Helpers;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.PageContents.Consultant;

/// <summary>
/// UserControl for the Consultant Nature Check view.
/// Contains search bar, action buttons, and data table for farms and nature checks.
/// </summary>
public sealed partial class ConsultantNatureCheck : UserControl
{
    public ConsultantNatureCheck()
    {
        InitializeComponent();
        Loaded += ConsultantNatureCheck_Loaded;
        DataContextChanged += ConsultantNatureCheck_DataContextChanged;
    }

    private void ConsultantNatureCheck_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is ConsultantNatureCheckViewModel viewModel)
        {
            // Subscribe to property changes to update badge visibility
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ConsultantNatureCheckViewModel.NotificationCount) ||
                    e.PropertyName == nameof(ConsultantNatureCheckViewModel.HasNotifications))
                {
                    UpdateNotificationBadge();
                }
            };
            UpdateNotificationBadge();
        }
    }

    private void UpdateNotificationBadge()
    {
        if (DataContext is ConsultantNatureCheckViewModel viewModel)
        {
            if (viewModel.HasNotifications)
            {
                NotificationBadge.Visibility = Visibility.Visible;
                NotificationCount.Text = viewModel.NotificationCount.ToString();
            }
            else
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void ConsultantNatureCheck_Loaded(object sender, RoutedEventArgs e)
    {
        // UI polish: Soft shadow (DropShadow) behind main card and search field
        if (TableCard != null)
        {
            Microsoft.UI.Xaml.Media.ThemeShadow shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
            TableCard.Shadow = shadow;
        }

        // UI polish: Subtle shadow on search field
        if (SearchBorder != null)
        {
            Microsoft.UI.Xaml.Media.ThemeShadow searchShadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
            SearchBorder.Shadow = searchShadow;
        }

        UpdateNotificationBadge();
    }

    private void NotificationButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ConsultantNatureCheckViewModel viewModel)
        {
            return;
        }

        // Create and show notification flyout
        Flyout flyout = new Flyout
        {
            Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedRight
        };

        StackPanel content = new StackPanel
        {
            Spacing = 12,
            Padding = new Thickness(16),
            MinWidth = 320,
            MaxHeight = 400
        };

        TextBlock header = new TextBlock
        {
            Text = "Notifikationer",
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        content.Children.Add(header);

        ScrollViewer scrollViewer = new ScrollViewer
        {
            MaxHeight = 300,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        StackPanel notificationsList = new StackPanel
        {
            Spacing = 8
        };

        if (viewModel.Notifications.Count == 0)
        {
            TextBlock emptyMessage = new TextBlock
            {
                Text = "Ingen nye notifikationer",
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 16)
            };
            notificationsList.Children.Add(emptyMessage);
        }
        else
        {
            foreach (ConsultantNotificationDto notification in viewModel.Notifications)
            {
                Border notificationItem = new Border
                {
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                StackPanel itemContent = new StackPanel
                {
                    Spacing = 4
                };

                TextBlock farmName = new TextBlock
                {
                    Text = notification.FarmName,
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };
                itemContent.Children.Add(farmName);

                TextBlock dateText = new TextBlock
                {
                    Text = $"Tildelt: {notification.AssignedAt:dd/MM/yyyy HH:mm}",
                    FontSize = 12,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                };
                itemContent.Children.Add(dateText);

                if (!string.IsNullOrWhiteSpace(notification.Priority))
                {
                    // Translate priority from English (database) to Danish (UI)
                    string priorityDisplay = PriorityTranslator.ToDanish(notification.Priority) ?? notification.Priority;

                    TextBlock priorityText = new TextBlock
                    {
                        Text = $"Prioritet: {priorityDisplay}",
                        FontSize = 12,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange)
                    };
                    itemContent.Children.Add(priorityText);
                }

                notificationItem.Child = itemContent;
                notificationsList.Children.Add(notificationItem);
            }
        }

        scrollViewer.Content = notificationsList;
        content.Children.Add(scrollViewer);

        flyout.Content = content;
        flyout.ShowAt(NotificationButton);
    }
}

