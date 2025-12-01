# **Brief Use Case UC002B – Assign Nature Check Case**

**Use Case ID:** UC002B  
**Use Case navn:** Assign Nature Check Case to Consultant  
**Scope:** System prototype (Arla Nature Connect)  
**Level:** Arla Employee goal

**Primary Actors:**
- Arla Employee (Admin / Sustainability Staff)

**Secondary Actors:**
- Consultant (receives the case via database notification)  

**Description:**  
The Arla staff member selects an existing farm or creates a new one, 
and marks it as ready for a Nature Check by creating a 
NatureCheckCase and assigning it to a consultant with optional priority level and notes. 
This enables the consultant to proceed with UC03 – Create Nature Check. 
The consultant receives a database notification (visible as a badge in UI) about the new assignment.

**Preconditions:**
- The staff member is authenticated in the system (UC01).  
- The farm exists in the system or is created via farm creation.  
- The consultant is an active user with the *Consultant* role.  
- No active NatureCheckCase is already assigned to the same farm.

**Postconditions:**
- A new NatureCheckCase is created with status **Assigned**.  
- The selected consultant is linked to the case.  
- Priority level (if selected) is stored in the database (English format: "Low", "Medium", "High", "Urgent").  
- A database notification is created for the consultant (consultant sees notification badge in UI).  
- The consultant can now begin UC03 – Create Nature Check.



**Backlog relations:**
- B01–B06 (farm & user association)  
- B07 (Mark farm for Nature Check & assign consultant)
- C01 (Create Nature Check – dependency)  
- E01–E02 (Notifications - database-based, not email/SMS)

**Technology Notes:**
- Priority values: UI displays Danish ("Lav", "Normal", "Høj", "Haster"), database stores English ("Low", "Medium", "High", "Urgent").
- Notifications: Database-based (consultant sees badge in UI), not email/SMS.
- Data aggregation: Service aggregates Farm + Person + Address + Case data into DTOs for efficient UI display.
