# **Operation Contract – UC01a: SelectRole(role)**

| **Item** | **Description** |
|-----------|-----------------|
| **Operation** | `SelectRole(role)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | The system is running and displays the start page. |
| **Input** | The user selects a role (Landmand, Konsulent, or Arla Medarbejder). |
| **Output** | The system either:<br>• Navigates directly to the dashboard (if Arla Medarbejder)<br>• Displays a list of available users (if Landmand or Konsulent) |
| **Postconditions** | The selected role is temporarily stored in memory for the current session. |

# **Operation Contract – UC01b: ChooseUser(user)**

| **Item** | **Description** |
|-----------|-----------------|
| **Operation** | `ChooseUser(user)` |
| **Cross References** | UC01 – Login & Role Access |
| **Preconditions** | A role that requires user selection (Landmand or Konsulent) has been chosen. |
| **Input** | The user selects a specific user profile from the dropdown list. |
| **Output** | The system loads and displays the dashboard and data for the selected user. |
| **Postconditions** | The system associates the current session with the chosen user and updates the displayed dashboard accordingly. |
