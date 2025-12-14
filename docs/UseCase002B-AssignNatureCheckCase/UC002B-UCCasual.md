# **Casual Use Case – UC002B Assign Nature Check Case to Consultant**

**Use case:** UC002B – Assign Nature Check Case to Consultant  
**Scope:** The prototype of the Arla Nature Connect system  
**Level:** User goal  
**Primary Actors:** Arla Employee (Admin / Sustainability Staff)  
**Secondary Actors:** Consultant (receives the case via notification)

---

### **Stakeholders and Interests**
- **Arla employees** want to assign farms to consultants so Nature Checks can be planned and executed.  
- **Consultants** want to receive clear assignments they can act on, with priority levels and notes.  
- **Farmers** want structured coordination and transparency around upcoming Nature Checks.

---

### **Preconditions**
- The Arla employee is authenticated (UC01).  
- The employee can either:
  - select an existing farm already registered in the system, **or**
  - create a new farm first through UC02 (farm creation is available).
- The consultant exists in the system and has the *Consultant* role.
- No active NatureCheckCase is currently assigned to the same farm.

---

### **Postconditions**
- A new **NatureCheckCase** is created with status **Assigned**.  
- The selected consultant is linked to the case.  
- Priority level (if selected) is stored in the database (English format: "Low", "Medium", "High", "Urgent").  
- A database notification is created for the consultant (consultant sees notification badge in UI).  
- The consultant can now continue in **UC03 – Create Nature Check**.

---

### **Main Success Scenario**
1. The Arla employee opens the *Nature Check Cases* module.  
2. The system loads available farms (with status "Tilføjet" / "Ikke tilføjet" and priority badges) and consultants, and displays them.  
3. The employee selects an existing farm (or creates one via inline farm creation if missing).  
4. The system presents the selected farm's summary information.  
5. If the farm has an active case, the system auto-populates the form with existing assignment data (consultant, priority, notes) and displays button text "Opdater natur Check opgave" (Update Nature Check task).  
6. If the farm has no active case, the system displays button text "Lav natur Check opgave" (Create Nature Check task).  
7. The employee selects a consultant, priority level (Lav, Normal, Høj, Haster), and optional notes.  
8. The employee confirms the assignment by clicking the button.  
9. The system validates the farm, consultant role, priority format, and any business rules.  
10. **If farm has no active case:** A new Nature Check Case is created with status **Assigned**, priority (stored in English), and linked to the chosen consultant and farm. The system shows success message "Natur Check opgave er oprettet for [FarmName]."  
10a. **If farm has an active case:** The existing active Nature Check Case is updated with the new consultant, priority (stored in English), and notes. The case assignment time is updated. The system shows success message "Natur Check opgave er opdateret for [FarmName]."  
11. The system creates a database notification for the consultant (consultant sees notification badge in UI).  
12. The system updates the farm list to show "Tilføjet" status.  
13. The consultant can continue in **UC03 – Create Nature Check** to plan the visit.

---

### **Extensions (Alternatives)**
- **3a. Farm not found:**  
  The employee selects "Tilføj ny gård" → System shows farm creation form → Employee enters farm details (name, CVR, address, owner email) → System validates input:
  - **3a.1. If CVR already exists:** System displays error "En gård med CVR-nummer '[CVR]' findes allerede i systemet. Vælg et andet CVR-nummer." → Employee must change CVR.
  - **3a.2. If owner email exists and person has Farmer role:** System reuses existing person and creates new farm linked to them → Employee returns to assignment form with new farm.
  - **3a.3. If owner email exists but person does not have Farmer role:** System displays error "En person med e-mail '[Email]' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen." → Employee must use different email or contact administrator.
  - **3a.4. If owner email does not exist:** System creates new person with Farmer role and new farm → Employee returns to assignment form with new farm.

- **5a. Farm has active case:**  
  The system auto-populates the form with existing assignment data (consultant, priority converted to Danish, notes). The button text changes to "Opdater natur Check opgave" (Update Nature Check task). Employee can modify consultant, priority, and notes, then click the button to update the existing case. The system updates the active case instead of creating a new one.

- **7a. No consultant selected:**  
  The system prompts the employee to choose a consultant before continuing.

- **7b. No priority selected:**  
  Priority is optional - the system allows assignment without priority.

- **9a. Selected user is not a consultant:**  
  The system rejects the selection and asks for a valid consultant.

- **9b. Farm does not exist:**  
  The system cannot complete the assignment and directs the employee to register the farm first (inline creation available).

- **9c. Consultant does not exist or is inactive:**  
  The system prevents assignment and requests another consultant.

- **9d. Farm already has an active case (and allowDuplicate is false):**  
  The system displays error: "Gården har allerede en aktiv Natur Check opgave."  
  The employee can either:
  - Cancel the operation, or
  - Select a different farm.

- **9e. Priority value is invalid:**  
  The system validates priority format (must be "Low", "Medium", "High", or "Urgent" in English) and rejects invalid values.

- **11a. Notification creation fails:**  
  The case is still created, but the consultant may not see the notification immediately. The system logs the error for investigation.

---

### **Special Requirements**
- Priority values are displayed in Danish ("Lav", "Normal", "Høj", "Haster") in the UI but stored in English ("Low", "Medium", "High", "Urgent") in the database.
- Notifications are database-based (consultant sees notification badge in UI), not email/SMS.
- Farm list displays assignment status ("Tilføjet" / "Ikke tilføjet") and priority badges for farms with active cases.
- When a farm with an active case is selected, the form is auto-populated with existing assignment data and the button text changes to "Opdater natur Check opgave" (Update Nature Check task).
- When a farm with no active case is selected, the button text displays "Lav natur Check opgave" (Create Nature Check task).
- The system can update existing active cases (status "Assigned" or "InProgress") with new consultant, priority, and notes without creating duplicate cases.
- **Farm creation:** A farmer (person with Farmer role) can own multiple farms with different CVR numbers. When creating a new farm, if the owner email already exists and the person has the Farmer role, the system reuses the existing person and links the new farm to them. This allows efficient management of farmers with multiple farm operations.
- **CVR uniqueness:** Each farm must have a unique CVR number. The system validates CVR uniqueness before creating a new farm.
- **Error handling:** The system provides specific, user-friendly error messages for validation failures and database constraint violations (duplicate email or CVR) to help users understand and correct issues.

---

