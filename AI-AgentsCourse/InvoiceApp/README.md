# Getting Started: AI Agents in C#

## InvoiceApp Web Application

This is a self-contained invoicing platform built on .NET and serves as a test bed for your [InvoiceAgentApi](../InvoiceAgentApi/README.md) AI Agent.  All the agent code is contained in [InvoiceAgentApi](../InvoiceAgentApi/README.md), the front end for this application will talk directly to that API when it is running on `http://localhost:5000`.

## About This Project

InvoiceApp is a C# ASP.NET Core web application for submitting and viewing invoices. The UI uses Tailwind CSS for styling and Alpine.js for interactivity, managed via CDN and LibMan.

## Getting Started

### 1. Open This Subfolder
```sh
cd InvoiceApp
```

### 2. Restore Dependencies
- **.NET dependencies** are restored automatically on build.
- **Frontend libraries** (Alpine.js) are managed by [LibMan](https://docs.microsoft.com/en-us/aspnet/core/client-side/libman/):

```sh
libman restore
```

> **Note:** `wwwroot/lib` is ignored in `.gitignore` except for a `.gitkeep` and `README.md` file. All library files are restored via LibMan.

### 3. Create The Local Database

```sh
dotnet ef database update
```

### 4. Run the Application
```sh
dotnet run
```
- The app will start (by default) at: [http://localhost:5000](http://localhost:5000)

### 5. Using the App
- Visit the homepage to submit or view invoices.
- Submitted invoices are stored in a local SQLite database (`Invoices.db`).

## TROUBLESHOOTING

* The Agent popup icon does not appear - try refreshing the page a couple of times
* `SqliteException: SQLite Error 1: 'no such table: Invoices'.` - you need to create the local database file, run `dotnet ef database update`


## Project Structure
- `Pages/` - Razor pages for UI
- `Controllers/` - API endpoints
- `Models/` - Data models
- `wwwroot/` - Static files (CSS, JS, LibMan-managed libraries)
- `libman.json` - LibMan configuration for frontend libraries
- `.gitignore` - Ignores build artifacts and LibMan-managed libraries

## Notes
- Tailwind CSS is loaded via CDN for simplicity. For production, consider integrating a build pipeline for purging unused styles.
- Alpine.js is restored via LibMan and used for lightweight interactivity.
- The SQLite database (`Invoices.db`) is **not** checked into git.
