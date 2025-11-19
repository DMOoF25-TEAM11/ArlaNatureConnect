# ![Logo][Logo] 🌿 Arla Nature Connect
Welcome to the Arla Nature Connect project! This repository contains resources and information for a prototype application designed for communication and collaboration on sustainability initiatives within the Arla Nature Connect community.

## 📚 Table of Contents

- [Overview](#overview)
- [For developers](#for-developers)
  - [Getting Started](#getting-started)
  - [Project Structure](#project-structure)
  - [Debugging](#debugging)
  - [Boilerplate](#boilerplate)
    - [ViewModelBase](#viewmodelbase)
  - [Services Used](#services-used)
    - [AppMessageService](#appmessageservice)
    - [StatusInfoService](#statusinfoservice)

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

### 📘 Boilerplate
Common base classes and patterns used throughout the application.
#### [ViewModelBase]
See detailed documentation in the linked page.

### ⚙️ Services Used
How to use common services in the application

#### [AppMessageService]
See detailed documentation in the linked page.

#### [ConnectionStringService]
See detailed documentation in the linked page.

#### [StatusInfoService]
See detailed documentation in the linked page.

<!-- MARKDOWN LINKS & IMAGES -->
[Logo]: images/logo/logo32x32.png "Arla Nature Connect Logo"

[AppMessageService]: docs/wiki/services/AppMessageService.md "AppMessageService Documentation"
[ConnectionStringService]: docs/wiki/services/ConnectionStringService.md "ConnectionStringService Documentation"
[StatusInfoService]: docs/wiki/services/StatusInfoService.md "StatusInfoService Documentation"
[ViewModelBase]: docs/wiki/abstract/ViewModelBase.md "ViewModelBase Documentation"
