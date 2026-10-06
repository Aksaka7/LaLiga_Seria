# LaLiga

A football league application built as two separate projects: an ASP.NET Core 8 Web API that owns the data and the rules, and an MVC front end that consumes it over HTTP.

<!-- Add a screenshot: upload it to docs/ and replace this line with ![Standings](docs/standings.png) -->

This project is based on a case study from the training I received at M&Y Yazılım Eğitim Akademi, under the guidance of Murat Yücedağ and Erhan Gündüz. The case asked for a Serie A application; I built it for La Liga instead, wrote the code myself, and extended the API beyond the minimum so the front end could match the instructor's five UI designs one to one.

## Image
<img width="1438" height="796" alt="image" src="https://github.com/user-attachments/assets/688f9634-e51c-45db-9628-761b09f2cfbd" />

### Public pages

- __Standings:__ points, goal difference, goals scored and recent form for every active team, with search and filters
- __Weekly fixtures:__ matches grouped by week, with a week selector and match status (not started, live, finished, postponed)
- __Match detail:__ goals with assists, cards, substitutions and match statistics such as possession and shots

### Admin panel

- __Matches:__ list with server-side filtering, plus create, edit and delete
- __Teams:__ list with sorting and filters, plus create, edit and delete
- __Rules:__ every write goes through the API's business rules, and the API's error messages are shown next to the related form field

### Business rules enforced by the API

- A team cannot play against itself
- A passive team cannot be added to a match
- A team can play only one match per week
- A live or finished match must have both scores
- A match that has goals, cards, substitutions or statistics cannot be moved back to "not started" or "postponed"
- A team with matches cannot be deleted (409 Conflict); it can be made passive instead
- On shot statistics, shots on target cannot exceed total shots

## Tech Stack

- __Framework:__ ASP.NET Core 8 Web API and ASP.NET Core 8 MVC
- __Database:__ SQL Server and Entity Framework Core 8 (Code First, migrations)
- __API documentation:__ Swagger (Swashbuckle)
- __Front end:__ Razor views, Bootstrap 5.3 (CSS only, from CDN) and small vanilla JavaScript files
- __Communication:__ `HttpClient` through a typed `LaLigaApiClient` class

## Architecture

```
LaLiga/            Web API
  Controllers/     Teams, Matches, MatchEvents, Standings, Weeks
  Entities/        Team, Match, MatchGoal, MatchCard, Substitution, MatchStatistic
  DTOs/            request and response models
  Services/        StandingsService
  Context/         LaLigaContext

LaLiga.WebUI/      MVC front end
  Controllers/     Standings, Fixtures, Match, Admin
  ViewModels/      page models and the API client
  Filters/         ApiExceptionFilter
```

- __Calculated standings:__ The table is not stored anywhere. `StandingsService` calculates it on every request from finished matches only: three points for a win, one for a draw, then sorted by points, goal difference and goals scored.
- __No direct database access in the front end:__ The front end never reads the database. If the API is down, the user sees a clear error page (503) instead of a crash.
- __One error format:__ All API errors use the same shape, `{ message, errors }`, so the front end can show them consistently.

### API endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/standings` | Calculated league table |
| GET | `/api/weeks` | Weeks with their featured match |
| GET | `/api/matches` | Matches, with optional filters |
| GET | `/api/matches/week/{week}` | Matches of one week |
| GET | `/api/matches/{id}` | Match detail with all events |
| POST, PUT, DELETE | `/api/matches` | Match management |
| POST, DELETE | `/api/matches/{id}/goals`, `/cards`, `/substitutions` | Match events |
| PUT, DELETE | `/api/matches/{id}/statistic` | Match statistics |
| GET, POST, PUT, DELETE | `/api/teams` | Team management |

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (Express or LocalDB)

### Setup

```bash
git clone https://github.com/Aksaka7/LaLiga_Seria.git
cd LaLiga_Seria
```

1. Set your connection string in `LaLiga/appsettings.json` under `ConnectionStrings:DefaultConnection`.
2. Create the database:

```bash
cd LaLiga
dotnet ef database update
```

3. Start both projects. In Visual Studio, set multiple startup projects; from the command line, run each in its own terminal:

```bash
dotnet run --project LaLiga
dotnet run --project LaLiga.WebUI
```

- __API and Swagger:__ https://localhost:7171/swagger
- __Web UI:__ https://localhost:7278

The Web UI reads the API address from `LaLiga.WebUI/appsettings.json` (`ApiSettings:BaseUrl`). The database starts empty; add teams and matches from the admin panel.

## What I learned

- __Leftover files can change the whole page.__ The MVC template ships a `_Layout.cshtml.css` file. Because it existed, Razor turned on CSS isolation and added a `b-xxxxxxxxxx` attribute to every element in the layout, which broke my selectors in unexpected places. Deleting the leftover file fixed it. Now I clean template files before building on them.
- __An edit form can silently erase data.__ The match edit form only shows some fields. On the first version, saving it sent empty values for referee, attendance, minute and note, and the API overwrote the real values. I kept those fields in hidden inputs so an edit only changes what the user actually touched.
- __A unique index needs clean data first.__ My second migration added a unique index on the team code, but the existing rows all had an empty code, so the migration failed. I rolled it back, fixed the data, and applied it again. Now I check existing data before adding a constraint.
- __File encoding matters for Turkish text.__ `Program.cs` had been saved in ANSI, which broke Turkish characters in error messages. I added an `.editorconfig` with `charset = utf-8-bom` to the solution so every new file uses the same encoding.

## Status

Complete. All case requirements and all five UI screens are implemented.

## Acknowledgements

Thanks to Murat Yücedağ and Erhan Gündüz at M&Y Yazılım Eğitim Akademi for the training and the case study this project grew out of.

## About me

Mehmet Asker, a self-taught full stack developer with a background in operations management. More projects: [github.com/m3hmtA-k3r](https://github.com/m3hmtA-k3r)
