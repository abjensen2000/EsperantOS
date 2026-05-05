using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.Tests.DataAccess.Repositories;

public class VagtRepositoryTests : IDisposable
{
    private readonly EsperantOSContext _context;
    private readonly VagtRepository _repo;

    public VagtRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EsperantOSContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new EsperantOSContext(options);
        _repo = new VagtRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static DateTime NextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    private Medarbejder AddMedarbejder(string name)
    {
        var m = new Medarbejder { Name = name, Vagter = new List<Vagt>() };
        _context.Medarbejdere.Add(m);
        _context.SaveChanges();
        return m;
    }

    private Vagt AddVagt(DateTime dato, bool ædru = false, bool frigivet = false, List<Medarbejder>? medarbejdere = null)
    {
        var v = new Vagt { Dato = dato, Ædru = ædru, Frigivet = frigivet, Medarbejdere = medarbejdere ?? new List<Medarbejder>() };
        _context.Vagter.Add(v);
        _context.SaveChanges();
        return v;
    }

    // ── GetFridayVagterAsync ──────────────────────────────────

    [Fact]
    public async Task GetFridayVagter_Test()
    {
        AddVagt(NextFriday().AddHours(20));
        AddVagt(NextFriday().AddDays(1)); // lørdag

        var result = await _repo.GetFridayVagterAsync();

        Assert.Single(result);
        Assert.Equal(DayOfWeek.Friday, result[0].Dato.DayOfWeek);
    }

    // ── GetVagterByMedarbejderAsync ───────────────────────────

    [Fact]
    public async Task GetVagterByMedarbejder_Test()
    {
        var simon = AddMedarbejder("Simon");
        var oliver = AddMedarbejder("Oliver");
        var friday = NextFriday().AddHours(16);

        AddVagt(friday, medarbejdere: new List<Medarbejder> { simon });
        AddVagt(friday.AddHours(4), medarbejdere: new List<Medarbejder> { oliver });

        var result = await _repo.GetVagterByMedarbejderAsync(simon.Id);

        Assert.Single(result);
        Assert.Contains(result, v => v.Medarbejdere.Any(m => m.Id == simon.Id));
    }

    // ── GetVagtWithMedarbejdereAsync ──────────────────────────

    [Fact]
    public async Task GetVagtWithMedarbejdere_Test()
    {
        var simon = AddMedarbejder("Simon");
        var vagt = AddVagt(NextFriday().AddHours(16), medarbejdere: new List<Medarbejder> { simon });

        var result = await _repo.GetVagtWithMedarbejdereAsync(vagt.Id);

        Assert.NotNull(result);
        Assert.Equal("Simon", result!.Medarbejdere[0].Name);
    }


    // ── GetAllVagterWithMedarbejdereAsync ─────────────────────

    [Fact]
    public async Task GetAllVagterWithMedarbejdere_Test()
    {
        var simon = AddMedarbejder("Simon");
        var friday = NextFriday().AddHours(16);

        AddVagt(friday, medarbejdere: new List<Medarbejder> { simon });
        AddVagt(friday.AddHours(4), medarbejdere: new List<Medarbejder>());

        var result = await _repo.GetAllVagterWithMedarbejdereAsync();

        Assert.Equal(2, result.Count);
        Assert.Single(result.Single(v => v.Medarbejdere.Any()).Medarbejdere);
    }
}