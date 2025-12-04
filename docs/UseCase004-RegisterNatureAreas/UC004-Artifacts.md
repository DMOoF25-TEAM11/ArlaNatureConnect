# Use Case 004 - Register Nature Areas
This document outlines the artifacts associated with Use Case 004 - Register Nature Areas. The purpose of this use case is to facilitate the registration and management of nature areas within the system.

## User Story
As an Employee,
I want to register new nature areas
so that they can be managed and preserved effectively.

## User Case Brief
Employees can register new nature areas by providing necessary details such as area name, coordinates, size, and type of the area.
The system will store this information and allow for future management and updates.

-ID: UC-004-B
-Primary Actor: Employee
-Stakeholders:
	- Employee: Wants to register and manage nature areas.
	- Farmer: Interested in the preservation of nature areas.
	- Consultant: Provides expertise on nature area management.
	- System Administrator: Ensures the system functions correctly.
- Scope: WinUI application
- Level: User goal
- Cross references:
	- See the User Story above: [User Story](#user-story)
  - See the Domain Model below: [UC-004-DM](#domain-model)
 
### Preconditions
- The employee is logged into the system.
- Is on the "Register Nature Area" page.

### Postconditions
- A new nature area is registered in the system with the provided details.
- The nature area can be viewed and managed by authorized users.

### Primary success scenario (happy path)
1. Employee navigates chooses a farm to register a nature area for.
1. Employee selects the option to register a new nature area.
1. Employee fills in the required details (name, coordinates, size, type).
1. Employee submits the registration form.
1. System validates the input and saves the new nature area.
1. System confirms the successful registration of the nature area.
1. Employee receives confirmation and can view the registered nature area.

