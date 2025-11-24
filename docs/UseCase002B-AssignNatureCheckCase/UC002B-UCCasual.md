# **Casual Use Case – UC02b Assign Nature Check Case to Consultant**

**Use case:** UC02b – Assign Nature Check Case to Consultant  
**Scope:** The prototype of the Arla Nature Connect system  
**Level:** User goal  
**Primary Actors:** Arla Employee (Admin / Sustainability Staff)  

---

### **Stakeholders and Interests**
- **Arla employees** want to assign farms to consultants so Nature Checks can be planned and executed.  
- **Consultants** want to receive clear assignments they can act on.  
- **Farmers** want structured coordination and transparency around upcoming Nature Checks.

---

### Preconditions
- The Arla employee is authenticated (UC01).  
- The employee can either:
  - select an existing farm already registered in the system, **or**
  - create a new farm first through UC02.
- The consultant exists in the system and has the *Consultant* role.
- No active NatureCheckCase is currently assigned to the same farm (soft rule).


---

### **Postconditions**
- A new **NatureCheckCase** is created with status **Assigned**.  
- The selected consultant is linked to the case.  
- A system notification is sent to the consultant (E01–E02).  
- The consultant can now continue in **UC03 – Create Nature Check**.

---

### **Main Success Scenario**
1. The Arla employee opens the *Nature Check Cases* module.  
2. The system calls `NatureCheckCaseService.LoadFarmsAndConsultantsAsync()` which:
   - Loads all farms via `IFarmRepository.GetAllAsync()`
   - Loads all consultants via `IPersonRepository.GetPersonsByRoleAsync("Consultant")`
3. The system displays a list of all farms and consultants registered in the system.  
4. The employee selects a farm.  
5. If the farm does not exist, the employee first creates it via UC02.  
6. The system shows the farm's details (name, CVR, address, farmer).  
7. The employee clicks **"Create Nature Check Case"**.  
8. The system opens a form asking the employee to select:  
   - a consultant to assign (from the list of consultants)  
   - optional internal notes (batch, priority, comments)  
9. The employee saves the case.  
10. The system calls `NatureCheckCaseService.AssignCaseAsync()` which:
    - Validates that the farm exists via `IFarmRepository.GetByIdAsync(farmId)`
    - Validates that the selected person exists and has the *Consultant* role via `IPersonRepository.GetByIdAsync(consultantId)`
    - Creates a new **NatureCheckCase** entity with:
      - `FarmId` = selected farm
      - `ConsultantId` = selected consultant
      - `AssignedByPersonId` = current authenticated Arla employee
      - `Status` = NatureCheckCaseStatus.Assigned
      - `Notes` = optional notes
      - `CreatedAt` = DateTimeOffset.UtcNow
      - `AssignedAt` = DateTimeOffset.UtcNow
    - Saves the case via `INatureCheckCaseRepository.AddAsync()`
    - Sends a notification via `INotificationService.NotifyConsultantAsync()`
    - Displays success message via `IAppMessageService.AddInfoMessage()`
11. The system displays a confirmation message.  
12. The consultant receives a notification about the new assignment.  
13. The consultant can now proceed to **UC03 – Create Nature Check** and arrange the visit date with the farmer.

---

### **Extensions (Alternatives)**
- **3a. Farm not found:**  
  The employee selects “Create Farm” → redirects to UC02 → returns to UC02b afterward.

- **8a. No consultant selected:**  
  The system displays an error message via `IAppMessageService.AddErrorMessage()` and prevents saving.

- **8b. Selected user is not a consultant:**  
  The system validates the role via `IPersonRepository.GetByIdAsync()` and role check.  
  If validation fails, the system rejects the selection via `IAppMessageService.AddErrorMessage()` and asks for a valid consultant.

- **10a. Farm does not exist:**  
  The system validates via `IFarmRepository.GetByIdAsync(farmId)`.  
  If the farm is not found, the system displays an error message and redirects to UC02 to create the farm.

- **10b. Consultant does not exist or is inactive:**  
  The system validates via `IPersonRepository.GetByIdAsync(consultantId)` and checks `IsActive` status.  
  If validation fails, the system displays an error message and prevents saving.

- **10c. Farm already has an active case (and allowDuplicate is false):**  
  The system checks for existing active cases via `INatureCheckCaseRepository`.  
  The system prompts: "This farm already has an active Nature Check Case. Create another?"  
  - If *yes* → continue with `allowDuplicate = true`.  
  - If *no* → cancel the operation.
