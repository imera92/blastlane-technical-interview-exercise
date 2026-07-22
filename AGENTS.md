# Repository instructions for Codex

## Git privacy rules

Before running any Git staging, commit, push, or pull-request command:

1. Inspect `git status`.
2. Stage implementation files explicitly; never use `git add .` or `git add -A`.
3. Never use `git add -f` or another force-add mechanism for protected files.


## Branch workflow

- Use `main` as the working branch.
- Commit requested changes directly to `main` unless the user explicitly requests a separate branch.
- Push `main` directly when the user requests a push.
- Do not create feature branches by default.


## Architecture rules

- Preserve the dependency direction: Domain <- Application <- Infrastructure/API.
- Domain must not reference ASP.NET Core, Identity, or EF Core.
- Application must not reference Infrastructure, API, `HttpContext`, `UserManager`, `SignInManager`, or EF Core.
- Controllers must remain thin.
- Do not expose `DbSet` or `IQueryable` outside Infrastructure.
- Do not add MediatR, AutoMapper, generic repositories, microservices, event buses, or unrelated patterns.
- Authentication uses ASP.NET Core Identity cookies, not JWT.
- Authentication endpoints belong in `AuthController` inside the same API host.
- Money is `decimal` in C# and persisted as SQLite INTEGER cents through EF Core value converters.
- API transaction amounts are signed, nonzero decimals and do not include transaction type; positive means income and negative means expense.
- Backend responses group transactions by date; frontend only renders those groups.
- Never accept a user ID in budget or transaction request bodies.

## TDD workflow

For unit-testable behavior:

1. Add the smallest relevant failing unit test.
2. Run it and confirm the expected failure.
3. Implement the minimum production code.
4. Run the relevant test project.
5. Run the full unit-test suite.
6. Refactor only while tests remain green.

Integration tests are currently deferred. Do not add integration-test projects or `WebApplicationFactory` unless explicitly requested.

Do not weaken, delete, or rewrite a valid test merely to make implementation pass.

## Completion report

At the end of every task, report:

- Files created or changed.
- Tests added or changed.
- Commands executed.
- Test results.
- Manual verification performed, if applicable.
- Assumptions or unresolved issues.
