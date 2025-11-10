# OC - Operations Contracts

## Table of Contents
- [Operation Contracts - UC010a: SelectRole](#operation-contracts-for-uc010a--dashboard--data-overview)
- [Operation Contract – UC010b: ChooseUser](#operation-contract--uc010b-chooseuseruser)
- [Operation Contract – UC010c: ViewDataOverview](#operation-contract--uc010c-viewdataoverview)
- [Operation Contract – UC010d: GenerateReports](#operation-contract--uc010d-generatereportsreporttype)

# **Operation Contracts for UC010a – SelectRole**
| Item | Description |
|-----------|-----------------|
| **Operation** | `SelectRole(role)` |
| **Cross References** | UC010 – Login & Role Access |
| **Preconditions** | The system is running and displays the start page. |
| **Input** | The user selects a role (Farmer, Consultant, or Arla Employee). |
| **Output** | The system either:<br>• Navigates directly to the dashboard (if Arla Employee)<br>• Displays a list of available users (if Farmer or Consultant) |
| **Postconditions** | The selected role is temporarily stored in memory for the current session. |

# **Operation Contract – UC010b: ChooseUser(user)**
| Item | Description |
|-----------|-----------------|
| **Operation** | `ChooseUser(user)` |
| **Cross References** | UC010 – Login & Role Access |
| **Preconditions** | A role that requires user selection (Farmer or Consultant) has been chosen. |
| **Input** | The user selects a specific user profile from the dropdown list. |
| **Output** | The system loads and displays the dashboard and data for the selected user. |
| **Postconditions** | The system associates the current session with the chosen user and updates the displayed dashboard accordingly. |

# **Operation Contract – UC010c: ViewDataOverview()**
| Item | Description |
|-----------|-----------------|
| **Operation** | `ViewDataOverview()` |
| **Cross References** | UC010 – Login & Role Access |
| **Preconditions** | The user is logged in and has access to the dashboard. |
| **Input** | No input parameters. |
| **Output** | The system retrieves and displays an overview of data and analytics relevant to the user's role and selected profile. |
| **Postconditions** | The user can view the data overview and analytics on the dashboard. |

# **Operation Contract – UC010d: GenerateReports(reportType)**
| Item | Description |
|-----------|-----------------|
| **Operation** | `GenerateReports(reportType)` |
| **Cross References** | UC010 – Login & Role Access |
| **Preconditions** | The user is logged in and has access to the dashboard. |
| **Input** | The user selects the type of report to generate (if applicable). |
| **Output** | The system generates and provides the requested report based on the selected type. |
| **Postconditions** | The user receives the generated report for review or download. |
