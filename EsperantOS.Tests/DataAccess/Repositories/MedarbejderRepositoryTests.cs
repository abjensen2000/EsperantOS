using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.Tests.DataAccess.Repositories;

public class MedarbejderRepositoryTests : IDisposable
{
    private readonly EsperantOSContext _context;
    private readonly MedarbejderRepository _repo;

    public MedarbejderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EsperantOSContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new EsperantOSContext(options);
        _repo = new MedarbejderRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private Medarbejder AddMedarbejder(string name, bool board = false)
    {
        var m = new Medarbejder { Name = name, Bestyrelsesmedlem = board, Vagter = new List<Vagt>() };
        _context.Medarbejdere.Add(m);
        _context.SaveChanges();
        return m;
    }

    // ── GetMedarbejderByNameAsync ─────────────────────────────

    [Fact]
    public async Task GetByName_Test()
    {
        AddMedarbejder("Simon");

        var result = await _repo.GetMedarbejderByNameAsync("Simon");

        Assert.NotNull(result);
        Assert.Equal("Simon", result!.Name);
    }

    

    // ── GetBestyrelsesmedlemmerAsync ──────────────────────────

    [Fact]
    public async Task GetBestyrelsesmedlemmer_Test()
    {
        AddMedarbejder("Simon", board: false);
        AddMedarbejder("Mads", board: true);
        AddMedarbejder("Emma", board: true);

        var result = await _repo.GetBestyrelsesmedlemmerAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.True(m.Bestyrelsesmedlem));
    }

    // ── GetMedarbejderWithVagterAsync ─────────────────────────

    [Fact]
    public async Task GetWithVagter_Test()
    {
        var m = AddMedarbejder("Simon");
        _context.Vagter.Add(new Vagt { Dato = DateTime.Today.AddDays(1), Medarbejdere = new List<Medarbejder> { m } });
        _context.SaveChanges();

        var result = await _repo.GetMedarbejderWithVagterAsync(m.Id);

        Assert.NotNull(result);
        Assert.NotEmpty(result!.Vagter);
    }
}