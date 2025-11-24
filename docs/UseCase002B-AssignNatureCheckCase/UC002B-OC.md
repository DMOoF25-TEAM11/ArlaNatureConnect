# **Operation Contract – UC02b: LoadFarmsAndConsultantsAsync()**

| Item | Description |
|------|-------------|
| **Operation** | Load all farms and consultants from the database and return them to the UI for display in the assignment form. |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • The Arla employee is authenticated (UC01).<br>• The employee has opened the Nature Check Cases module. |
| **Input** | None (operation is triggered when the module is opened) |
| **Output** | A list of all farms registered in the system and a list of all consultants (persons with Consultant role) that can be assigned to cases. |
| **Postconditions** | • All farms are loaded from the database via `IFarmRepository.GetAllAsync()`.<br>• All consultants are loaded from the database via `IPersonRepository.GetPersonsByRoleAsync("Consultant")`.<br>• The lists are returned to the ViewModel for display in the UI. |


# **Operation Contract – UC02b: AssignCaseAsync(farmId, consultantId, assignedByPersonId, notes, allowDuplicate)**

| Item | Description |
|------|-------------|
| **Operation** | Create a new Nature Check Case for the selected farm and assign it to the selected consultant. The system validates that both the farm and consultant exist, creates the case with status "Assigned", stores it in the database, sends a notification to the consultant, and displays a success message to the Arla employee. |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • The selected farm exists in the system (validated via `IFarmRepository.GetByIdAsync(farmId)`).<br>• The selected person exists and has the *Consultant* role (validated via `IPersonRepository.GetByIdAsync(consultantId)` and role check).<br>• The `assignedByPersonId` refers to the authenticated Arla employee.<br>• No active NatureCheckCase exists for this farm (unless `allowDuplicate` is true). |
| **Input** | • The ID of the selected farm<br>• The ID of the selected consultant<br>• The ID of the Arla employee assigning the case<br>• Optional notes about the case<br>• Optional flag to allow duplicate cases for the same farm |
| **Output** | The newly created Nature Check Case with status "Assigned", containing all the assignment details. |
| **Postconditions** | • A new `NatureCheckCase` object is created with:<br>  - `FarmId` = farmId<br>  - `ConsultantId` = consultantId<br>  - `AssignedByPersonId` = assignedByPersonId<br>  - `Status` = NatureCheckCaseStatus.Assigned<br>  - `Notes` = notes<br>  - `CreatedAt` = DateTimeOffset.UtcNow<br>  - `AssignedAt` = DateTimeOffset.UtcNow<br>• The case is stored in the database via `INatureCheckCaseRepository.AddAsync()`.<br>• A notification is sent to the consultant via `INotificationService.NotifyConsultantAsync()`.<br>• Success message is displayed via `IAppMessageService.AddInfoMessage()`. |

# **Operation Contract – UC02b: NotifyConsultantAsync(consultantId, caseId)**

| Item | Description |
|------|-------------|
| **Operation** | Send a notification to the assigned consultant informing them that a new Nature Check Case has been assigned to them. The notification alerts the consultant so they can begin working on the case. |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • A NatureCheckCase has just been created and assigned to the consultant.<br>• The consultant exists in the system. |
| **Input** | • The ID of the consultant who should receive the notification<br>• The ID of the newly created Nature Check Case |
| **Output** | Confirmation that the notification has been successfully sent to the consultant. |
| **Postconditions** | • The consultant receives a notification (internal message or alert) about the new assignment.<br>• The consultant can now proceed with UC03 – Create Nature Check. |
