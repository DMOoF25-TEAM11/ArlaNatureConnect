using ArlaNatureConnect.Core.DTOs;
using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Services;

// Purpose: Application service  – loads assignment data, persists cases and manages farm registration shortcuts.
// Notes: Keeps WinUI thin by centralising orchestration and validation logic across repositories.
public interface INatureCheckCaseService
{
    /// <summary>
    /// Loads data required to assign a new nature check case.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<NatureCheckCaseAssignmentContext> LoadAssignmentContextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new nature check case based on the assignment request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<NatureCheckCase> AssignCaseAsync(NatureCheckCaseAssignmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing active nature check case with new consultant, priority, and notes.
    /// </summary>
    /// <param name="farmId">The ID of the farm whose active case should be updated.</param>
    /// <param name="request">The update request containing new consultant, priority, and notes.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The updated nature check case.</returns>
    Task<NatureCheckCase> UpdateCaseAsync(Guid farmId, NatureCheckCaseAssignmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new farm based on the registration request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Farm> SaveFarmAsync(FarmRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a farm by its identifier.
    /// </summary>
    /// <param name="farmId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteFarmAsync(Guid farmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all assigned (new) nature check cases for a specific consultant as notifications.
    /// </summary>
    /// <param name="consultantId">The ID of the consultant.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A list of notification DTOs for assigned cases.</returns>
    Task<IReadOnlyList<ConsultantNotificationDto>> GetNotificationsForConsultantAsync(Guid consultantId, CancellationToken cancellationToken = default);
}


