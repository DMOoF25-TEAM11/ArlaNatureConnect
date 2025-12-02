# UC002 — Administrate Farms and Users (Users CRUD)

- ID: UC002B
- Primary actor: Administrator
- Stakeholders: System administrators, farm managers
- Scope: Web / WinUI application
- Level: User goal

## Brief
Administrator can create, view, update and delete user accounts. This use case assumes the administrator is already authenticated and has appropriate permissions.

### Preconditions
- The Administrator is authenticated and authorized (already logged in).
- Administrator is on the administration screen (Users management section).

## Postconditions
- Changes to user records are persisted to the data store.
- Audit trail / logs record administration actions (if enabled).

## Primary success scenario (happy path)
1. Administrator opens the Users management view.
2. System displays a list of existing users.

At this point the Administrator chooses one of the following actions (each is an alternative option):

### Option: Create
3. Administrator selects "New User".
4. Administrator chooses role for the new user (e.g., Farmer, Consultant, Admin).
5. If role requires additional data (farmName, location, cvr), system prompts for that information.
6. Administrator enters required fields (firstName, lastName, email and address) and saves.
7. System validates input, creates the user, and updates the list.

### Option: Read
8. Administrator selects a user from the list.
9. System displays user details.

### Option: Update
10. Administrator selects a user and chooses "Edit".
11. Administrator modifies fields (role, email, enabled/disabled) and saves.
12. System validates changes, persists them, and refreshes the list.

### Option: Delete
13. Administrator selects a user and chooses "Delete".
14. System asks for confirmation.
15. Administrator confirms deletion.
16. System removes user (or marks as deleted/disabled) and updates the list.

## Security & Permissions
- Only users with the Administrator role may access this use case.
- Actions must be audited and validated server-side.
- Sensitive fields (password) must be handled securely (hashed, not returned in clear text).

## Non-functional requirements
- Response time for CRUD operations: <500 ms under normal load.
- Concurrency: Changes must be applied safely; optimistic concurrency recommended.

## Cross references
- User story: "Administrate Farms and Users" (see project backlog / UC002 user story in repository).
- Domain model: `UC002-DomainModel.mmd` (see `docs/UseCase002-AdministrateFarmsAndUsers/UC002-DomainModel.mmd`).

## Notes
- Consider soft-delete strategy for users to preserve historical data.
- Consider email verification or invitation workflow when creating new users.
