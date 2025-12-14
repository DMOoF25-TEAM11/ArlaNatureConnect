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
        ArlaEmployee ->> System: Enter farm details (name, CVR, address, owner email) and save
        alt Validation succeeds
            alt Owner email exists and has Farmer role
                Note over System: System reuses existing person<br/>Creates new farm linked to existing owner
                System -->> ArlaEmployee: Display success message "Ny gård er tilføjet"
            else Owner email does not exist
                Note over System: System creates new person with Farmer role<br/>Creates new farm linked to new owner
                System -->> ArlaEmployee: Display success message "Ny gård er tilføjet"
            end
            System -->> ArlaEmployee: Return to farm list with new farm
            ArlaEmployee ->> System: Select newly created farm
            System -->> ArlaEmployee: Display farm details<br/>Empty assignment form
        else Validation fails
            alt CVR already exists
                System -->> ArlaEmployee: Display error "En gård med CVR-nummer '[CVR]' findes allerede i systemet. Vælg et andet CVR-nummer."
            else Owner email exists but does not have Farmer role
                System -->> ArlaEmployee: Display error "En person med e-mail '[Email]' findes allerede i systemet, men har ikke rollen 'Farmer'. En landmand kan kun have flere gårde hvis de har Farmer-rollen."
            else Missing required fields
                System -->> ArlaEmployee: Display error "Udfyld alle påkrævede felter"
            end
        end
    end

    alt Farm has active case
        System -->> ArlaEmployee: Display button "Opdater natur Check opgave" (Update Nature Check task)
    else Farm has no active case
        System -->> ArlaEmployee: Display button "Lav natur Check opgave" (Create Nature Check task)
    end

    ArlaEmployee ->> System: Select consultant, priority, enter notes, then click button
    alt Validation succeeds
        alt Farm has active case
            System -->> ArlaEmployee: Display success message "Natur Check opgave er opdateret for [FarmName]"
            Note over System: System updates existing NatureCheckCase with:<br/>- New consultant (if changed)<br/>- New priority (English format)<br/>- New notes<br/>- Updated assignment time<br/>System updates farm list to show "Tilføjet" status
        else Farm has no active case
            System -->> ArlaEmployee: Display success message "Natur Check opgave er oprettet for [FarmName]"
            Note over System: System creates new NatureCheckCase with status "Assigned"<br/>System updates farm list to show "Tilføjet" status<br/>System stores priority (English) and notes in database
        end
    else Validation fails
        alt No consultant selected
            System -->> ArlaEmployee: Display error "Vælg konsulent"
        else No farm selected
            System -->> ArlaEmployee: Display error "Vælg en gård"
        else Farm already has active case
            System -->> ArlaEmployee: Display error "Gården har allerede en aktiv Natur Check opgave"
        else Consultant does not have Consultant role
            System -->> ArlaEmployee: Display error "Den valgte person har ikke konsulent-rollen"
        else Database constraint violation
            alt Email already exists (different person)
                System -->> ArlaEmployee: Display error "En person med denne e-mail findes allerede i systemet. Vælg en anden e-mail."
            else CVR already exists
                System -->> ArlaEmployee: Display error "En gård med dette CVR-nummer findes allerede i systemet. Vælg et andet CVR-nummer."
            end
        end
    end

    Note over System: System sends database notification to consultant<br/>(Consultant sees notification badge in UI)
```

**Notes:**
- The system displays farms with their assignment status ("Tilføjet" / "Ikke tilføjet") and priority badges.
- When a farm with an active case is selected, the form is auto-populated with existing assignment data (consultant, priority, notes) and the button text changes to "Opdater natur Check opgave" (Update Nature Check task).
- When a farm with no active case is selected, the button text displays "Lav natur Check opgave" (Create Nature Check task).
- Priority selection is available in the assignment form (Danish values: Lav, Normal, Høj, Haster).
- The system stores priority in English format in the database ("Low", "Medium", "High", "Urgent").
- Notifications are database-based (consultant sees notification badge in UI, not email/SMS).
- The system validates all inputs before creating or updating the case and provides clear error messages.
- The system can update existing active cases (status "Assigned" or "InProgress") without creating duplicate cases.
- **Farm creation:** When creating a new farm, if the owner email already exists and the person has the Farmer role, the system reuses the existing person and links the new farm to them. This allows a farmer to own multiple farms with different CVR numbers. If the email exists but the person does not have the Farmer role, the system displays an error message.
- **CVR validation:** Each farm must have a unique CVR number. If a CVR already exists, the system displays an error message.
- **Error handling:** The system provides specific error messages for database constraint violations (duplicate email or CVR) to help users understand what went wrong.
