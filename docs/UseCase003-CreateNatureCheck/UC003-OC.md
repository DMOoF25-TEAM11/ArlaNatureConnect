# **Operation Contract – UC003a: SelectRole(role)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `SelectRole(role)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | The system is running and displays the start page. |
| **Input** | The user selects a role (Consultant, or Arla Employee). |
| **Output** | The system either:<br>• Navigates directly to the dashboard (if Arla Employee)<br>• Displays a list of available users (Consultant) |
| **Postconditions** | The selected role is temporarily stored in memory for the current session. |

# **Operation Contract – UC003b: ChooseUser(user)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `ChooseUser(user)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | A role that requires user selection (Consultant) has been chosen. |
| **Input** | The user selects a specific user profile from the dropdown list. |
| **Output** | The system loads and displays the dashboard and data for the selected user. |
| **Postconditions** | The system associates the current session with the chosen person and updates the displayed dashboard accordingly. |

# **Operation Contract – UC003c: LoadAvailableUsers(role)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `LoadAvailableUsers(role)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | A role that requires person selection (Consultant) has been chosen. |
| **Input** | The system receives the selected role. |
| **Output** | The system loads and displays a list of available persons matching the role from the database. |
| **Postconditions** | AvailablePersons list is populated with active persons matching the role, sorted alphabetically by first name and last name. |

# **Operation Contract – UC003c: NavigateToNatureCheck(role)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `NavigateToNatureCheck(role)` |
| **Cross References** | UC03 – Create Nature Check |
| **Preconditions** | The user is logged in as a Consultant or ArlaMedarbejder and is on the dashboard. |
| **Input** | The user selects the "Create Nature Check" option from the dashboard. |
| **Output** | The system navigates to the "Create Nature Check" interface, displaying relevant fields for farm selection, date suggestion, and details input. |
| **Postconditions** | The user is presented with the "Create Nature Check" form, ready to input data for a new nature check. |

# **Operation Contract – UC003d: SubmitNatureCheck(farm, date, details)**

| Item | Description |
|-----------|-----------------|
| **Operation** | `SubmitNatureCheck(farm, date, details)` |
| **Cross References** | UC03 – Create Nature Check |
| **Preconditions** | The user has filled out the "Create Nature Check" form with valid data. |
| **Input** | The user submits the nature check with selected farm, suggested date, and additional details. |
| **Output** | The system creates a new nature check entry in the database and notifies the associated farmer. |
| **Postconditions** | A new nature check is created and associated with the selected farm and consultant. The farmer is notified for acceptance or decline. |
