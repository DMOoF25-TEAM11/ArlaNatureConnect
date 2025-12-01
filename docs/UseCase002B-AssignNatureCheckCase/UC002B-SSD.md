# **UC002B-SSD – Assign Nature Check Case to Consultant**

System Sequence Diagram showing the interaction between the Arla Employee actor and the System, following Larmann's UML conventions.

```mermaid
sequenceDiagram
    title UC002B – SSD: Assign Nature Check Case to Consultant

    actor ArlaEmployee as Arla Employee
    participant System as System

    ArlaEmployee ->> System: Open "Nature Check Cases" module
    System -->> ArlaEmployee: Display list of farms with status (Tilføjet/Ikke tilføjet) and priority badges

    alt Farm exists in list
        ArlaEmployee ->> System: Select existing farm
        alt Farm has active case
            Note over System: Auto-populate form with existing assignment data
            System -->> ArlaEmployee: Display farm details<br/>Form pre-filled with:<br/>- Assigned consultant<br/>- Priority (Danish)<br/>- Notes
        else Farm has no active case
            System -->> ArlaEmployee: Display farm details<br/>Empty assignment form
        end
    else Farm does not exist
        ArlaEmployee ->> System: Click "Tilføj ny gård" (Create New Farm)
        System -->> ArlaEmployee: Display farm creation form
        ArlaEmployee ->> System: Enter farm details (name, CVR, address, owner) and save
        System -->> ArlaEmployee: Display confirmation and return to farm list
        ArlaEmployee ->> System: Select newly created farm
        System -->> ArlaEmployee: Display farm details<br/>Empty assignment form
    end

    ArlaEmployee ->> System: Click "Lav natur check opgave" (Create Nature Check Case)
    System -->> ArlaEmployee: Display assignment form with:<br/>- Consultant dropdown<br/>- Priority dropdown (Lav, Normal, Høj, Haster)<br/>- Notes text field

    ArlaEmployee ->> System: Select consultant, priority, enter notes, then submit
    alt Validation succeeds
        System -->> ArlaEmployee: Display success message "Opgave tildelt succesfuldt"
        Note over System: System creates NatureCheckCase with status "Assigned"<br/>System updates farm list to show "Tilføjet" status<br/>System stores priority (English) and notes in database
    else Validation fails
        alt No consultant selected
            System -->> ArlaEmployee: Display error "Vælg konsulent"
        else No farm selected
            System -->> ArlaEmployee: Display error "Vælg en gård"
        else Farm already has active case
            System -->> ArlaEmployee: Display error "Gården har allerede en aktiv Natur Check opgave"
        else Consultant does not have Consultant role
            System -->> ArlaEmployee: Display error "Den valgte person har ikke konsulent-rollen"
        end
    end

    Note over System: System sends database notification to consultant<br/>(Consultant sees notification badge in UI)
```

**Notes:**
- The system displays farms with their assignment status ("Tilføjet" / "Ikke tilføjet") and priority badges.
- When a farm with an active case is selected, the form is auto-populated with existing assignment data (consultant, priority, notes).
- Priority selection is available in the assignment form (Danish values: Lav, Normal, Høj, Haster).
- The system stores priority in English format in the database ("Low", "Medium", "High", "Urgent").
- Notifications are database-based (consultant sees notification badge in UI, not email/SMS).
- The system validates all inputs before creating the case and provides clear error messages.
