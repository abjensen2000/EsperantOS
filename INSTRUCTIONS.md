# EsperantOS – Project Instructions & Architecture Reference

> **Active branch:** `simonFærdig`  
> **Framework:** ASP.NET Core MVC · .NET 10 · Entity Framework Core · SQL Server (LocalDB)

---

## Purpose

EsperantOS is a shift-management web application for a student bar called **Esperanto**. It lets members log in, view their upcoming shifts (*vagter*), release shifts they cannot attend, pick up released shifts, and lets administrators create/edit/delete shifts. All shifts are constrained to **Fridays** and one of three fixed time slots.

---

## N-Layer Architecture

The solution is a **single ASP.NET Core MVC project** (`EsperantOS/`) that internally implements a clean N-Layer structure through namespaced folders. There are also two standalone class-library projects (`BusinessLogic/` and `DataAccess/`) that appear to be earlier extraction attempts — the *working* code lives inside the main web project. The layering from top to bottom is:

```
┌──────────────────────────────────────────┐
│           Presentation Layer             │  Controllers + Razor Views
├──────────────────────────────────────────┤
│         Business Logic Layer (BLL)       │  VagtBLL, MedarbejderBLL
├──────────────────────────────────────────┤
│      DTO Layer (transfer objects)        │  VagtDTO, MedarbejderDTO
├──────────────────────────────────────────┤
│         Data Access Layer (DAL)          │  Repositories, UnitOfWork, Mappers
├──────────────────────────────────────────┤
│            Database Layer                │  EF Core DbContext → SQL Server LocalDB
└──────────────────────────────────────────┘
```

---

## Solution Structure

```
EsperantOS/                          ← Root / solution folder
├── EsperantOS.slnx                  ← Solution file
├── INSTRUCTIONS.md                  ← This file
├── AGENTS.md
│
├── EsperantOS/                      ← Main web project (ASP.NET Core MVC)
│   ├── Program.cs                   ← App bootstrap, DI registration, seed data
│   ├── EsperantOS.csproj
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   │
│   ├── Models/                      ← Domain / EF entity models
│   │   ├── Medarbejder.cs
│   │   ├── Vagt.cs
│   │   ├── HomeViewModel.cs
│   │   ├── LoginViewModel.cs
│   │   └── ErrorViewModel.cs
│   │
│   ├── Controllers/                 ← Presentation layer
│   │   ├── HomeController.cs
│   │   ├── VagtsController.cs
│   │   └── AccountController.cs
│   │
│   ├── Views/                       ← Razor views
│   │   ├── Home/Index.cshtml
│   │   ├── Vagts/
│   │   │   ├── Index.cshtml
│   │   │   ├── Details.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── Delete.cshtml
│   │   ├── Account/Login.cshtml
│   │   └── Shared/
│   │       ├── _Layout.cshtml
│   │       ├── Error.cshtml
│   │       └── _ValidationScriptsPartial.cshtml
│   │
│   ├── BusinessLogic/               ← BLL layer (service classes)
│   │   ├── VagtBLL.cs
│   │   └── MedarbejderBLL.cs
│   │
│   ├── DTO/Model/                   ← DTO layer
│   │   ├── VagtDTO.cs
│   │   └── MedarbejderDTO.cs
│   │
│   ├── Data/                        ← DbContext
│   │   └── EsperantOSContext.cs
│   │
│   ├── DataAccess/
│   │   ├── Repositories/            ← DAL: generic + specific repos
│   │   │   ├── IRepository.cs
│   │   │   ├── Repository.cs
│   │   │   ├── IVagtRepository / VagtRepository.cs
│   │   │   └── IMedarbejderRepository / MedarbejderRepository.cs
│   │   ├── UnitOfWork/
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── UnitOfWork.cs
│   │   └── Mappers/                 ← Entity ↔ DTO mapping (static helpers)
│   │       ├── VagtMapper.cs
│   │       └── MedarbejderMapper.cs
│   │
│   ├── Extensions/
│   │   └── DtoModelExtensions.cs    ← DTO → Model extension methods (for Views)
│   │
│   ├── Helpers/
│   │   └── VagtHelpers.cs           ← Friday/timeslot SelectList generators
│   │
│   ├── Properties/launchSettings.json
│   ├── wwwroot/                     ← Static assets (Bootstrap, jQuery, site.css, site.js)
│   └── css/site.css
│
├── BusinessLogic/                   ← Standalone class library (legacy/unused in active branch)
└── DataAccess/                      ← Standalone class library (legacy/unused in active branch)
    └── Data/EsperantOSContext.cs
```

---

## Domain Models

### `Medarbejder` (Employee / Member)
| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Name` | `string` | Used as login username |
| `Bestyrelsesmedlem` | `bool` | Board member flag |
| `Vagter` | `List<Vagt>` | Navigation – many-to-many with Vagt |

### `Vagt` (Shift)
| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Dato` | `DateTime` | Must be a Friday; one of three fixed hours |
| `Ædru` | `bool` | Shift requires the worker to be sober |
| `Frigivet` | `bool` | Shift has been released and is available for pickup |
| `Medarbejdere` | `List<Medarbejder>` | Navigation – many-to-many |

**Shift time slots (fixed):**
- `16:00` – Opening shift
- `20:00` – Evening shift
- `00:00` (technically Saturday) – Closing shift (always requires `Ædru = true`)

---

## Data Flow (Request → Response)

```
HTTP Request
  └─► Controller  (injects BLL via DI)
        └─► BLL   (calls UnitOfWork → Repository)
              └─► Repository  (EF Core query on DbContext)
                    └─► SQL Server (LocalDB)
              ◄── Entity (Vagt / Medarbejder)
        ◄── Mapper converts Entity → DTO
  ◄── BLL returns DTO list
Controller converts DTO → Model via DtoModelExtensions
  └─► View renders Model
```

---

## Key Components

### Program.cs – Bootstrap & Seed
- Registers `EsperantOSContext`, `IUnitOfWork`, `VagtBLL`, `MedarbejderBLL` as **Scoped**.
- Configures **Cookie Authentication** (`/Account/Login`, `/Account/Logout`).
- On startup: **drops and recreates** the database (`EnsureDeleted` + `EnsureCreated`).
- Seeds 4 employees (Simon, Mads, Oliver, Emma) and 10 weeks of Friday shifts (3 shifts/Friday, randomly assigned workers).

### EsperantOSContext
- Uses `UseSqlServer` with LocalDB connection string (hardcoded).
- `DbSet<Vagt> Vagter` and `DbSet<Medarbejder> Medarbejdere`.
- EF Core handles the many-to-many join table automatically.

### Authentication
- Simple **cookie-based** auth. No ASP.NET Identity.
- **Hardcoded credentials:** username `simon`, password `test123` (AccountController).
- All controllers are decorated with `[Authorize]`.
- On successful login the username is stored as `ClaimTypes.Name` and used throughout to look up the logged-in employee by name.

### Generic Repository Pattern
- `IRepository<T>` / `Repository<T>` — CRUD base.
- `IVagtRepository` / `VagtRepository` — extends with:  
  `GetFridayVagterAsync()`, `GetVagterByMedarbejderAsync(id)`, `GetVagtWithMedarbejdereAsync(id)`, `GetAllVagterWithMedarbejdereAsync()`
- `IMedarbejderRepository` / `MedarbejderRepository` — extends with:  
  `GetMedarbejderByNameAsync(name)`, `GetBestyrelsesmedlemmerAsync()`, `GetMedarbejderWithVagterAsync(id)`

### Unit of Work
- `IUnitOfWork` exposes `VagtRepository`, `MedarbejderRepository`, and `SaveChangesAsync()`.
- Controllers never touch the context or repos directly — everything goes through BLL → UoW.

### BLL Classes
**VagtBLL:**
- `GetAllVagterAsync()`, `GetFridayVagterAsync()`, `GetVagtByIdAsync(id)`
- `GetVagterByMedarbejderAsync(id)`, `GetVagterByMedarbejderNameAsync(name)`
- `EnsureAedruVagterAsync()` — auto-creates a sober closing shift for any Friday that lacks one
- `CreateVagtAsync`, `UpdateVagtAsync`, `DeleteVagtAsync`, `VagtExistsAsync`

**MedarbejderBLL:**
- `GetAllMedarbejdereAsync()`, `GetMedarbejderByIdAsync(id)`, `GetMedarbejderByNameAsync(name)`
- `GetBestyrelsesmedlemmerAsync()`
- `CreateMedarbejderAsync`, `UpdateMedarbejderAsync`, `DeleteMedarbejderAsync`

### Mappers & Extensions
- `VagtMapper` / `MedarbejderMapper` — static classes, `ToDto(entity)` and `ToEntity(dto)`.  
  Note: `ToEntity` does **not** map the nested collection (Medarbejdere/Vagter) — this is intentional to avoid circular updates.
- `DtoModelExtensions` — extension methods `ToModel()` / `ToModelList()` on DTOs, used in controllers to produce View models.

### VagtHelpers
- `GetUpcomingFridaysSelectList()` — next 10 Fridays as `SelectListItem` list.
- `GetShiftTimesSelectList()` — fixed 3 time slots.
- `GetShiftDateTime(date, time)` — combines date + time string into `DateTime`; adds a day for midnight closing shifts.

---

## Controllers & Actions

### HomeController (`[Authorize]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | Shows logged-in user's own upcoming shifts |

### VagtsController (`[Authorize]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | Lists all Friday shifts; calls `EnsureAedruVagterAsync` |
| `Details/{id}` | GET | Shows a single shift |
| `Create` | GET | Form with Friday + timeslot dropdowns |
| `Create` | POST | Validates Friday constraint, creates shift |
| `Edit/{id}` | GET | Pre-filled edit form |
| `Edit/{id}` | POST | Updates shift date/time/flags |
| `Frigiv/{id}` | POST | Marks shift as released (`Frigivet = true`) |
| `TagVagt/{id}` | POST | Current user takes a released shift; auto-creates employee if not found |
| `Delete/{id}` | GET | Confirm delete page |
| `Delete/{id}` | POST | Deletes shift |

### AccountController
| Action | Method | Description |
|---|---|---|
| `Login` | GET | Redirects to Home if already authenticated |
| `Login` | POST | Validates hardcoded credentials, issues cookie |
| `Logout` | GET | Signs out, redirects to Login |

---

## NuGet Dependencies (EsperantOS.csproj)
- `Microsoft.EntityFrameworkCore.SqlServer` — EF Core SQL Server provider
- `Microsoft.AspNetCore.Authentication.Cookies` — Cookie auth middleware
- `Azure.Identity` / `Microsoft.Identity.Client` — referenced (likely from scaffolding), not actively used

---

## Known Conventions & Rules

1. **Shifts are always on Fridays** — enforced in both BLL (`EnsureAedruVagterAsync`) and controller validation.
2. **Three fixed time slots per Friday:** 16:00, 20:00, 00:00.
3. **`Ædru = true`** is automatically ensured for the closing slot (last shift of a Friday) by `EnsureAedruVagterAsync`.
4. **Database is reset on every app start** (`EnsureDeleted` + `EnsureCreated` in `Program.cs`) — development only.
5. **Authentication is username-driven** — the logged-in `ClaimTypes.Name` is used to match `Medarbejder.Name`. Case-sensitive.
6. **DTOs are the contract between BLL and Controllers** — Views always receive domain `Model` objects (never DTOs directly).
7. **No validation attributes** on models/DTOs — validation is handled procedurally in controller actions.
8. **Many-to-many** between `Vagt` and `Medarbejder` is managed by EF Core with an implicit join table.
