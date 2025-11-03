# OC - Operations Contracts

## Table of Contents

- [OC000-Navigate To Manage Users](#oc000-navigate-to-manage-users)
- [OC001-AddUser](#oc001-adduser)
- [OC002-Select User](#oc002-select-user)
- [OC003-Edit User](#oc003-edit-user)
- [OC004-Delete User](#oc004-delete-user)
- [OC005-Navigate To Home](#oc005-navigate-to-home)


## OC000-Navigate To Manage Users

**Operation**: NavigateToManageUsers()

**Cross References**: UC002-SSD.mmd (Admin->>System: Open Users management view), UC002-UCBrief.md (steps1–2)

**Preconditions**:
- Administrator is logged in and has `Administrator` privileges.
- Main navigation is available.

**Postconditions**:
- The user administration view is active.
- A list of existing users is loaded and displayed.
- The UI is ready for the following alternative actions: Create / Select / Edit / Delete.

## OC001-AddUser

**Operation**: AddNewUser(NewUserDto user, CredentialsDto? credentials)

**Cross References**: UC002-SSD.mmd (Admin->>System: AddUser(User)), UC002-UCBrief.md (Create flow)

**Preconditions**:
- Administrator is logged in and authorized.
- The UI is in "New User" state (form visible).
- Required fields in `NewUserDto` are filled (e.g., FirstName, LastName, Email, Role).
- If Role == Farmer and farm data is required, `FarmDto` is provided.

**Postconditions**:
- A new `UserAggregate` is persisted to the data store.
- Credentials (if provided) are hashed and stored securely.
- The user list is updated in the UI.
- An audit log entry is created with actor and timestamp.

**Validation**:
- Email format and unique constraints (Email/Username) are enforced.
- Password policy is validated for provided credentials.

**Error handling**:
- `ValidationException` for invalid input.
- `ConflictException` for duplicate Email/Username/CVR.
- `UnauthorizedAccessException` if lacking permissions.


## OC002-Select User

**Operation**: SelectUser(Guid userId)

**Cross References**: UC002-SSD.mmd (Admin->>System: GetUserDetails), UC002-UCBrief.md (Read flow)

**Preconditions**:
- The user list is loaded in the UI.
- `userId` exists in the data store and is not archived (or otherwise available for viewing).
- No unsaved local changes block selection (or the user accepts discarding them).

**Postconditions**:
- Details for the selected `UserAggregate` are returned to the UI and displayed in a form/panel.
- The UI enters view or edit mode depending on context.

**Error handling**:
- `NotFoundException` if the userId does not exist.
- `UnauthorizedAccessException` if the call is not authorized.

## OC003-Edit User

**Operation**: SaveUser(Guid userId, UserUpdateDto changes, int? expectedVersion = null)

**Cross References**: UC002-SSD.mmd (Admin->>System: UpdateUser), UC002-UCBrief.md (Update flow)

**Preconditions**:
- An existing user is selected for editing (`userId`).
- Input fields in `UserUpdateDto` meet validation rules.
- If email or username changes, they remain unique or are otherwise permitted.

**Postconditions**:
- Changes are persisted to the data store (optimistic concurrency applied when `expectedVersion` is provided).
- The user list and any displayed details are refreshed.
- An audit log entry is created with change details, actor and timestamp.

**Validation**:
- Email format, field lengths and business rules are validated on the service layer.

**Error handling**:
- `ValidationException` for invalid input.
- `ConflictException` for version conflicts or unique constraint violations.
- `NotFoundException` if the userId was removed between read and write.


## OC004-Delete User

**Operation**: DeleteUser(Guid userId, bool force = false)

**Cross References**: UC002-SSD.mmd (Admin->>System: DeleteUser), UC002-UCBrief.md (Delete flow)

**Preconditions**:
- `userId` exists.
- Administrator is authorized to delete users.
- The UI has confirmed the action (confirmation dialog completed with "OK").
- System permissions for deletion or soft-delete are satisfied (e.g., no critical dependency without force).

**Postconditions**:
- User is marked inactive or removed (depending on policy).
- The user list is updated in the UI.
- An audit log entry is created with actor and reason.

**Error handling**:
- `ConflictException` if the user cannot be deleted due to dependencies.
- `NotFoundException` if the `userId` does not exist.
- `UnauthorizedAccessException` if lacking permissions.


## OC005-Navigate To Home

**Operation**: NavigateToHome()

**Cross References**: UC002-SSD.mmd (recent UX flow or return navigation), UC002-UCBrief.md (closing flow)

**Preconditions**:
- The user administration view is active.
- No unresolved critical changes exist (or the user has accepted discarding them).

**Postconditions**:
- The home view is displayed.
- Temporary edit state is ended and any temporary data is discarded.


---

### Notes (WinUI in-process)
- The operations are intended to be exposed via an in-process service interface (`IUserService`) and invoked via DI in view models.
- The service layer must enforce authorization, validation, auditing and exception handling — the UI must not assume these are performed.
- Errors are communicated through well-defined exceptions that are caught in view models and presented to the user in a friendly manner.
- Use asynchronous methods (`Task`/`async`) and `CancellationToken` to keep the UI responsive.

