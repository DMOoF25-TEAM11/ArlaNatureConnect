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
2. The system loads available farms and consultants and displays them.  
3. The employee selects an existing farm (or creates one via UC02 if missing).  
4. The system presents the selected farm’s summary information.  
5. The employee chooses to **Create Nature Check Case**.  
6. The system shows a form where the employee selects a consultant and optional notes.  
7. The employee confirms the assignment.  
8. The system validates the farm, consultant role, and any business rules.  
9. A new Nature Check Case is created with status **Assigned** and linked to the chosen consultant and farm.  
10. The system notifies the consultant and shows the employee a success confirmation.  
11. The consultant can continue in **UC03 – Create Nature Check** to plan the visit.

---

### **Extensions (Alternatives)**
- **3a. Farm not found:**  
  The employee selects “Create Farm” → UC02 handles the creation → the employee returns to UC02b.

- **8a. No consultant selected:**  
  The system prompts the employee to choose a consultant before continuing.

- **8b. Selected user is not a consultant:**  
  The system rejects the selection and asks for a valid consultant.

- **10a. Farm does not exist:**  
  The system cannot complete the assignment and directs the employee to register the farm first (UC02).

- **10b. Consultant does not exist or is inactive:**  
  The system prevents assignment and requests another consultant.

- **10c. Farm already has an active case (and allowDuplicate is false):**  
  The system prompts: "This farm already has an active Nature Check Case. Create another?"  
  - If *yes* → continue with `allowDuplicate = true`.  
  - If *no* → cancel the operation.
