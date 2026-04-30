# AGENTS.md – Agent Orientation Guide

> Read **INSTRUCTIONS.md** first for full architecture details. This file covers the agent-specific context: how to orient quickly, coding conventions, current limitations, and patterns to follow when adding or changing code.

---

## Active Branch & Repo

- **Branch:** `simonFærdig`
- **Repo:** `https://github.com/LokeMoneyB/EsparantOS` (private)
- **Mounted workspace path:** `EsperantOS/` (note the 'e' — distinct from `EsparantOS/` which is an unrelated Blazor scaffolding folder)
- **Primary project folder:** `EsperantOS/EsperantOS/` (the ASP.NET Core MVC web project)

---

## Tech Stack at a Glance

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core MVC |
| ORM | Entity Framework Core 10 (SQL Server) |
| Database | SQL Server LocalDB (dev only) |
| Auth | ASP.NET Core Cookie Authentication (no Identity) |
| Frontend | Razor Views + Bootstrap 5 + jQuery + vanilla JS |
| Language | C# (backend), Danish (UI text and property names) |

---

## Orientation Checklist (read these files first)

When picking up a task, read these in order:

1. `EsperantOS/EsperantOS/Program.cs` — DI registrations, middleware pipeline, seed data
2. `EsperantOS/EsperantOS/Models/Vagt.cs` + `Medarbejder.cs` — domain entities
3. `EsperantOS/EsperantOS/Controllers/VagtsController.cs` — the main controller
4. `EsperantOS/EsperantOS/BusinessLogic/VagtBLL.cs` — business logic
5. `EsperantOS/EsperantOS/DataAccess/Repositories/VagtRepository.cs` — data access
6. `EsperantOS/EsperantOS/Views/Vagts/Index.cshtml` + `wwwroot/js/vagts.js` — main UI

---

## Coding Conventions

### Naming (Danish domain language)
- `Vagt` = Shift
- `Medarbejder` = Employee / Member
- `Vagter` = Shifts (plural)
- `Medarbejdere` = Employees (plural)
- `Ædru` = Sober (property on Vagt — closing shifts require this)
- `Frigivet` = Released (shift available for pickup)
- `Bestyrelsesmedlem` = Board member (admin flag on Medarbejder)

Do not rename these to English. All domain properties stay in Danish.

### Layering rules — always follow this chain
```
Controller → BLL → UnitOfWork → Repository → DbContext
```
- Controllers **never** inject `EsperantOSContext` directly.
- Controllers **never** inject repositories directly.
- Controllers inject BLL classes only.
- BLL classes inject `IUnitOfWork` only.
- After any write operation in BLL, call `await _unitOfWork.SaveChangesAsync()`.

### DTO → Model conversion
- BLL methods always return `VagtDTO` or `MedarbejderDTO` (never entities).
- Controllers call `.ToModel()` or `.ToModelList()` from `DtoModelExtensions` before passing to views.
- Views are always typed to `Model` classes (e.g. `@model List<Vagt>`), never DTOs.

### Adding a new BLL method
1. Add the method to the BLL class (`VagtBLL.cs` or `MedarbejderBLL.cs`).
2. If new data access is needed, add an interface method to `IVagtRepository` / `IMedarbejderRepository`, then implement in the concrete repository.
3. Use `await _unitOfWork.VagtRepository.YourMethodAsync()` in the BLL.
4. Return a DTO (map the entity with the relevant Mapper).

### Mappers
- `VagtMapper.ToEntity(dto)` deliberately does **not** map `Medarbejdere` — this prevents EF Core from treating navigation collection updates as new inserts.
- `MedarbejderMapper.ToEntity(dto)` similarly does **not** map `Vagter`.
- When updating many-to-many relations in BLL (e.g. `UpdateVagtAsync`), load the tracked entity first, then manually sync the collection.

---

## Current State of the Codebase

### What is fully implemented
- Cookie-based authentication (login/logout)
- Full CRUD for shifts (`VagtsController` — Create, Edit, Delete, Details)
- Release a shift (`Frigiv` action — sets `Frigivet = true`)
- Pick up a released shift (`TagVagt` action — links logged-in user to shift; auto-creates `Medarbejder` if username not found)
- Personal dashboard (Home/Index — shows logged-in user's upcoming unreleased shifts)
- Dual-mode schedule view (by-date tabs / released-shifts-only), driven by `?view=` URL param and `vagts.js`
- Auto-ensure sober closing shift (`EnsureAedruVagterAsync` — called on every Vagts/Index GET)
- Seed data: 4 employees, 10 weeks of shifts, randomised assignment

### Known limitations & tech debt

| Issue | Location | Notes |
|---|---|---|
| Hardcoded credentials (`simon` / `test123`) | `AccountController.cs` | Acceptable for dev; must be replaced before any real deployment |
| Hardcoded DB connection string | `EsperantOSContext.cs` | Should move to `appsettings.json` / user secrets |
| Database dropped and recreated on every startup | `Program.cs` | Development only — remove `EnsureDeleted` before production |
| Only one login user | `AccountController.cs` | All other employees exist in the DB but cannot log in |
| No server-side validation attributes | Models, DTOs | Validation is procedural in controllers — fragile |
| `DayOfWeek` filter done client-side in EF | `VagtRepository.cs` | `DateTime.DayOfWeek` is not translatable to SQL; query fetches all rows first |
| Legacy class libraries not used | `BusinessLogic/`, `DataAccess/` projects | Artefacts of an earlier refactor — ignore or remove |
| AGENTS.md was empty | root | Now filled in |

---

## Adding New Features — Checklist

When implementing a new feature:

- [ ] If new domain state is needed → add property to entity model + DTO
- [ ] If new DB query is needed → add method to `IXxxRepository`, implement in `XxxRepository`
- [ ] Business logic goes in BLL, not controller
- [ ] Controller action: inject BLL, call async method, convert DTO → Model, pass to View
- [ ] New views go in `Views/{ControllerName}/` and must inherit `_Layout.cshtml`
- [ ] Client-side interactivity (no full reload): use `fetch` POST, follow the pattern in `vagts.js`
- [ ] New shifts must pass the Friday constraint check — see `VagtsController.Create` (POST) for the validation pattern
- [ ] Run `dotnet build` from `EsperantOS/EsperantOS/` to verify before finishing

---

## Running the Project

```bash
cd EsperantOS/EsperantOS
dotnet run
```

Default launch URLs (from `launchSettings.json`):
- HTTP: `http://localhost:5219`
- HTTPS: `https://localhost:7040`

Login with: `simon` / `test123`

> The database is fully wiped and reseeded on every startup. Do not rely on persisted data between runs.

---

## File Locations Quick Reference

| What | Where |
|---|---|
| App bootstrap + seed | `EsperantOS/Program.cs` |
| Domain entities | `EsperantOS/Models/` |
| View models | `EsperantOS/Models/HomeViewModel.cs`, `LoginViewModel.cs` |
| DTOs | `EsperantOS/DTO/Model/` |
| BLL services | `EsperantOS/BusinessLogic/` |
| Repositories | `EsperantOS/DataAccess/Repositories/` |
| Unit of Work | `EsperantOS/DataAccess/UnitOfWork/` |
| Mappers | `EsperantOS/DataAccess/Mappers/` |
| DTO → Model extensions | `EsperantOS/Extensions/DtoModelExtensions.cs` |
| Friday/timeslot helpers | `EsperantOS/Helpers/VagtHelpers.cs` |
| DbContext | `EsperantOS/Data/EsperantOSContext.cs` |
| Controllers | `EsperantOS/Controllers/` |
| Views | `EsperantOS/Views/` |
| Layout | `EsperantOS/Views/Shared/_Layout.cshtml` |
| Client-side JS | `EsperantOS/wwwroot/js/vagts.js` |
| Global CSS | `EsperantOS/wwwroot/css/site.css` |
