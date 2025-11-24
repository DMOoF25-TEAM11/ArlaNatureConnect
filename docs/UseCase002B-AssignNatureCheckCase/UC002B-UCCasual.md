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
2. The system displays a list of all farms registered in the system.  
3. The employee selects a farm.  
4. If the farm does not exist, the employee first creates it via UC02.  
5. The system shows the farm’s details (name, CVR, address, farmer, area).  
6. The employee clicks **“Create Nature Check Case”**.  
7. The system opens a form asking the employee to select:  
   - a consultant to assign  
   - optional internal notes (batch, priority, comments)  
8. The employee saves the case.  
9. The system validates the input.  
10. The system creates a new **NatureCheckCase** with status **Assigned**.  
11. The system sends a notification to the consultant.  
12. The consultant can now proceed to **UC03 – Create Nature Check** and arrange the visit date with the farmer.

---

### **Extensions (Alternatives)**
- **3a. Farm not found:**  
  The employee selects “Create Farm” → redirects to UC02 → returns to UC02b afterward.

- **7a. No consultant selected:**  
  The system displays an error message and prevents saving.

- **7b. Selected user is not a consultant:**  
  The system rejects the selection and asks for a valid consultant.

- **10a. Farm already has an active case:**  
  The system prompts: “This farm already has an active Nature Check Case. Create another?”  
  - If *yes* → continue.  
  - If *no* → cancel the operation.
