# UC002 — Administrate Farms and Users

## Title
Administrate farms and users

## Summary
As a system administrator, I want to manage farm records and user accounts so that farms and user access can be created, viewed, updated, and deactivated in a controlled way.

## Primary actor
- System Administrator

## Secondary actors
- Farm Manager (may own or be linked to farm records)
- System (automated email service, audit log)

## Preconditions
- Administrator is authenticated and authorized with the appropriate role.
- System has a valid connection to the user directory and farm data store.

## Postconditions
- Farm and user data changes are persisted.
- Relevant users receive notifications when appropriate (e.g., new account created).
- All actions are recorded in an audit log.

## User story
- As a System Administrator
- I want to create, read, update, and deactivate farm records and user accounts
- So that I can keep farm information accurate and control who has access to the system.

## Acceptance criteria
1. Administrator can create a new farm with required fields: name, location, contact person, contact email, and optional description.
2. Administrator can update existing farm data and save changes.
3. Administrator can view a paginated, searchable list of farms and open a farm detail view.
4. Administrator can create user accounts and assign roles and farm associations.
5. Administrator can deactivate/reactivate user accounts; deactivated users cannot log in.
6. The system sends a confirmation email for new accounts and password resets (if configured).
7. All create, update, deactivate actions are logged with timestamp, admin user, and reason (if provided).
8. Validation errors are shown to the admin for missing or malformed required fields.

## Basic flow (happy path)
1. Admin navigates to the "Administrate Farms & Users" section.
2. Admin clicks "Create Farm" and fills required fields.
3. Admin saves; system validates input, persists the farm, and shows success message.
4. Admin clicks "Create User", fills user details, assigns role(s) and links to farm(s), then saves.
5. System creates the user, sends confirmation email, and logs the action.
6. Admin views the farm list, uses search/filters, and opens a farm detail view.

## Alternate flows
- AF1 — Validation error: If required fields are missing or invalid, the system highlights fields and displays error messages; the admin corrects them and retries.
- AF2 — Deactivate user: Admin deactivates a user; the system marks the account inactive, prevents login, and logs the action.
- AF3 — Role change: Admin updates a user role; changes take effect immediately and are logged.

## UI considerations
- Farm list: table with columns for name, location, contact, status, and actions (view/edit/deactivate).
- Farm detail: editable form with clear save/cancel actions and validation messages.
- User form: fields for name, email, roles (multi-select), associated farms (multi-select), and status.
- Confirmation dialogs for destructive actions (deactivate farm/account).

## Security and privacy
- Only administrators with the correct role may access this area.
- Sensitive data (passwords) must not be displayed; password flows use secure reset links.
- Actions must be audited for compliance.

## Performance
- Farm list should paginate and support server-side filtering for large datasets.

## Notes
- Integrate with existing identity provider (if present) for user provisioning.
- Consider bulk import/export for farms and users in future iterations.

## Test cases (high level)
- Create farm with valid data -> success and persisted.
- Create farm with missing required field -> validation error.
- Create user and assign role/farm -> confirmation email sent and user listed.
- Deactivate user -> user cannot authenticate.
- Audit log contains entries for create/update/deactivate actions.
