
# RaceDay API (Part 2 - PROG6212 POE)

ASP.NET Core Web API for the RaceDay system, built with Entity Framework Core
(Code-First) and JWT authentication with role-based access control (Organiser / Participant).

## How everything lines up
- `Models/` + `Data/RaceDayDbContext.cs` match the Part 1 ERD and `racecar-schema-poepart1.sql`
  (table names, column names, types, defaults, unique constraints and FKs are identical).
- `Controllers/` implement every route documented in `st10478076-api-plan-tina-jeke-prog6212.docx`,
  including the documented status codes (201/400/401/403/404/409/204).

## Setup in Visual Studio
1. Open `RaceDayAPI.csproj` in Visual Studio 2022.
2. Update the connection string in `appsettings.json` if your SQL Server
   is not the local default (`Server=.;`).
3. Open Package Manager Console and run:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
4. Run the project (F5). Swagger opens automatically.
5. On first run the database is seeded (roles, users, events, categories,
   enrolments and results) to match the SQL script.

> Note: your Part 1 SQL script drops/recreates `RaceDayDB`. Run the API AFTER
> running the EF migration (the API recreates the schema via Code-First), or
> point the API at a fresh copy of the database.

## Seeded logins (all passwords: `Password123!`)
| Role        | Email                  |
|-------------|------------------------|
| Organiser   | thandiwe@raceday.co.za |
| Organiser   | james@raceday.co.za    |
| Participant | tina@example.com       |
| Participant | tshepo@example.com     |
| Participant | amy@example.com        |

## Testing in Swagger
1. POST `/api/auth/login` with a seeded account -> copy the `token`.
2. Click **Authorize** and enter: `Bearer {token}`.
3. Test role-restricted endpoints - wrong role returns 403, no token returns 401.

## Endpoint summary (matches Part 1 API plan)
| Method | Route | Role |
|--------|-------|------|
| POST   | /api/auth/register | None |
| POST   | /api/auth/login | None |
| GET/PUT| /api/users/me | Any |
| GET    | /api/events | None (upcoming) |
| GET    | /api/events/{id} | None |
| POST   | /api/events | Organiser |
| PUT/DELETE | /api/events/{id} | Organiser (owner) |
| GET    | /api/events/{id}/categories | None |
| POST   | /api/events/{id}/categories | Organiser (owner) |
| PUT/DELETE | /api/categories/{id} | Organiser (owner) |
| POST   | /api/categories/{id}/enrol | Participant |
| GET    | /api/users/me/enrolments | Participant |
| GET    | /api/categories/{id}/enrolments | Organiser (owner) |
| DELETE | /api/enrolments/{id} | Participant (owner) |
| POST   | /api/enrolments/{id}/result | Organiser (owner) |
| PUT    | /api/results/{id} | Organiser (owner) |
| GET    | /api/categories/{id}/result | None (leaderboard by position) |
