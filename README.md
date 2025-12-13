# ![Logo][Logo] Arla Nature Connect
Welcome to the Arla Nature Connect project! This repository contains resources and information for a prototype application designed for communication and collaboration on sustainability initiatives within the Arla Nature Connect community.

## 📚 Table of Contents

- [Overview](#overview)
- [For developers](#for-developers)
  - [Getting Started](#getting-started)
  - [Project Structure](#project-structure)
  - [Debugging](#debugging)
- [Wiki Documentation](#wiki-documentation)
- [Boilerplate](#boilerplate)
  - [ViewModelBase](#viewmodelbase)
  - [ListViewModelBase](#listviewmodelbase)
  - [CRUDViewModelBase](#crudviewmodelbase)
- [Services Used](#services-used)
  - [AppMessageService](#appmessageservice)
  - [ConnectionStringService](#connectionstringservice)
  - [StatusInfoService](#statusinfoservice)
- [Documentation](#documentation)
- [Use Cases](#use-cases)

## 📖 Overview
Arla Nature Connect is a platform that enables members of the Arla Nature Connect community to connect, share ideas, and collaborate on sustainability projects.
The application provides features such as discussion forums, resource sharing, event management, and project tracking to support the community's efforts in promoting sustainable practices.

## 👩‍💻 For developers
Guided instructions and information for developers interested in contributing to the Arla Nature Connect project.

### 🚀 Getting Started
To get started with the Arla Nature Connect project, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/DMOoF25-TEAM11/ArlaNatureConnect.git
   cd ArlaNatureConnect
   ```

2. Install the required dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run --project src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI (Package).wapproj
   ```

### 🗂️ Project Structure
```plaintext
Arla Nature Connect/
├── 📂 src/                                      # Source code for the project
│   ├── 📦 ArlaNatureConnect.Domain/             # Domain models
│   ├── 📦 ArlaNatureConnect.Core/               # Business logic
│   ├── 📦 ArlaNatureConnect.Infrastructure/     # Data access and external services
│   └── 📦 ArlaNatureConnect.WinUI/              # Windows application
├── 📂 tests/                                    # Unit and integration tests
│   └── 📦 ArlaNatureConnect/                    # Test projects
│       ├── 📦 TestCore/                         # Tests for core functionality
│       ├── 📦 TestDomain/                       # Tests for domain models
│       ├── 📦 TestInfrastructure/               # Tests for infrastructure
│       └── 📦 TestWinUI/                        # Tests for WinUI
├── 🖼️ images/                                   # Image assets for the project
├── 📄 docs/                                     # Documentation files
├── 📄 README.md                                 # Project documentation
└── 📄 .gitignore                                # Git ignore file
```

### 🐞 Debugging
When running the application in debug mode, the application will send debug meassages starting wih *** Namespace.Method : Message

## Wiki Documentation
In-depth documentation for various components of the Arla Nature Connect project can be found in the [docs/wiki](docs/wiki) directory.

### 📘 Boilerplate
Common base classes and patterns used throughout the application.  
Click the links below for detailed documentation.

| Abstract Classes | Description |
|------------------|-------------|
| [ViewModelBase]  | Base class for all ViewModels in the application. |
| [ListViewModelBase] | Base class for ViewModels that represent a list of items. |
| [CRUDViewModelBase] | Base class for ViewModels that implement CRUD operations. |

### ⚙️ Services Used
How to use common services in the application.  
Click the links below for detailed documentation.

| Services               | Description                                      |
|------------------------|--------------------------------------------------|
| [AppMessageService]    | Service for displaying application messages to the user. |
| [ConnectionStringService] | Service for managing database connection strings. |
| [StatusInfoService]    | Service for displaying status information in the application. |

## Documentation
Additional documentation and resources for the Arla Nature Connect project can be found in the subfolder docs

### Use Cases

| Use Case | Documentation |
|----------|-------------|
| UC-001   | [Domain model][UC-001-DM]<br/>[DCD][UC-001-DCD]<br/>[User Story][UC-001-S]<br/>[SSD][UC-001-SSD]<br/>[OC][UC-001-OC]<br/>[SD][UC-001-SD]<br/>[UCBrief][UC-001-UCB]<br/>[UCCasual][UC-001-UCC] |
| UC-002   | [Artifacts][UC-002-Artifacts] |
| UC-002B  |
| UC-003   |
| UC-004   |
| UC-010	 |


<!-- MARKDOWN LINKS & IMAGES -->
[Logo]: images/logo/logo32x32.png "Arla Nature Connect Logo"

[AppMessageService]: docs/wiki/Core/Services/AppMessageService.md "AppMessageService Documentation"
[ConnectionStringService]: docs/wiki/Core/Services/ConnectionStringService.md "ConnectionStringService Documentation"
[StatusInfoService]: docs/wiki/Core/Services/StatusInfoService.md "StatusInfoService Documentation"

[ViewModelBase]: docs/wiki/Winui/ViewModels/Abstract/ViewModelBase.md "ViewModelBase Documentation"
[ListViewModelBase]: docs/wiki/Winui/ViewModels/Abstract/ListViewModelBase.md "ListViewModelBase Documentation"
[CRUDViewModelBase]: docs/wiki/Winui/ViewModels/Abstract/CRUDViewModelBase.md "CRUDViewModelBase Documentation"

[UC-001-DM]:    docs/UseCase001-LoginAndRoleaccess/UC001-DomainModel.md
[UC-001-DCD]:   docs/UseCase001-LoginAndRoleaccess/UC001-DCD.md
[UC-001-S]: 	  docs/UseCase001-LoginAndRoleaccess/UC001-UserStory.md
[UC-001-SSD]:   docs/UseCase001-LoginAndRoleaccess/UC001-SSD.md
[UC-001-OC]:    docs/UseCase001-LoginAndRoleaccess/UC001-OC.md
[UC-001-SD]:    docs/UseCase001-LoginAndRoleaccess/UC001-SD.md
[UC-001-UCB]:   docs/UseCase001-LoginAndRoleaccess/UC001-UCBrief.md
[UC-001-UCC]:   docs/UseCase001-LoginAndRoleaccess/UC001-UCCasual.md
[UC-001-ERD]:   docs/UseCase001-LoginAndRoleaccess/UC001-ER.md

[UC-002-Artifacts]:    docs/UseCase002-AdministrateFarmsAndUsers/UC002-Artifacts.md
