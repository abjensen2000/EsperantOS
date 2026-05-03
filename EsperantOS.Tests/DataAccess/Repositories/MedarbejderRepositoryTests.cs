using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.Tests.DataAccess.Repositories;

public class MedarbejderRepositoryTests : IDisposable
{
    private readonly EsperantOSContext     _context;
    private readonly MedarbejderRepository _repo;

    public MedarbejderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EsperantOSContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EsperantOSContext(options);
        _repo    = new MedarbejderRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Helpers ───────────────────────────────────────────────

    private Medarbejder AddMedarbejder(string name, bool board = false)
    {
        var m = new Medarbejder { Name = name, Bestyrelsesmedlem = board, Vagter = new List<Vagt>() };
        _context.Medarbejdere.Add(m);
        _context.SaveChanges();
        return m;
    }

    private Vagt AddVagtFor(Medarbejder m, DateTime dato)
    {
        var v = new Vagt
        {
            Dato         = dato,
            Medarbejdere = new List<Medarbejder> { m }
        };
        _context.Vagter.Add(v);
        _context.SaveChanges();
        return v;
    }

    // ── Base Repository: GetAllAsync ──────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllMedarbejdere()
    {
        AddMedarbejder("Simon");
        AddMedarbejder("Mads");

        var result = (await _repo.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    // ── Base Repository: GetByIdAsync ─────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMedarbejder()
    {
        var m = AddMedarbejder("Emma");

        var result = await _repo.GetByIdAsync(m.Id);

        Assert.NotNull(result);
        Assert.Equal("Emma", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    // ── Base Repository: AddAsync ─────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsMedarbejder()
    {
        var m = new Medarbejder { Name = "Oliver", Bestyrelsesmedlem = false, Vagter = new List<Vagt>() };

        await _repo.AddAsync(m);
        await _context.SaveChangesAsync();

        Assert.Equal(1, await _context.Medarbejdere.CountAsync());
    }

    // ── Base Repository: UpdateAsync ──────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesArePersisted()
    {
        var m = AddMedarbejder("Simon", board: false);
        m.Bestyrelsesmedlem = true;

        await _repo.UpdateAsync(m);
        await _context.SaveChangesAsync();

        var updated = await _context.Medarbejdere.FindAsync(m.Id);
        Assert.True(updated!.Bestyrelsesmedlem);
    }

    // ── Base Repository: DeleteAsync ──────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesMedarbejderFromDatabase()
    {
        var m = AddMedarbejder("Simon");

        await _repo.DeleteAsync(m.Id);
        await _context.SaveChangesAsync();

        Assert.Equal(0, await _context.Medarbejdere.CountAsync());
    }

    // ── GetMedarbejderByNameAsync ─────────────────────────────

    [Fact]
    public async Task GetMedarbejderByNameAsync_ExactMatch_ReturnsMedarbejder()
    {
        AddMedarbejder("Simon");

        var result = await _repo.GetMedarbejderByNameAsync("Simon");

        Assert.NotNull(result);
        Assert.Equal("Simon", result!.Name);
    }

    [Fact]
    public async Task GetMedarbejderByNameAsync_CaseInsensitive_ReturnsMedarbejder()
    {
        AddMedarbejder("Simon");

        var result = await _repo.GetMedarbejderByNameAsync("simon");

        Assert.NotNull(result);
        Assert.Equal("Simon", result!.Name);
    }

    [Fact]
    public async Task GetMedarbejderByNameAsync_AllUppercase_ReturnsMedarbejder()
    {
        AddMedarbejder("Simon");

        var result = await _repo.GetMedarbejderByNameAsync("SIMON");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetMedarbejderByNameAsync_NoMatch_ReturnsNull()
    {
        AddMedarbejder("Simon");

        var result = await _repo.GetMedarbejderByNameAsync("Ukendt");

        Assert.Null(result);
    }

    // ── GetBestyrelsesmedlemmerAsync ──────────────────────────

    [Fact]
    public async Task GetBestyrelsesmedlemmerAsync_ReturnsOnlyBoardMembers()
    {
        AddMedarbejder("Simon", board: false);
        AddMedarbejder("Mads",  board: true);
        AddMedarbejder("Emma",  board: true);

        var result = await _repo.GetBestyrelsesmedlemmerAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.True(m.Bestyrelsesmedlem));
    }

    [Fact]
    public async Task GetBestyrelsesmedlemmerAsync_NoBoardMembers_ReturnsEmpty()
    {
        AddMedarbejder("Simon", board: false);

        var result = await _repo.GetBestyrelsesmedlemmerAsync();

        Assert.Empty(result);
    }

    // ── GetMedarbejderWithVagterAsync ─────────────────────────

    [Fact]
    public async Task GetMedarbejderWithVagterAsync_IncludesVagter()
    {
        var m = AddMedarbejder("Simon");
        AddVagtFor(m, DateTime.Today.AddDays(1));

        var result = await _repo.GetMedarbejderWithVagterAsync(m.Id);

        Assert.NotNull(result);
        Assert.NotEmpty(result!.Vagter);
    }

    [Fact]
    public async Task GetMedarbejderWithVagterAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repo.GetMedarbejderWithVagterAsync(999);
        Assert.Null(result);
    }
}
