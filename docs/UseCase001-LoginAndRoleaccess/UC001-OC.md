# **Operation Contract – UC01a: SelectRole(roleName)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `SelectRole(roleName: string)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | The system is running and displays the start page. |
| **Input** | The user selects a role name as string (e.g., "Farmer", "Consultant", "ArlaEmployee"). |
| **Output** | The system creates a Role object and navigates to the appropriate page:<br>• FarmerPage (if Farmer)<br>• ConsultantPage (if Consultant)<br>• ArlaEmployeePage (if ArlaEmployee)<br>• AdministratorPage (if Administrator) |
| **Postconditions** | The selected role is stored in LoginPageViewModel.SelectedRole and the appropriate page is loaded. |

# **Operation Contract – UC01b: LoadAvailablePersons(role)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `LoadAvailablePersonsAsync(role: string)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | A role that requires person selection (Farmer or Consultant) has been chosen and the SideMenu ViewModel is initialized. |
| **Input** | The system receives the role name (e.g., "Farmer" or "Consultant"). |
| **Output** | The system loads and displays a list of available persons matching the role from the database. |
| **Postconditions** | AvailablePersons collection in SideMenuViewModelBase is populated with active persons matching the role. The list is filtered by role and sorted. |

# **Operation Contract – UC01c: ChooseUser(person)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `ChooseUser(person: Person)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | A role that requires user selection (Farmer or Consultant) has been chosen and AvailablePersons has been loaded. |
| **Input** | The user selects a specific person from the dropdown list in the side menu. |
| **Output** | The system updates SelectedPerson in the SideMenu ViewModel and notifies the host Page ViewModel via ChooseUserCommand. |
| **Postconditions** | The system associates the current session with the chosen person. SelectedPerson is set in the SideMenu ViewModel and the host Page ViewModel is notified. |
