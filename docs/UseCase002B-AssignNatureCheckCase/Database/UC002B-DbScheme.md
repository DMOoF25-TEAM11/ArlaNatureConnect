
## Relational Database Schema – UC002B: Assign Nature Check Case
---
### **Normalization**
| Normal form | Status | Explanation |
|--------------|---------|-------------|
| **1NF** | ✅ | All attributes are atomic |
| **2NF** | ✅ | All non-key attributes depend on the whole primary key |
| **3NF** | ✅ | No transitive dependencies – each entity has its own table |

---

### **Compact Notation**
|Notation|
|---------------------------------------------|
|NATURE_CHECK_CASES(CaseID PK, FarmID FK, ConsultantID FK, AssignedByPersonID FK, Status, Notes, Priority, CreatedAt, AssignedAt)|

---

### **NATURE_CHECK_CASES**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **Id** | `uniqueidentifier` | **PK** | Unique identifier for each Nature Check Case |
| **FarmId** | `uniqueidentifier` | **FK → Farms(Id)** | Reference to the farm for which the case is created |
| **ConsultantId** | `uniqueidentifier` | **FK → Persons(Id)** | Reference to the consultant assigned to the case (must have Consultant role) |
| **AssignedByPersonId** | `uniqueidentifier` | **FK → Persons(Id)** | Reference to the Arla employee who assigned the case (must have Employee role) |
| **Status** | `nvarchar(50)` |  | Current status of the case (e.g., "Assigned", "InProgress", "Completed", "Cancelled") |
| **Notes** | `nvarchar(MAX)` *(nullable)* |  | Optional internal notes about the case (batch, priority, comments) |
| **Priority** | `nvarchar(50)` *(nullable)* |  | Optional priority level of the case (e.g., "Low", "Medium", "High", "Urgent") |
| **CreatedAt** | `datetime2` |  | Timestamp of when the case was created (UTC) |
| **AssignedAt** | `datetime2` *(nullable)* |  | Timestamp of when the case was assigned to the consultant (UTC) |

---

### **Relationships**
| Relation | Cardinality | Description |
|-----------|--------------|-------------|
| `Farms(Id)` → `NatureCheckCases(FarmId)` | **1 : ∞** | One farm can have many Nature Check Cases |
| `Persons(Id)` → `NatureCheckCases(ConsultantId)` | **1 : ∞** | One consultant can be assigned to many Nature Check Cases |
| `Persons(Id)` → `NatureCheckCases(AssignedByPersonId)` | **1 : ∞** | One Arla employee can assign many Nature Check Cases |

---

### **Constraints**
- **Primary Key**: `Id` - Unique identifier for each case
- **Foreign Keys**:
  - `FarmId` references `Farms.Id` (ON DELETE CASCADE - if farm is deleted, cases are deleted)
  - `ConsultantId` references `Persons.Id` (ON DELETE NO ACTION - cannot delete consultant with active cases)
  - `AssignedByPersonId` references `Persons.Id` (ON DELETE NO ACTION - cannot delete employee who assigned cases)
- **Indexes**:
  - `IX_NatureCheckCases_FarmId` - For efficient queries by farm
  - `IX_NatureCheckCases_ConsultantId` - For efficient queries by consultant
  - `IX_NatureCheckCases_Status` - For filtering by status
  - `IX_NatureCheckCases_CreatedAt` - For sorting by creation date

---

### **Entity Framework Core Implementation**

**Note:** In Entity Framework Core implementation:
- All data access uses EF Core repositories with LINQ queries
- Notifications are generated from NatureCheckCase data (not stored in database)
- No stored procedures or views needed

---

### **Business Rules**
1. A `ConsultantId` must reference a `Person` with the role "Consultant"
2. An `AssignedByPersonId` must reference a `Person` with the role "Employee"
3. `Status` should be one of: "Assigned", "InProgress", "Completed", "Cancelled"
4. `CreatedAt` is automatically set by EF Core when the case is created
5. `AssignedAt` is set when the case is assigned (can be the same as `CreatedAt` for immediate assignment)
6. Multiple cases can exist for the same farm (soft rule: typically only one active case per farm)
7. All operations use Entity Framework Core repositories, not stored procedures

---

