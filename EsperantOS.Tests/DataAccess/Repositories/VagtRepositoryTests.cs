using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.Tests.DataAccess.Repositories;

/// <summary>
/// Repository-tests kører mod en EF Core InMemory-database, så vi tester
/// faktiske LINQ-forespørgsler uden at røre filsystemet.
/// </summary>
public class VagtRepositoryTests : IDisposable
{
    private readonly EsperantOSContext _context;
    private readonly VagtRepository   _repo;

    public VagtRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EsperantOSContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unik DB per test
            .Options;

        _context = new EsperantOSContext(options);
        _repo    = new VagtRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Helpers ───────────────────────────────────────────────

    private static DateTime NextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    private Medarbejder AddMedarbejder(string name, bool board = false)
    {
        var m = new Medarbejder { Name = name, Bestyrelsesmedlem = board, Vagter = new List<Vagt>() };
        _context.Medarbejdere.Add(m);
        _context.SaveChanges();
        return m;
    }

    private Vagt AddVagt(DateTime dato, bool ædru = false, bool frigivet = false,
                         List<Medarbejder>? medarbejdere = null)
    {
        var v = new Vagt
        {
            Dato         = dato,
            Ædru         = ædru,
            Frigivet     = frigivet,
            Medarbejdere = medarbejdere ?? new List<Medarbejder>()
        };
        _context.Vagter.Add(v);
        _context.SaveChanges();
        return v;
    }

    // ── Base Repository: GetAllAsync ──────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllVagter()
    {
        AddVagt(NextFriday().AddHours(16));
        AddVagt(NextFriday().AddHours(20));

        var result = (await _repo.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var result = await _repo.GetAllAsync();
        Assert.Empty(result);
    }

    // ── Base Repository: GetByIdAsync ─────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsVagt()
    {
        var vagt = AddVagt(NextFriday().AddHours(16));

        var result = await _repo.GetByIdAsync(vagt.Id);

        Assert.NotNull(result);
        Assert.Equal(vagt.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    // ── Base Repository: AddAsync ─────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsVagt()
    {
        var vagt = new Vagt { Dato = NextFriday().AddHours(16), Ædru = false, Frigivet = true, Medarbejdere = new List<Medarbejder>() };

        await _repo.AddAsync(vagt);
        await _context.SaveChangesAsync();

        Assert.Equal(1, await _context.Vagter.CountAsync());
    }

    // ── Base Repository: UpdateAsync ──────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesArePersisted()
    {
        var vagt = AddVagt(NextFriday().AddHours(16), frigivet: false);
        vagt.Frigivet = true;

        await _repo.UpdateAsync(vagt);
        await _context.SaveChangesAsync();

        var updated = await _context.Vagter.FindAsync(vagt.Id);
        Assert.True(updated!.Frigivet);
    }

    // ── Base Repository: DeleteAsync ──────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesVagtFromDatabase()
    {
        var vagt = AddVagt(NextFriday().AddHours(16));

        await _repo.DeleteAsync(vagt.Id);
        await _context.SaveChangesAsync();

        Assert.Equal(0, await _context.Vagter.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_DoesNotThrow()
    {
        await _repo.DeleteAsync(999);
        await _context.SaveChangesAsync(); // bør ikke kaste en exception
    }

    // ── GetVagterByDayOfWeekAsync ─────────────────────────────

    [Fact]
    public async Task GetVagterByDayOfWeekAsync_FiltersByDayOfWeek()
    {
        var friday   = NextFriday().AddHours(20);
        var saturday = friday.AddDays(1);

        AddVagt(friday);
        AddVagt(saturday);

        var result = await _repo.GetVagterByDayOfWeekAsync(DayOfWeek.Friday);

        Assert.Single(result);
        Assert.Equal(DayOfWeek.Friday, result[0].Dato.DayOfWeek);
    }

    // ── GetFridayVagterAsync ──────────────────────────────────

    [Fact]
    public async Task GetFridayVagterAsync_ReturnsOnlyFridayVagter()
    {
        var friday   = NextFriday().AddHours(20);
        var saturday = friday.AddDays(1);

        AddVagt(friday);
        AddVagt(saturday);

        var result = await _repo.GetFridayVagterAsync();

        Assert.Single(result);
        Assert.Equal(DayOfWeek.Friday, result[0].Dato.DayOfWeek);
    }

    [Fact]
    public async Task GetFridayVagterAsync_NoFridayVagter_ReturnsEmpty()
    {
        AddVagt(NextFriday().AddDays(1)); // Lørdag

        var result = await _repo.GetFridayVagterAsync();

        Assert.Empty(result);
    }

    // ── GetVagterByMedarbejderAsync ───────────────────────────

    [Fact]
    public async Task GetVagterByMedarbejderAsync_ReturnsShiftsForEmployee()
    {
        var simon  = AddMedarbejder("Simon");
        var oliver = AddMedarbejder("Oliver");
        var friday = NextFriday().AddHours(16);

        AddVagt(friday,               medarbejdere: new List<Medarbejder> { simon });
        AddVagt(friday.AddHours(4),   medarbejdere: new List<Medarbejder> { oliver });

        var result = await _repo.GetVagterByMedarbejderAsync(simon.Id);

        Assert.Single(result);
        Assert.Contains(result, v => v.Medarbejdere.Any(m => m.Id == simon.Id));
    }

    [Fact]
    public async Task GetVagterByMedarbejderAsync_EmployeeHasNoShifts_ReturnsEmpty()
    {
        var simon = AddMedarbejder("Simon");

        var result = await _repo.GetVagterByMedarbejderAsync(simon.Id);

        Assert.Empty(result);
    }

    // ── GetVagtWithMedarbejdereAsync ──────────────────────────

    [Fact]
    public async Task GetVagtWithMedarbejdereAsync_IncludesMedarbejdere()
    {
        var simon = AddMedarbejder("Simon");
        var vagt  = AddVagt(NextFriday().AddHours(16), medarbejdere: new List<Medarbejder> { simon });

        var result = await _repo.GetVagtWithMedarbejdereAsync(vagt.Id);

        Assert.NotNull(result);
        Assert.Single(result!.Medarbejdere);
        Assert.Equal("Simon", result.Medarbejdere[0].Name);
    }

    [Fact]
    public async Task GetVagtWithMedarbejdereAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repo.GetVagtWithMedarbejdereAsync(999);
        Assert.Null(result);
    }

    // ── GetAllVagterWithMedarbejdereAsync ─────────────────────

    [Fact]
    public async Task GetAllVagterWithMedarbejdereAsync_IncludesMedarbejdereOnAllVagter()
    {
        var simon  = AddMedarbejder("Simon");
        var friday = NextFriday().AddHours(16);

        AddVagt(friday,             medarbejdere: new List<Medarbejder> { simon });
        AddVagt(friday.AddHours(4), medarbejdere: new List<Medarbejder>());

        var result = await _repo.GetAllVagterWithMedarbejdereAsync();

        Assert.Equal(2, result.Count);
        var withEmployee = result.Single(v => v.Medarbejdere.Any());
        Assert.Equal("Simon", withEmployee.Medarbejdere[0].Name);
    }
}
