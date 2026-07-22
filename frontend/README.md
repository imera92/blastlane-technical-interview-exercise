# Expense Tracker UI

Angular 20 frontend for the Expense Tracker API.

## Requirements

- Node.js `^20.19.0`, `^22.12.0`, or `^24.0.0`
- npm
- The .NET API running on `http://127.0.0.1:5000`

If you use nvm, run `nvm use` inside `frontend/`; the included `.nvmrc` selects Node 22.

## Run locally

From the repository root, start the API:

```bash
ASPNETCORE_URLS=http://127.0.0.1:5000 dotnet run --project src/ExpenseTracker.Api
```

In another terminal, start Angular:

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200` and use the public reviewer account:

```text
Email: reviewer@example.com
Password: Reviewer123
```

The Angular development server proxies `/api/**` to the .NET API. Browser requests therefore stay on the Angular origin and the Identity cookie works without bearer tokens or development CORS configuration.

Login first obtains an ASP.NET Core antiforgery token. Angular's built-in XSRF support then sends that token automatically on every state-changing relative API request.

## Checks

```bash
npm test -- --watch=false
npm run build
```
