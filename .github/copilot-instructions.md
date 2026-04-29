# EsperantOS Copilot Instructions

## Project Overview
**EsperantOS** is an ASP.NET Core shift-management web application for "ætru" (sober/responsible) Friday bar duty coordination. The app tracks employee shifts, marks shifts as "released" (`Frigivet`) or requiring sobriety (`Ædru`), and ensures adequate coverage.

- **Framework**: ASP.NET Core MVC with Razor Views + Controllers (not Razor Pages)
- **Target**: .NET 10
- **Database**: SQL Server (LocalDB at startup via `EnsureCreated()`)
- **Architecture**: N-tier (Controllers → BLL → DAL with Unit of Work + Repositories)

---

## Architecture Layers

### 1. **Web Layer** (`Controllers/`)
- **Controllers**: `HomeController`, `VagtsController`, `AccountController`
- **Pattern**: Controllers inject BLL (`VagtBLL`, `MedarbejderBLL`) via constructor DI
- **Data Flow**: Controllers receive BLL DTOs, convert them back to **Models** for views (see `VagtsController.Index()`)
- **Auth**: Cookie-based (`[Authorize]` attribute on protected controllers)

### 2. **Business Logic Layer** (`BusinessLogic/`)
- **Classes**: `VagtBLL`, `MedarbejderBLL`
- **Scope**: Scoped via DI in `Program.cs`
- **Responsibility**: 
  - DTO mapping (e.g., `MapToDto()` methods)
  - Business rules (e.g., `EnsureAedruVagterAsync()` ensures at least one "ædru" shift exists)
  - Orchestrating repository calls via `IUnitOfWork`
- **Example**: `VagtBLL.EnsureAedruVagterAsync()` auto-creates an unpaid "ædru" shift if none exist for Friday

### 3. **Data Access Layer** (`DataAccess/`)

#### **Repositories** (`Repositories/`)
- **Pattern**: Generic `Repository<T>` with specialized repositories (`VagtRepository`, `MedarbejderRepository`)
- **Interfaces**: `IRepository<T>`, `IVagtRepository`, `IMedarbejderRepository`
- **Key Methods**:
  - `GetVagterByDayOfWeekAsync(DayOfWeek)` - **Note**: Uses client-side filtering after `ToListAsync()` because `DateTime.DayOfWeek` is not SQL-translatable
  - `GetFridayVagterAsync()` - delegates to above
  - `GetVagterByMedarbejderAsync()` - filters by employee ID
  - `GetAllVagterWithMedarbejdereAsync()` - includes related employees

#### **Unit of Work** (`UnitOfWork/`)
- **Scope**: Injected as scoped in `Program.cs`
- **Purpose**: Lazy-initializes repositories and manages `DbContext` lifetime
- **Interface**: `IUnitOfWork` exposes `VagtRepository` and `MedarbejderRepository`
- **SaveChanges**: Called explicitly after modifications (not auto-committed)

#### **Mappers** (`Mappers/`)
- **Static classes**: `VagtMapper`, `MedarbejderMapper`
- **Methods**: `ToDto()` converts Entity → DTO; `ToEntity()` converts DTO → Entity
- **Purpose**: Detach EF Core tracking and provide clean DTO contracts for BLL/UI

### 4. **Data Transfer Objects** (`DTO/Model/`)
- **Classes**: `VagtDTO`, `MedarbejderDTO`
- **Usage**: Exchanged between Controllers and BLL; enables UI to avoid Entity Framework tracking

### 5. **Domain Models** (`Models/`)
- **Classes**: `Vagt`, `Medarbejder`, `HomeViewModel`, `LoginViewModel`, `ErrorViewModel`
- **Note**: Models in this folder are used by views; they mirror DTOs but may be extended with UI-specific properties

---

## Critical Patterns & Gotchas

### EF Core SQL Translation
**Issue**: `v.Dato.DayOfWeek == dayOfWeek` cannot be translated to SQL.  
**Solution**: Materialize with `.ToListAsync()` first, then filter client-side:
```csharp
var vagter = await _dbSet.Include(v => v.Medarbejdere).ToListAsync();
return vagter.Where(v => v.Dato.DayOfWeek == dayOfWeek).ToList();
```

### DTO Usage in Controllers
Controllers receive BLL-returned DTOs but **must convert back to Models** before passing to views:
```csharp
var vagterDto = await _vagtBLL.GetFridayVagterAsync();
var vagter = vagterDto.Select(dto => new Vagt { Id = dto.Id, ... }).ToList();
// Pass 'vagter' to view, not 'vagterDto'
```

### Database Startup Behavior
- `Program.cs` calls `context.Database.EnsureDeleted()` and `EnsureCreated()` on startup
- Seeds 4 employees + 30 Friday shifts (10 weeks × 3 shifts)
- **Dev-only**: This resets the DB every run; not suitable for production

### Authentication
- **Hardcoded Credentials** in `AccountController.Login()`: username `simon`, password `test123`
- **Cookie-based** with claims (username in `ClaimTypes.Name`)
- Protected routes use `[Authorize]`

---

## Key Files & Patterns

| File | Purpose | Key Pattern |
|------|---------|-------------|
| `Program.cs` | Startup, DI, seeding | Service registration + DB initialization |
| `BusinessLogic/VagtBLL.cs` | Shift business logic | Async BLL methods + DTO mapping |
| `DataAccess/Repositories/VagtRepository.cs` | Shift data access | Client-side `DayOfWeek` filtering |
| `Controllers/VagtsController.cs` | Shift UI endpoints | DTO → Model conversion for views |
| `Data/EsperantOSContext.cs` | EF Core context | LocalDB connection string hardcoded |

---

## Domain Language (Danish)
- **Vagt** = Shift
- **Medarbejder** = Employee
- **Ædru** = Sober (safety-critical closing shift)
- **Frigivet** = Released (employee offered the shift to others)
- **Bestyrelsesmedlem** = Board member

Use these terms consistently in code and comments.

---

## Development Workflows

### Build & Test
```powershell
dotnet build                              # Compile all projects
dotnet build EsperantOS/EsperantOS.csproj  # Compile single project
```

### Adding a New Feature
1. **Create/modify Entity** in `Models/`
2. **Create DTO** in `DTO/Model/` with corresponding mapper
3. **Add Repository method** (or extend existing) in `DataAccess/Repositories/`
4. **Add BLL method** in `BusinessLogic/` with DTO mapping
5. **Add Controller action** that calls BLL, converts DTOs → Models, returns view
6. **Create/modify Razor view** in `Views/{Controller}/`

### Debugging EF Core Queries
- Enable EF Core logging in `Program.cs` or inspect `VagtRepository` client-side filtering
- Check `LocalDB` via SQL Server Management Studio: `(localdb)\MSSQLLocalDB`

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Startup database error | Verify LocalDB is running: `sqllocaldb info` |
| "LINQ expression could not be translated" | Materialize query with `.ToListAsync()` before filtering |
| DTO/Model mismatch in views | Use `Mapper.ToDto()/ToEntity()` and convert before returning to views |
| Hardcoded auth rejected | Use credentials: `simon` / `test123` |
| Database always empty | Check `Program.cs` seeding logic; DB is recreated on every startup |

---

## Code Style & Conventions
- **Naming**: Danish domain terms (`Vagt`, `Medarbejder`); English technical terms
- **Async**: All data-access methods are `async Task<T>` or `async Task`
- **Validation**: Use `ModelState.AddModelError()` in controllers for form errors
- **Dependency Injection**: Constructor-based, scoped for BLL/UoW layers
