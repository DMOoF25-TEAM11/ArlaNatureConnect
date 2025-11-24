# **Brief Use Case UC02b – Assign Nature Check Case**

**Use Case ID:** UC02B  
**Use Case navn:** Assign Nature Check Case to Consultant  
**Scope:** System prototype (Arla Nature Connect)  
**Level:** Arla Employee goal

**Primary Actors:**
- Arla Employee (Admin / Sustainability Staff)

**Secondary Actors:**
- Consultant (receives the case)  

**Kort beskrivelse:**  
The Arla staff member selects an existing farm or creates a new one (UC02), 
and marks it as ready for a Nature Check by creating a 
NatureCheckCase and assigning it to a consultant. This enables the consultant to 
proceed with UC03 – Create Nature Check.

**Preconditions:**
- The staff member is authenticated in the system (UC01).  
- The farm exists in the system or is created via UC02.  
- The consultant is an active user with the *Consultant* role.  
- No active NatureCheckCase is already assigned to the same farm.

**Postconditions:**
- A new NatureCheckCase is created with status **Assigned**.  
- The selected consultant is linked to the case.  
- A notification is sent to the consultant (E01–E02).  
- The consultant can now begin UC03 – Create Nature Check.

Backlog relations:
- B01–B06 (farm & user association)  
- B07 (Mark farm for Nature Check & assign consultant)
- C01 (Create Nature Check – dependency)  
- E01–E02 (Notifications, messaging)