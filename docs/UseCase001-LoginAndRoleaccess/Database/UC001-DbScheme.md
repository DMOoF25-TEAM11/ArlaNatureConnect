

## Relational Database Schema – UC01: Login & Role Access
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
|ROLE(RoleID PK, Name)|
|PERSONS(PersonID PK, FirstName, LastName, Email, RoleID FK, AddressID FK, IsActive)|
|LOGIN_SESSION(SessionID PK, PersonID FK, LoginTimestamp, LogoutTimestamp)|

---

### **ROLE**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **RoleID** | `uniqueidentifier` | **PK** | Unique identifier for each role |
| Name | `nvarchar(50)` |  | Name of the role (*Farmer*, *Consultant*, *ArlaEmployee*) |

---

### **PERSONS**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **PersonID** | `uniqueidentifier` | **PK** | Unique identifier for each person |
| FirstName | `nvarchar(100)` |  | Person's first name |
| LastName | `nvarchar(100)` |  | Person's last name |
| Email | `nvarchar(256)` |  | Person's email address (unique) |
| RoleID | `uniqueidentifier` | **FK → Role(RoleID)** | Reference to the person's role |
| AddressID | `uniqueidentifier` | **FK → Address(AddressID)** | Reference to the person's address (nullable) |
| IsActive | `bit` |  | Indicates whether the person is active |

---

### **LOGIN_SESSION**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **SessionID** | `uniqueidentifier` | **PK** | Unique identifier for the login session |
| PersonID | `uniqueidentifier` | **FK → Persons(PersonID)** | Reference to the person (column name kept as UserId for backward compatibility) |
| LoginTimestamp | `datetime2` |  | Timestamp of when the person logged in |
| LogoutTimestamp | `datetime2` *(nullable)* |  | Timestamp of when the person logged out (can be null) |

---

### **Relationships**
| Relation | Cardinality | Description |
|-----------|--------------|-------------|
| `Role(RoleID)` → `Persons(RoleID)` | **1 : ∞** | One role can be assigned to many persons |
| `Persons(PersonID)` → `LoginSession(PersonID)` | **1 : ∞** | One person can have many login sessions |
| `Address(AddressID)` → `Persons(AddressID)` | **1 : ∞** | One address can be associated with many persons (nullable) |

---

