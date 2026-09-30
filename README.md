# Library Management System (ASP.NET Core MVC — Tiny Project)

A simple full-stack web application for managing a library's books, members,
and borrow/return activity. Built with **ASP.NET Core MVC**, **Entity Framework
Core**, and **SQLite**, styled with **Bootstrap 5**.

## Features

- **Admin Login** — the whole app (except the login page) requires signing
  in via cookie authentication. Default demo credentials:
  `admin` / `admin123` (change them in `appsettings.json` under
  `AdminAccount`).
- **Dashboard** — quick stats (total titles, total copies, members, active
  loans, overdue loans, outstanding fines) and a recent-activity table.
- **Books** — add, edit, view, delete, and search the catalog. Tracks total
  vs. available copies automatically.
- **Members** — add, edit, view, delete, and search library members.
- **Borrow / Return** — issue a book to a member (with a configurable loan
  period), return a book, and filter records by Active / Overdue / Returned.
- **Overdue Fines** — automatically calculated at a configurable rate per
  day overdue (`LibrarySettings:FinePerDayOverdue` in `appsettings.json`,
  default ₹5/day). Shown on the Borrow list, the Return confirmation page,
  and each book's/member's loan history.
- Server-side validation on all forms, plus client-side validation via
  jQuery Validate.
- Seed data (3 sample books, 2 sample members) so the app isn't empty on
  first run.

## Project Structure

```
LibraryManagementSystem/
├── Controllers/        HomeController, BooksController, MembersController, BorrowController, AccountController
├── Models/              Book.cs, Member.cs, BorrowRecord.cs
├── Data/                ApplicationDbContext.cs (EF Core + seed data)
├── Helpers/             LibrarySettings.cs (fine-rate holder)
├── Views/               Razor views for each controller, shared layout, login layout
├── wwwroot/css/         site.css (custom styling)
├── Properties/          launchSettings.json (fixed local ports)
├── Program.cs           App startup / DB initialization / auth setup
├── appsettings.json     Connection string, admin credentials, fine rate
└── LibraryManagementSystem.csproj
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Any OS (Windows, macOS, Linux) — Visual Studio, VS Code, or Rider all work

## How to Run

1. Open a terminal in the `LibraryManagementSystem` folder.
2. Restore packages:
   ```
   dotnet restore
   ```
3. Run the app:
   ```
   dotnet run
   ```
4. Open the URL shown in the terminal (e.g. `https://localhost:5101` or
   `http://localhost:5100`) in your browser.
5. Log in with the demo admin credentials: **admin** / **admin123**.

The SQLite database file (`library.db`) is created automatically the first
time the app runs — no manual migration step is required.

### Opening in Visual Studio

1. Double-click `LibraryManagementSystem.csproj` (or create a solution file
   with `dotnet new sln && dotnet sln add LibraryManagementSystem.csproj`).
2. Press **F5** / **Run**.

## Database Schema

- **Book**: Id, Title, Author, ISBN, Category, TotalCopies, AvailableCopies, AddedOn
- **Member**: Id, FullName, Email, Phone, MembershipType, JoinedOn
- **BorrowRecord**: Id, BookId (FK), MemberId (FK), BorrowDate, DueDate, ReturnDate

A borrow record with `ReturnDate == null` represents an active loan; it's
flagged **Overdue** automatically once `DueDate` has passed.

## Notes for Submission

- This project follows the standard ASP.NET Core MVC pattern (Model /
  View / Controller) requested for the Tiny Project.
- Feel free to rename the namespace, add your name/roll number to the
  footer in `Views/Shared/_Layout.cshtml`, or extend it with authentication,
  fines calculation, or reports if your rubric asks for those.
