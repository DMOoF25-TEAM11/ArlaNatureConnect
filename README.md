# ![Logo][Logo] Arla Nature Connect
Welcome to the Arla Nature Connect project! This repository contains resources and information for a prototype application designed for communication and collaboration on sustainability initiatives within the Arla Nature Connect community.

## Table of Contents

## 📖 Overview

## 🚀 Getting Started
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

## 📦 Project Structure
```plaintext
ArlaNatureConnect/
├── 📂 src/                                      # Source code for the project
│   ├── 📦 ArlaNatureConnect.Domain/             # Domain models
│   ├── 📦 ArlaNatureConnect.Core/               # Business logic
│   ├── 📦 ArlaNatureConnect.Infrastructure/     # Data access and external services
│   └── 📦 ArlaNatureConnect.WinUI/              # Windows application
├── 📂 tests/                                    # Unit and integration tests
├── 🖼️ images/                                   # Image assets for the project
├── 📄 docs/                                     # Documentation files
├── 📄 README.md                                 # Project documentation
└── .gitignore                              # Git ignore file
```

## 🐞 Debugging

When running the application in debug mode, the application will send debug meassages starting wih *** Namespace.Method : Message

<!-- MARKDOWN LINKS & IMAGES -->
[logo]: https://raw.githubusercontent.com//DMOoF25-TEAM11/ArlaNatureConnect/main/images/logo/logo32x32.png
