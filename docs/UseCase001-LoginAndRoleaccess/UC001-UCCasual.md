# **Casual Use Case - UC01 Login & Role Access**

**Use case:** UC01 – Login & Role Access  
**Scope:** The prototype of the Arla Nature Connect system  
**Level:** User goal  
**Primary Actors:** Farmer, Consultant, Arla Employee  

---

### **Stakeholders and Interests**
- Farmers want to access their farm dashboard and see sustainability data.  
- Consultants want to view and manage farm nature checks.  
- Arla employees want to monitor and manage system-wide activities.  

---

### **Preconditions**
- The system is running and displays the front page.  

---

### **Postconditions**
- The selected role is stored in memory for the active session.  
- The correct dashboard view is displayed for that role.  

---

### **Main Success Scenario**
1. The user opens the application.  
2. The system displays the front page with logo, title, and three buttons: *Landmand*, *Konsulent*, *Arla Medarbejder*.  
3. The user clicks the button for their desired role.  
4. If *Landmand* or *Konsulent* is selected:
   4a. The system loads available persons with the selected role from the database repositories.
   4b. The system filters persons by role and active status, and sorts them alphabetically.
   4c. The system displays the role dashboard with a dropdown list of available persons.
5. The user selects their profile from the dropdown and confirms.  
6. The system updates the dashboard corresponding to the selected person and role.  
7. If *Arla Medarbejder* is selected, the system goes directly to the Arla dashboard without requiring person selection.  

---

### **Extensions (Alternatives)**

