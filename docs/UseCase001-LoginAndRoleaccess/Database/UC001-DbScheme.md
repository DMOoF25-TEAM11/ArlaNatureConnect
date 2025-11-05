
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
|USER(UserID PK, FirstName, LastName, RoleID FK, IsActive)|
|LOGIN_SESSION(SessionID PK, UserID FK, LoginTimestamp, LogoutTimestamp)|

---

### **ROLE**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **RoleID** | `uniqueidentifier` | **PK** | Unique identifier for each role |
| Name | `nvarchar(50)` |  | Name of the role (*Farmer*, *Consultant*, *ArlaEmployee*) |

---

### **USER**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **UserID** | `uniqueidentifier` | **PK** | Unique identifier for each user |
| FirstName | `nvarchar(100)` |  | User’s first name |
| LastName | `nvarchar(100)` |  | User’s last name |
| RoleID | `uniqueidentifier` | **FK → Role(RoleID)** | Reference to the user’s role |
| IsActive | `bit` |  | Indicates whether the user is active |

---

### **LOGIN_SESSION**
| Attribute | Data type | Key | Description |
|------------|------------|-----|-------------|
| **SessionID** | `uniqueidentifier` | **PK** | Unique identifier for the login session |
| UserID | `uniqueidentifier` | **FK → User(UserID)** | Reference to the user |
| LoginTimestamp | `datetime2` |  | Timestamp of when the user logged in |
| LogoutTimestamp | `datetime2` *(nullable)* |  | Timestamp of when the user logged out (can be null) |

---

### **Relationships**
| Relation | Cardinality | Description |
|-----------|--------------|-------------|
| `Role(RoleID)` → `User(RoleID)` | **1 : ∞** | One role can be assigned to many users |
| `User(UserID)` → `LoginSession(UserID)` | **1 : ∞** | One user can have many login sessions |

---

