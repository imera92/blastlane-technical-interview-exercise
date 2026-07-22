Implemented the approved task CRUD slice with persisted, user-scoped SQLite storage.

## Files changed

- Domain: [TaskItem.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Domain/Tasks/TaskItem.cs), [TaskStatus.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Domain/Tasks/TaskStatus.cs)
- Application: task repository abstraction, service, interface, commands/results, and [DependencyInjection.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Application/DependencyInjection.cs)
- Infrastructure: EF configuration, repository, DI registration, `AddTasks` migration/designer, and model snapshot
- API: create/update contracts, response contract, and [TasksController.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Api/Controllers/TasksController.cs)
- Tests: four new task-focused test files across Domain, Application, Infrastructure, and API

The pre-existing `README.md` modification and untracked repository documentation were preserved.

## Tests

Added:

- 12 Domain tests
- 15 Application tests
- 4 Infrastructure model tests
- 11 API tests

Final focused results:

- Domain: 12/12 passed
- Application: 15/15 passed
- Infrastructure: 4/4 passed
- API: 11/11 passed

Full suite: **177/177 passed**.

## Commands

Executed TDD red/green runs with focused `dotnet test` commands, followed by:

- `dotnet test ExpenseTracker.sln --no-restore --nologo`
- `dotnet tool run dotnet-ef migrations add AddTasks ...`
- `dotnet tool run dotnet-ef migrations has-pending-model-changes ...`
- `dotnet run --no-build ...`
- Localhost `curl` authentication and CRUD checks
- `git status`, `git diff --stat`, `git diff --check`, and architecture/whitespace searches

EF reported: **No changes have been made to the model since the last migration.**

## Manual verification

Against a fresh disposable SQLite database:

- Anonymous list: `401`
- Invalid create: `400`
- Create: `201`
- List/get/update/read-after-update: `200`
- Second user saw an empty list
- Second-user get/update/delete attempts: `404`
- Owner data remained unchanged after foreign attempts
- Owner delete: `204`
- Read after delete: `404`

The API was stopped afterward. No repository database was modified.

## Unresolved issues

None. No unrelated implementation changes, commits, staging, or pushes were made.
