namespace ArlaNatureConnect.Core.DTOs;

// Purpose: Represents a notification for a consultant about a new assigned nature check case.
// Notes: Used to display notifications in the consultant's UI.
public sealed class ConsultantNotificationDto
{
    public Guid CaseId { get; init; }
    public Guid FarmId { get; init; }
    public string FarmName { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; init; }
    public string? Priority { get; init; }
    public string? Notes { get; init; }
}

