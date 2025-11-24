# **Operation Contract – UC02b: LoadFarmsAndConsultantsAsync()**

| Item | Description |
|------|-------------|
| **Operation** | `LoadFarmsAndConsultantsAsync(CancellationToken cancellationToken = default)` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • The Arla employee is authenticated (UC01).<br>• The employee has opened the Nature Check Cases module. |
| **Input** | `CancellationToken cancellationToken` (optional) |
| **Output** | `Task<(IReadOnlyList<Farm>, IReadOnlyList<Person>)>` - A tuple containing a list of all farms and a list of all consultants (persons with Consultant role). |
| **Postconditions** | • All farms are loaded from the database via `IFarmRepository.GetAllAsync()`.<br>• All consultants are loaded from the database via `IPersonRepository.GetPersonsByRoleAsync("Consultant")`.<br>• The lists are returned to the ViewModel for display in the UI. |


# **Operation Contract – UC02b: AssignCaseAsync(farmId, consultantId, assignedByPersonId, notes, allowDuplicate)**

| Item | Description |
|------|-------------|
| **Operation** | `AssignCaseAsync(Guid farmId, Guid consultantId, Guid assignedByPersonId, string? notes, bool allowDuplicate = false, CancellationToken cancellationToken = default)` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • The selected farm exists in the system (validated via `IFarmRepository.GetByIdAsync(farmId)`).<br>• The selected person exists and has the *Consultant* role (validated via `IPersonRepository.GetByIdAsync(consultantId)` and role check).<br>• The `assignedByPersonId` refers to the authenticated Arla employee.<br>• No active NatureCheckCase exists for this farm (unless `allowDuplicate` is true). |
| **Input** | `farmId` (Guid), `consultantId` (Guid), `assignedByPersonId` (Guid), `notes` (string?, optional), `allowDuplicate` (bool, default false), `cancellationToken` (CancellationToken, optional) |
| **Output** | `Task<NatureCheckCase>` - The newly created and assigned NatureCheckCase with status **Assigned**. |
| **Postconditions** | • A new `NatureCheckCase` object is created with:<br>  - `FarmId` = farmId<br>  - `ConsultantId` = consultantId<br>  - `AssignedByPersonId` = assignedByPersonId<br>  - `Status` = NatureCheckCaseStatus.Assigned<br>  - `Notes` = notes<br>  - `CreatedAt` = DateTimeOffset.UtcNow<br>  - `AssignedAt` = DateTimeOffset.UtcNow<br>• The case is stored in the database via `INatureCheckCaseRepository.AddAsync()`.<br>• A notification is sent to the consultant via `INotificationService.NotifyConsultantAsync()`.<br>• Success message is displayed via `IAppMessageService.AddInfoMessage()`. |

# **Operation Contract – UC02b: NotifyConsultantAsync(consultantId, caseId)**

| Item | Description |
|------|-------------|
| **Operation** | `NotifyConsultantAsync(Guid consultantId, Guid caseId, CancellationToken cancellationToken = default)` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • A NatureCheckCase has just been created and assigned to the consultant.<br>• The consultant exists in the system. |
| **Input** | `consultantId` (Guid), `caseId` (Guid), `cancellationToken` (CancellationToken, optional) |
| **Output** | `Task` - Completes when the notification has been sent. |
| **Postconditions** | • The consultant receives a notification (internal message or alert) about the new assignment.<br>• The consultant can now proceed with UC03 – Create Nature Check. |
