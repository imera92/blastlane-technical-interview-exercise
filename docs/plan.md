I inspected [AGENTS.md](/home/imera92/batcave/blastlane-technical-interview-exercise/AGENTS.md), the complete three-line [feature brief](/home/imera92/batcave/blastlane-technical-interview-exercise/docs/gen-ai-exercise.md), and the existing Domain/Application/Infrastructure/API patterns.

The feature fits the current architecture cleanly, but implementation should wait for approval of the unresolved contract, domain, persistence, and security decisions below.

## Proposed scope

A backend-only task-management slice providing authenticated CRUD:

- `POST /api/tasks`
- `GET /api/tasks`
- `GET /api/tasks/{taskId}`
- `PUT /api/tasks/{taskId}`
- `DELETE /api/tasks/{taskId}`

The slice would include:

- A task domain entity and status model.
- Application commands, results, service, and persistence abstraction.
- EF Core configuration, repository, and SQLite migration.
- Thin API controller and request/response contracts.
- Domain, Application, Infrastructure-model, and API-controller unit tests.
- Manual verification against a disposable SQLite database.

No user ID would be accepted from the client under the recommended security model.

## Approval-required decisions

These choices affect requirements or contracts and will not be made implicitly.

| Area | Recommended baseline requiring approval |
|---|---|
| Resource route | Use `/api/tasks` and long integer task IDs. |
| Ownership | All task endpoints require Identity-cookie authentication. Ownership comes only from `ICurrentUser`; user IDs are absent from requests and responses. |
| Cross-user access | Return `404 Not Found`, matching existing budget/transaction privacy behavior, rather than revealing that another user owns the task. |
| Title | Required, trimmed, 1–100 characters. |
| Description | Optional; trim when supplied, store empty string for omitted input, maximum 1,000 characters. |
| Status values | Exactly `pending`, `inProgress`, and `completed`. No other values or transition restrictions. |
| Create status | Require the client to supply status. Alternative: omit it and default to `pending`. |
| Due date | Required, date-only ISO value (`YYYY-MM-DD`); past dates are allowed. |
| Update semantics | `PUT` performs full replacement of title, description, status, and due date. No partial `PATCH`. Invalid updates preserve all existing state. |
| JSON field name | Use `dueDate`, consistent with the repository’s existing camel-case JSON convention. The brief says `due_date`; if that spelling is contractual, it should instead be explicitly serialized as `due_date`. |
| Responses | Return only `id`, `title`, `description`, `status`, and the due-date field. Do not add creation/update timestamps. |
| Status persistence | Store status as readable SQLite `TEXT`, not an integer enum ordinal. |
| User relationship | Required task-to-Identity-user foreign key with `DeleteBehavior.Restrict`, matching budgets. |
| List behavior | Return only the current user’s tasks, ordered by ID ascending, without filtering or pagination. |
| HTTP results | Create `201` with `Location`; reads/update `200`; delete `204`; validation `400`; unauthenticated `401`; inaccessible/missing task `404`. |

Please either approve this recommended baseline or specify changes. In particular, the status vocabulary, `dueDate` versus `due_date`, description optionality, create defaults, and update semantics need explicit confirmation.

## Expected files and code changes

Domain:

- New `src/ExpenseTracker.Domain/Tasks/TaskItem.cs`
  - Internal class name `TaskItem` avoids collision with `System.Threading.Tasks.Task`.
  - Owns validation and atomic update behavior.
- New `src/ExpenseTracker.Domain/Tasks/TaskStatus.cs`
  - Added only if the approved status model is an enum.

Application:

- New `src/ExpenseTracker.Application/Abstractions/Persistence/ITaskRepository.cs`
  - User-scoped list and lookup operations, plus add/remove.
- New `src/ExpenseTracker.Application/Tasks/ITaskService.cs`
- New `src/ExpenseTracker.Application/Tasks/TaskService.cs`
  - Authentication checks, ownership-scoped CRUD, error translation, mapping, and unit-of-work calls.
- New models under `src/ExpenseTracker.Application/Tasks/Models/`:
  - `CreateTaskCommand.cs`
  - `UpdateTaskCommand.cs`
  - `TaskResult.cs`
- Modify [DependencyInjection.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Application/DependencyInjection.cs) to register `ITaskService`.

Infrastructure:

- New `src/ExpenseTracker.Infrastructure/Persistence/Configurations/TaskItemConfiguration.cs`
- New `src/ExpenseTracker.Infrastructure/Persistence/Repositories/TaskRepository.cs`
  - EF queries remain internal and always constrain by `UserId`.
- Modify [DependencyInjection.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Infrastructure/DependencyInjection.cs) to register `ITaskRepository`.
- Generate:
  - `.../Migrations/<timestamp>_AddTasks.cs`
  - `.../Migrations/<timestamp>_AddTasks.Designer.cs`
- Modify `ExpenseTrackerDbContextModelSnapshot.cs`.

No public `DbSet` or `IQueryable` would be introduced, and [ExpenseTrackerDbContext.cs](/home/imera92/batcave/blastlane-technical-interview-exercise/src/ExpenseTracker.Infrastructure/Persistence/ExpenseTrackerDbContext.cs) should not need modification.

API:

- New contracts under `src/ExpenseTracker.Api/Contracts/Tasks/`:
  - `CreateTaskRequest.cs`
  - `UpdateTaskRequest.cs`
  - `TaskResponse.cs`
- New `src/ExpenseTracker.Api/Controllers/TasksController.cs`
  - `[Authorize]`, thin mapping, existing `ResultExtensions` error handling.
- Avoid a global JSON-enum configuration change; status serialization should be localized to this contract.

Tests:

- New `tests/ExpenseTracker.Domain.UnitTests/Tasks/TaskItemTests.cs`
- New `tests/ExpenseTracker.Application.UnitTests/Tasks/TaskServiceTests.cs`
- New `tests/ExpenseTracker.Infrastructure.UnitTests/Persistence/TaskPersistenceModelTests.cs`
- New `tests/ExpenseTracker.Api.UnitTests/Controllers/TasksControllerTests.cs`

Existing valid tests would not be weakened or rewritten.

## TDD execution plan

Each stage starts with the smallest failing test and confirms the expected failure before production code is added.

1. Domain creation rules:
   - Valid creation preserves task and ownership data.
   - Invalid/missing/overlong title is rejected.
   - Description constraints are enforced.
   - Only approved statuses are representable.
   - Due-date rules are enforced.

2. Domain update rules:
   - Valid update changes all mutable fields.
   - Invalid update leaves every existing field unchanged.

3. Application create:
   - Authenticated creation assigns `ICurrentUser.UserId`, adds once, and saves once.
   - Unauthenticated creation returns `Unauthorized` without touching persistence.
   - Domain validation becomes a task-validation `Result`.

4. Application reads:
   - List passes the current user ID and returns deterministic ordering.
   - Get uses both task ID and current user ID.
   - Missing or foreign-owned resources produce the same not-found result.

5. Application update/delete:
   - Update validates before saving and saves exactly once.
   - Invalid update does not save.
   - Delete removes and saves exactly once.
   - Missing/foreign-owned records are not mutated.

6. Infrastructure model:
   - Required scalar fields and approved maximum lengths.
   - Status storage policy.
   - Date-only persistence.
   - Required user foreign key and deletion behavior.
   - User-scoped list index.

7. API contract/controller:
   - Authorization metadata exists.
   - Requests map exactly to commands.
   - Responses expose the approved JSON contract.
   - CRUD success statuses and `CreatedAtAction` route values.
   - Validation, unauthorized, and not-found results use existing problem-details mappings.
   - Data annotations reject missing or oversized fields where applicable.

8. Migration:
   - Generate `AddTasks` only after the model test is green.
   - Inspect the migration and snapshot for unrelated changes.
   - Verify there are no pending model changes.

After each production step, run the relevant test project. After each coherent red/green cycle, run the full solution suite as required by `AGENTS.md`.

## Planned verification commands

- Focused `dotnet test` commands for each affected test project.
- `dotnet test ExpenseTracker.sln --no-restore --nologo`
- Repository-local EF migration generation.
- `dotnet tool run dotnet-ef migrations has-pending-model-changes ...`
- `git diff --check`
- `git diff --stat`
- Manual API run against a disposable SQLite database under `/tmp`.
- Cookie/XSRF `curl` workflow verifying:
  - anonymous access returns `401`;
  - one user can complete CRUD;
  - invalid input returns `400`;
  - a second user cannot list or access the first user’s tasks;
  - deleted tasks return `404`.

## Explicit exclusions

- Angular/frontend task screens or services.
- Integration-test projects, `WebApplicationFactory`, Testcontainers, or persistent SQLite test fixtures.
- Pagination, searching, filtering, sorting parameters, bulk operations, or task assignment.
- Categories, priorities, labels, subtasks, reminders, recurrence, comments, or audit history.
- PATCH semantics.
- Roles or administrator access.
- JWT authentication.
- MediatR, AutoMapper, generic repositories, event buses, or new architectural patterns.
- OpenAPI/Swagger changes.
- Changes to existing budget or transaction contracts.
- Documentation-file edits unless separately requested.

## Completion criteria

The slice is complete when:

- All five CRUD routes implement the approved contract.
- Every query and mutation is scoped to the authenticated user.
- Domain invariants and atomic updates are unit tested.
- Application orchestration and failure paths are unit tested.
- EF model metadata and the migration match the approved data model.
- Controller mapping, authorization metadata, and HTTP results are unit tested.
- All existing and new .NET tests pass.
- EF reports no pending model changes.
- Manual two-user ownership verification passes.
- Diff review finds no unrelated or protected-file changes.
- The completion report lists files, tests, commands, results, manual checks, and unresolved issues.

## Inspection report

- Files changed: none.
- Tests added or changed: none.
- Tests executed: none; avoiding build-artifact writes during this inspection-only task.
- Commands executed: repository/file inspection with `pwd`, `cat`, `sed`, `rg`, `wc`, `git status`, `git branch`, `git diff --stat`, and `git diff --check`.
- Current worktree note: `AGENTS-OG.md`, `AGENTS.md`, and `docs/` were already untracked. They were not modified.
- Current branch: `main`.
- Unresolved issue: implementation is awaiting approval of the decisions above.
