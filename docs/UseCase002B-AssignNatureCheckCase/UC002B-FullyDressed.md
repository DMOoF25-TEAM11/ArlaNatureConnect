# Fully Dressed Use Case – UC002B: Assign Nature Check Case to Consultant

**Use Case ID:** UC002B  
**Use Case Name:** Assign Nature Check Case to Consultant  
**Scope:** System prototype (Arla Nature Connect)  
**Level:** User goal  
**Primary Actor:** Arla Employee (Admin / Sustainability Staff)  
**Secondary Actors:** Consultant (receives the case via database notification)  
**Stakeholders and Interests:**
- **Arla employees** want to assign farms to consultants so Nature Checks can be planned and executed.
- **Consultants** want to receive clear assignments they can act on, with priority levels and notes.
- **Farmers** want structured coordination and transparency around upcoming Nature Checks.

---

## Preconditions

1. The Arla employee is authenticated in the system (UC01).
2. The employee has access to the Nature Check Cases module.
3. The system has access to farm and consultant data.
4. For creating a new case: The farm exists in the system or can be created via inline farm creation.
5. For creating a new case: No active NatureCheckCase is already assigned to the same farm (unless update is intended).
6. For updating an existing case: An active NatureCheckCase (status "Assigned" or "InProgress") exists for the farm.
7. The consultant exists in the system and has the *Consultant* role.

---

## Postconditions

### If creating a new case:
1. A new NatureCheckCase is created with status **Assigned**.
2. The case is linked to the selected farm.
3. The case is assigned to the selected consultant.
4. The case is marked as assigned by the authenticated Arla employee.
5. Priority level (if selected) is stored in the database (English format: "Low", "Medium", "High", "Urgent").
6. Notes (if provided) are stored with the case.
7. The case creation time is recorded.
8. The case assignment time is recorded.
9. A database notification is created for the consultant (consultant sees notification badge in UI).
10. The farm's assignment status is updated to show "Tilføjet" (Assigned).

### If updating an existing case:
1. The existing active NatureCheckCase is updated with new consultant, priority, and notes.
2. The case assignment time is updated to reflect the modification.
3. If consultant changed, a database notification is created for the new consultant.
4. The farm's assignment status remains "Tilføjet" (Assigned).

### In both cases:
1. The consultant can now begin UC03 – Create Nature Check.
2. The Arla employee sees a success confirmation message.

---

## Main Success Scenario

1. The Arla employee opens the *Nature Check Cases* module.
2. The system loads available farms (with status "Tilføjet" / "Ikke tilføjet" and priority badges) and consultants, and displays them.
3. The employee selects an existing farm (or creates one via inline farm creation if missing).
4. The system presents the selected farm's summary information.
5. **If the farm has an active case:**
   5a. The system auto-populates the form with existing assignment data (consultant, priority converted to Danish, notes).
   5b. The system displays button text "Opdater natur Check opgave" (Update Nature Check task).
6. **If the farm has no active case:**
   6a. The system displays button text "Lav natur Check opgave" (Create Nature Check task).
7. The employee selects a consultant, priority level (Lav, Normal, Høj, Haster), and optional notes.
8. The employee confirms the assignment by clicking the button.
9. The system validates the farm, consultant role, priority format, and any business rules.
10. **If farm has no active case:**
    10a. A new Nature Check Case is created with status **Assigned**, priority (stored in English), and linked to the chosen consultant and farm.
    10b. The system shows success message "Natur Check opgave er oprettet for [FarmName]."
11. **If farm has an active case:**
    11a. The existing active Nature Check Case is updated with the new consultant, priority (stored in English), and notes.
    11b. The case assignment time is updated.
    11c. The system shows success message "Natur Check opgave er opdateret for [FarmName]."
12. The system creates a database notification for the consultant (consultant sees notification badge in UI).
13. The system updates the farm list to show "Tilføjet" status.
14. The consultant can continue in **UC03 – Create Nature Check** to plan the visit.

---

## Extensions (Alternatives)

### 3a. Farm not found:
The employee selects "Tilføj ny gård" → System shows farm creation form → Employee enters farm details (name, CVR, address, owner email) → System validates input:
- **3a.1. If CVR already exists:** System displays error "En gård med CVR-nummer '[CVR]' findes allerede i systemet. Vælg et andet CVR-nummer." → Employee must change CVR.
- **3a.2. If owner email exists and person has Farmer role:** System reuses existing person and creates new farm linked to them → Employee returns to assignment form with new farm.
- **3a.3. If owner email exists but person does not have Farmer role:** System displays error "En person med e-mail '[Email]' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen." → Employee must use different email or contact administrator.
- **3a.4. If owner email does not exist:** System creates new person with Farmer role and new farm → Employee returns to assignment form with new farm.

### 5a. Farm has active case:
The system auto-populates the form with existing assignment data (consultant, priority converted to Danish, notes). The button text changes to "Opdater natur Check opgave" (Update Nature Check task). Employee can modify consultant, priority, and notes, then click the button to update the existing case. The system updates the active case instead of creating a new one.

### 7a. No consultant selected:
The system prompts the employee to choose a consultant before continuing.

### 7b. No priority selected:
Priority is optional - the system allows assignment without priority.

### 9a. Selected user is not a consultant:
The system rejects the selection and asks for a valid consultant.

### 9b. Farm does not exist:
The system cannot complete the assignment and directs the employee to register the farm first (inline creation available).

### 9c. Consultant does not exist or is inactive:
The system prevents assignment and requests another consultant.

### 9d. Farm already has an active case (and allowDuplicate is false):
The system displays error: "Gården har allerede en aktiv Natur Check opgave."  
The employee can either:
- Cancel the operation, or
- Select a different farm, or
- Update the existing case (if button text shows "Opdater natur Check opgave").

### 9e. Priority value is invalid:
The system validates priority format (must be "Low", "Medium", "High", or "Urgent" in English) and rejects invalid values.

### 11a. Notification creation fails:
The case is still created, but the consultant may not see the notification immediately. The system logs the error for investigation.

---

## Special Requirements

### Technology Requirements:
1. Priority values are displayed in Danish ("Lav", "Normal", "Høj", "Haster") in the UI but stored in English ("Low", "Medium", "High", "Urgent") in the database.
2. Notifications are database-based (consultant sees notification badge in UI), not email/SMS.
3. Data aggregation: Service aggregates Farm + Person + Address + Case data into DTOs for efficient UI display.

### Business Rules:
1. Farm list displays assignment status ("Tilføjet" / "Ikke tilføjet") and priority badges for farms with active cases.
2. When a farm with an active case is selected, the form is auto-populated with existing assignment data and the button text changes to "Opdater natur Check opgave" (Update Nature Check task).
3. When a farm with no active case is selected, the button text displays "Lav natur Check opgave" (Create Nature Check task).
4. The system can update existing active cases (status "Assigned" or "InProgress") with new consultant, priority, and notes without creating duplicate cases.
5. **Farm creation:** A farmer (person with Farmer role) can own multiple farms with different CVR numbers. When creating a new farm, if the owner email already exists and the person has the Farmer role, the system reuses the existing person and links the new farm to them. This allows efficient management of farmers with multiple farm operations.
6. **CVR uniqueness:** Each farm must have a unique CVR number. The system validates CVR uniqueness before creating a new farm.
7. **Error handling:** The system provides specific, user-friendly error messages for validation failures and database constraint violations (duplicate email or CVR) to help users understand and correct issues.

### Performance Requirements:
- Response time for loading farms and consultants: < 500 ms under normal load.
- Response time for creating/updating a case: < 1 second under normal load.

### Security Requirements:
- Only authenticated Arla employees can access this use case.
- Actions must be audited and validated server-side.
- Consultant role validation must be enforced at the database level.

---

## Technology & Data Variations List

### Priority Translation:
- **UI Display (Danish):** "Lav", "Normal", "Høj", "Haster"
- **Database Storage (English):** "Low", "Medium", "High", "Urgent"
- **Translation Logic:** Service layer handles conversion between UI and database formats.

### Notification Mechanism:
- **Type:** Database-based notifications (not email/SMS)
- **Storage:** Notification entries in database
- **Display:** Consultant sees notification badge in UI
- **Trigger:** Created automatically when case is assigned or updated (if consultant changed)

### Data Aggregation:
- **Service Layer:** Aggregates data from multiple repositories (Farm, Person, Address, NatureCheckCase)
- **DTOs:** FarmAssignmentOverviewDto contains combined data for efficient UI display
- **Performance:** Single service call returns all necessary data for assignment view

### Status Values:
- **Valid Statuses:** "Assigned", "InProgress", "Completed", "Cancelled"
- **Default Status:** "Assigned" (for new cases)
- **Active Statuses:** "Assigned", "InProgress" (can be updated)

### Database Design:
- **Standard Database Approach:** Uses stored procedures, views, and triggers (not Entity Framework)
- **Stored Procedures:** All CRUD operations implemented as stored procedures
- **Views:** Complex queries implemented as database views
- **Triggers:** Audit logging and validation handled via database triggers
- **Constraints:** Check constraints enforce Status and Priority values

---

## Related Information

**Backlog relations:**
- B01–B06 (farm & user association)  
- B07 (Mark farm for Nature Check & assign consultant)
- C01 (Create Nature Check – dependency)  
- E01–E02 (Notifications - database-based, not email/SMS)

**Cross References:**
- UC01 – Login & Role Access (authentication prerequisite)
- UC02 – Administrate Farms and Users (farm creation dependency)
- UC03 – Create Nature Check (enables consultant to proceed)

**Dependencies:**
- Requires Farms and Persons tables to exist (from UC001/UC002)
- Requires Roles table with "Consultant" and "Employee" roles
- Requires Addresses table for farm and person addresses


