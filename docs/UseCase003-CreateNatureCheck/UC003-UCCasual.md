# **Casual Use Case - UC03 Create Nature Check**

**Use case:** UC03 – Create Nature Check 
**Scope:** The prototype of the Arla Nature Connect system  
**Level:** User goal  
**Primary Actors:** Farmer, Consultant, Arla Employee  

---

### **Stakeholders and Interests**
- Consultants want to suggest dates for nature checks and create new nature checks that farmers can accept or decline.
- Farmer wants to receive and respond to nature check suggestions from consultants.

---

### **Preconditions**
- The system is running and displays the front page.
- The user is logged in as a Consultant.  

---

### **Postconditions**
- The selected role is stored in memory for the active session.  
- The correct dashboard view is displayed for that role.
- The nature check is created and associated with the selected farm and consultant.  

---

### **Main Success Scenario**
1. The user opens the application.  
2. The system displays the front page with logo, title, and three buttons: *Landmand*, *Konsulent*, *Arla Medarbejder*.  
3. The user clicks the button for their desired role.  
4. If *Konsulent* is selected:
   4a. The system loads available persons with the selected role from the database repositories.
   4b. The system filters persons by role and active status, and sorts them alphabetically.
   4c. The system displays the role dashboard with a dropdown list of available persons.
5. The user selects their profile from the dropdown and confirms.  
6. The system updates the dashboard corresponding to the selected person and role.
  
7. The consultant navigates to the "Create Nature Check" section.
8. The consultant selects a farm from a dropdown list of associated farms.
9. The consultant suggests dates for the nature check and fills in any required details.
10. The consultant submits the nature check suggestion.
11. The system creates the nature check and notifies the associated farmer for acceptance or decline.
12. The farmer receives the notification and can respond to the nature check suggestion.
13. The system updates the status of the nature check based on the farmer's response.
14. The consultant can view the status of the nature check in their dashboard.

15. If *Arla Medarbejder* is selected, the system goes directly to the Arla dashboard without requiring person selection.  

---