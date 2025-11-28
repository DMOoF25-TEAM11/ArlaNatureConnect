using System.Collections.ObjectModel;
using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ConsultantNatureCheck view - handles notifications and nature check case display for consultants.
/// </summary>
public class ConsultantNatureCheckViewModel : ViewModelBase
{
    private readonly INatureCheckCaseService _natureCheckCaseService;
    private readonly ObservableCollection<ConsultantNotificationDto> _notifications = new();
    private int _notificationCount;
    private Person? _selectedConsultant;

    public ConsultantNatureCheckViewModel(INatureCheckCaseService natureCheckCaseService)
    {
        _natureCheckCaseService = natureCheckCaseService ?? throw new ArgumentNullException(nameof(natureCheckCaseService));
    }

    /// <summary>
    /// Gets the list of notifications for the selected consultant.
    /// </summary>
    public ObservableCollection<ConsultantNotificationDto> Notifications => _notifications;

    /// <summary>
    /// Gets the count of unread notifications.
    /// </summary>
    public int NotificationCount
    {
        get => _notificationCount;
        private set
        {
            if (_notificationCount == value)
            {
                return;
            }

            _notificationCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNotifications));
        }
    }

    /// <summary>
    /// Gets a value indicating whether there are any notifications.
    /// </summary>
    public bool HasNotifications => NotificationCount > 0;

    /// <summary>
    /// Sets the selected consultant and loads notifications for that consultant.
    /// </summary>
    public Person? SelectedConsultant
    {
        get => _selectedConsultant;
        set
        {
            if (_selectedConsultant == value)
            {
                return;
            }

            _selectedConsultant = value;
            OnPropertyChanged();
            _ = LoadNotificationsAsync();
        }
    }

    /// <summary>
    /// Loads notifications for the currently selected consultant.
    /// </summary>
    public async Task LoadNotificationsAsync()
    {
        if (_selectedConsultant == null)
        {
            _notifications.Clear();
            NotificationCount = 0;
            return;
        }

        try
        {
            IReadOnlyList<ConsultantNotificationDto> notifications = await _natureCheckCaseService
                .GetNotificationsForConsultantAsync(_selectedConsultant.Id)
                .ConfigureAwait(true); // Keep on UI thread for collection updates

            _notifications.Clear();
            foreach (ConsultantNotificationDto notification in notifications)
            {
                _notifications.Add(notification);
            }

            NotificationCount = _notifications.Count;
        }
        catch
        {
            _notifications.Clear();
            NotificationCount = 0;
        }
    }
}

