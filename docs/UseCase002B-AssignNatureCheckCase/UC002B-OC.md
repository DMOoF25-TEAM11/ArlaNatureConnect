# **Operation Contract – UC02b: LoadFarms()**

| Item | Description |
|------|-------------|
| **Operation** | `LoadFarms()` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | The Arla employee is authenticated and has opened the Nature Check Cases module. |
| **Input** | None |
| **Output** | The system displays a list of farms currently registered in the system. |
| **Postconditions** | A list of available farms is populated from the database and shown in the UI. |


# **Operation Contract – UC02b: CreateNatureCheckCase(farmId, consultantId, notes)**

| Item | Description |
|------|-------------|
| **Operation** | `CreateNatureCheckCase(farmId, consultantId, notes)` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | • The selected farm exists in the system.<br>• The selected user has the *Consultant* role.<br>• No active NatureCheckCase exists for this farm (soft rule). |
| **Input** | `farmId`, `consultantId`, `notes` |
| **Output** | A new NatureCheckCase is created with status **Assigned**. |
| **Postconditions** | • A NatureCheckCase object is stored in the database.<br>• The case is associated with the selected farm and consultant.<br>• Timestamp and notes are saved. |

# **Operation Contract – UC02b: NotifyConsultant(consultantId, caseId)**

| Item | Description |
|------|-------------|
| **Operation** | `NotifyConsultant(consultantId, caseId)` |
| **Cross References** | UC02b – Assign Nature Check Case to Consultant |
| **Preconditions** | A NatureCheckCase has just been created and assigned to the consultant. |
| **Input** | `consultantId`, `caseId` |
| **Output** | A notification is delivered to the consultant (internal message or alert). |
| **Postconditions** | The consultant is informed about the new assignment and can proceed with UC03 – Create Nature Check. |
