```mermaid
sequenceDiagram
    title UC01 – Login & Role Access

    actor User
    participant LoginPage as "LoginPage.xaml (View)"
    participant LoginPageViewModel
    participant NavigationHandler
    participant FarmerPage as "FarmerPage.xaml (View)"
    participant FarmerPageViewModel
    participant ConsultantPage as "ConsultantPage.xaml (View)"
    participant ConsultantPageViewModel
    participant ArlaEmployeePage as "ArlaEmployeePage.xaml (View)"
    participant ArlaEmployeePageViewModel
    participant ArlaEmployeeAssignNatureCheckViewModel
    participant FarmerPageSideMenuUCViewModel
    participant ConsultantPageSideMenuUCViewModel
    participant IPersonRepository

    %% Step 1: Launch
    User ->> LoginPage: LaunchApplication()
    activate LoginPage
    LoginPage -->> User: DisplayRoleButtons()
    deactivate LoginPage

    %% Step 2: Role selection
    User ->> LoginPage: ClickRoleButton(role)
    activate LoginPage
    LoginPage ->> LoginPageViewModel: Execute SelectRoleCommand(role)
    activate LoginPageViewModel
    LoginPageViewModel ->> LoginPageViewModel: SelectRole(roleName)
    LoginPageViewModel ->> NavigationHandler: Navigate(pageType, parameter=role)
    activate NavigationHandler

    %% Step 3: Navigation based on role
    alt Role = Farmer
        NavigationHandler ->> FarmerPage: LoadPage()
        activate FarmerPage
        FarmerPage ->> FarmerPageViewModel: InitializeAsync(role)
        activate FarmerPageViewModel
        FarmerPage ->> FarmerPageSideMenuUCViewModel: InitializeAsync()
        activate FarmerPageSideMenuUCViewModel
        FarmerPageSideMenuUCViewModel ->> IPersonRepository: GetPersonsByRoleAsync("Farmer")
        activate IPersonRepository
        IPersonRepository -->> FarmerPageSideMenuUCViewModel: persons
        deactivate IPersonRepository
        FarmerPageSideMenuUCViewModel -->> FarmerPage: UpdateAvailablePersons()
        FarmerPage -->> User: DisplayPersonDropdown()
        User ->> FarmerPage: SelectPerson(person)
        FarmerPage ->> FarmerPageSideMenuUCViewModel: SelectedPerson = person
        FarmerPageSideMenuUCViewModel ->> FarmerPageViewModel: ChooseUserCommand.Execute(person)
        FarmerPageViewModel ->> FarmerPage: UpdateDashboard(person)
        FarmerPage -->> User: DisplayPersonSpecificDashboard()
        deactivate FarmerPageSideMenuUCViewModel
        deactivate FarmerPageViewModel
        deactivate FarmerPage
    else Role = Consultant
        NavigationHandler ->> ConsultantPage: LoadPage()
        activate ConsultantPage
        ConsultantPage ->> ConsultantPageViewModel: InitializeAsync(role)
        activate ConsultantPageViewModel
        ConsultantPage ->> ConsultantPageSideMenuUCViewModel: InitializeAsync()
        activate ConsultantPageSideMenuUCViewModel
        ConsultantPageSideMenuUCViewModel ->> IPersonRepository: GetPersonsByRoleAsync("Consultant")
        activate IPersonRepository
        IPersonRepository -->> ConsultantPageSideMenuUCViewModel: persons
        deactivate IPersonRepository
        ConsultantPageSideMenuUCViewModel -->> ConsultantPage: UpdateAvailablePersons()
        ConsultantPage -->> User: DisplayPersonDropdown()
        User ->> ConsultantPage: SelectPerson(person)
        ConsultantPage ->> ConsultantPageSideMenuUCViewModel: SelectedPerson = person
        ConsultantPageSideMenuUCViewModel ->> ConsultantPageViewModel: ChooseUserCommand.Execute(person)
        ConsultantPageViewModel ->> ConsultantPage: UpdateDashboard(person)
        ConsultantPage -->> User: DisplayPersonSpecificDashboard()
        deactivate ConsultantPageSideMenuUCViewModel
        deactivate ConsultantPageViewModel
        deactivate ConsultantPage
    else Role = Arla Employee
        NavigationHandler ->> ArlaEmployeePage: LoadPage()
        activate ArlaEmployeePage
        ArlaEmployeePage ->> ArlaEmployeePageViewModel: InitializeAsync(role)
        activate ArlaEmployeePageViewModel
        ArlaEmployeePageViewModel ->> ArlaEmployeeAssignNatureCheckViewModel: InitializeAsync()
        activate ArlaEmployeeAssignNatureCheckViewModel
        ArlaEmployeeAssignNatureCheckViewModel -->> ArlaEmployeePageViewModel: context loaded
        deactivate ArlaEmployeeAssignNatureCheckViewModel
        ArlaEmployeePage -->> User: DisplayEmployeeDashboard()
        deactivate ArlaEmployeePageViewModel
        deactivate ArlaEmployeePage
    end

    deactivate NavigationHandler
    deactivate LoginPageViewModel
    deactivate LoginPage
```
