# **Operation Contracts – UC002B: Assign Nature Check Case**


---

## **Operation Contract – UC002B: Load Assignment Data**

| Item | Description |
|------|-------------|
| **Operation** | Load all farms and consultants from the system so the Arla employee can view available farms and select a consultant for assignment. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant<br/>UC002B-SSD: Step 2<br/>UC002B-SD: Steps 15-20 |
| **Preconditions** | • The Arla employee is authenticated in the system.<br>• The employee has opened the Nature Check Cases module.<br>• The system has access to farm and consultant data. |
| **Postconditions** | • All farms in the system are loaded and displayed to the employee.<br>• Each farm shows its assignment status (whether it has an active Nature Check Case or not).<br>• Each farm with an active case shows the assigned consultant's name and priority level.<br>• All consultants (persons with Consultant role) in the system are loaded and displayed to the employee.<br>• Consultants are sorted alphabetically by name.<br>• Farms are sorted alphabetically by name. |

---

## **Operation Contract – UC002B: Assign Nature Check Case**

| Item | Description |
|------|-------------|
| **Operation** | Create a new Nature Check Case for the selected farm and assign it to the selected consultant. The system validates the assignment, creates the case, and notifies the consultant. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant<br/>UC002B-SSD: Steps 8-9<br/>UC002B-SD: Steps 41-59 |
| **Preconditions** | • The selected farm exists in the system.<br>• The selected person exists in the system and has the Consultant role.<br>• The Arla employee assigning the case is authenticated.<br>• No active Nature Check Case is currently assigned to the same farm (unless duplicate assignment is explicitly allowed).<br>• If a priority is selected, it is a valid priority level. |
| **Postconditions** | • A new Nature Check Case is created in the system with the following properties:<br>  - The case is linked to the selected farm<br>  - The case is assigned to the selected consultant<br>  - The case is marked as assigned by the authenticated Arla employee<br>  - The case status is set to "Assigned"<br>  - If provided, notes are stored with the case<br>  - If provided, priority level is stored with the case<br>  - The case creation time is recorded<br>  - The case assignment time is recorded<br>• The farm's assignment status is updated to show it has an active case.<br>• Notifications are available when consultant views them (generated from case data, not stored in database).<br>• The Arla employee sees a success confirmation message. |

---

## **Operation Contract – UC002B: Create Farm**

| Item | Description |
|------|-------------|
| **Operation** | Create a new farm in the system with its address and owner information, or update an existing farm's information. The system can reuse an existing person (farmer) if they already exist and have the Farmer role, allowing a farmer to own multiple farms with different CVR numbers. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant (farm creation during assignment)<br/>UC002B-SSD: Steps 4-5<br/>UC002B-SD: Farm creation flow |
| **Preconditions** | • All required farm information is provided (farm name, CVR number).<br>• All required address information is provided (street, city, postal code, country).<br>• All required owner information is provided (first name, last name, email).<br>• The CVR number is unique in the system (no other farm has the same CVR).<br>• If the owner email already exists, the person must have the Farmer role.<br>• If updating an existing farm, the farm exists in the system. |
| **Postconditions** | • If creating a new farm:<br>  - A new farm is created in the system with the provided information<br>  - A new address is created and linked to the farm<br>  - **If owner email does not exist:** A new person (owner) is created with the Farmer role and linked to the farm<br>  - **If owner email exists and person has Farmer role:** The existing person is reused and linked to the new farm (allowing a farmer to own multiple farms)<br>  - **If owner email exists but person does not have Farmer role:** The operation fails with an error message<br>  - The farm is available for selection in the assignment process<br>• If updating an existing farm:<br>  - The farm's information is updated<br>  - The farm's address information is updated<br>  - The farm's owner information is updated<br>• The farm appears in the farm list with the updated information. |
| **Validation** | • Farm name and CVR are required and validated.<br>• CVR must be unique in the system.<br>• Owner first name, last name, and email are required.<br>• If owner email exists, the person must have the Farmer role to allow multiple farms. |
| **Error handling** | • `InvalidOperationException` if CVR already exists: "En gård med CVR-nummer '[CVR]' findes allerede i systemet. Vælg et andet CVR-nummer."<br>• `InvalidOperationException` if owner email exists but person does not have Farmer role: "En person med e-mail '[Email]' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen."<br>• `ArgumentException` for invalid input (missing required fields).<br>• `DbUpdateException` with specific error messages for database constraint violations (handled by ViewModel error extraction). |

---

## **Operation Contract – UC002B: Delete Farm**

| Item | Description |
|------|-------------|
| **Operation** | Remove a farm from the system. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant (farm deletion during assignment)<br/>UC002B-SSD: Farm management |
| **Preconditions** | • The farm exists in the system.<br>• The farm has no active Nature Check Cases assigned to it. |
| **Postconditions** | • The farm is removed from the system.<br>• The farm no longer appears in the farm list.<br>• The farm cannot be selected for new assignments. |

---

## **Operation Contract – UC002B: Update Nature Check Case**

| Item | Description |
|------|-------------|
| **Operation** | Update an existing active Nature Check Case for a farm with new consultant, priority, and notes. The system validates the update and updates the case information. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant<br/>UC002B-SSD: Steps 8-9 (update flow)<br/>UC002B-SD: Update sequence |
| **Preconditions** | • The selected farm exists in the system.<br>• The farm has an active Nature Check Case (status "Assigned" or "InProgress").<br>• The selected person exists in the system and has the Consultant role.<br>• The Arla employee updating the case is authenticated.<br>• If a priority is selected, it is a valid priority level. |
| **Postconditions** | • The existing active Nature Check Case is updated with the following properties:<br>  - The case is reassigned to the selected consultant (if changed)<br>  - The case priority is updated (if changed)<br>  - The case notes are updated (if changed)<br>  - The case assignment time is updated to reflect the modification<br>• The farm's assignment status remains "Tilføjet" (Assigned).<br>• Notifications are regenerated from updated case data when consultant views them (not stored in database).<br>• The Arla employee sees a success confirmation message: "Natur Check opgave er opdateret for [FarmName]." |

---

## **Operation Contract – UC002B: Load Consultant Notifications**

| Item | Description |
|------|-------------|
| **Operation** | Load all new Nature Check Case assignments for a specific consultant so the consultant can see what work has been assigned to them. |
| **Cross References** | UC002B – Assign Nature Check Case to Consultant (consultant notification flow)<br/>UC002B-SSD: Notification display |
| **Preconditions** | • The consultant exists in the system.<br>• The consultant is authenticated or selected in the consultant interface. |
| **Postconditions** | • All Nature Check Cases assigned to the consultant with status "Assigned" are loaded.<br>• For each assigned case, the following information is displayed:<br>  - The name of the farm assigned to the consultant<br>  - When the case was assigned<br>  - The priority level of the case (if set)<br>  - Any notes about the case (if provided)<br>• Cases are sorted by assignment date, with newest assignments first.<br>• If no cases are assigned, an empty list is shown. |

---

