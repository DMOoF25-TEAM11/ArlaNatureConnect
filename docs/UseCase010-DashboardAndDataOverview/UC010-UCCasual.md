# **Casual Use Case - UC010 Dashboard & Data Overview**

**Use case:** UC010 – Dashboard & Data Overview  
**Scope:** The prototype of the Arla Nature Connect system  
**Level:** User goal  
**Primary Actors:** Farmer, Consultant, Arla Employee  

---

### **Stakeholders and Interests**
- Farmers want to access their farm dashboard and see sustainability data.  
- Consultants want to view and manage farm nature checks.  
- Arla employees want to monitor and manage system-wide activities.  
- The user wants a simple path through a complex system to view relevant KPIs, graphs, and drill-down data about Nature Checks and assignments.  

---

### **Preconditions**
- The system is running and displays the front page. 
- The user has selected their role (Landmand, Konsulent, or Arla Medarbejder).  
- The user is on the appropriate dashboard page based on their role.   

---

### **Postconditions**
- The selected role is stored in memory for the active session.  
- The correct dashboard view is displayed for that role.  
- The user can view relevant KPIs, graphs, and drill-down data based on their role.
- The user can navigate to assignments related to nature checks (for farmers and consultants).

---

### **Main Success Scenario**
1. The user opens the application.  
2. The system displays the front page with logo, title, and three buttons: *Landmand*, *Konsulent*, *Arla Medarbejder*.  
3. The user clicks the button for their desired role.  
4. If *Landmand* or *Konsulent* is selected, the system goes to the role dashboard, where they can choose user profile from the dropdown.  
5. The user selects their profile and confirms.  
6. The system updates the dashboard corresponding to the selected user and role.  
7. If *Arla Medarbejder* is selected, the system goes directly to the Arla dashboard. 
8. The user views the dashboard with relevant KPIs, graphs, and drill-down options.  
9. The user navigates to assignments related to nature checks (if applicable).  

---

### **Extensions (Alternatives)**
1. At step 4, if the user selects an invalid profile, the system displays an error message and prompts for a valid selection.  
2. At step 8, if the dashboard fails to load data, the system shows a loading error and suggests retrying or contacting support.  
