using EsperantOS.DTO.Model;
using EsperantOS.Extensions;
using EsperantOS.Models;

namespace EsperantOS.Tests.Extensions;

public class DtoModelExtensionsTests
{
    // ── VagtDTO.ToModel ───────────────────────────────────────

    [Fact]
    public void VagtToModel_Test()
    {
        var dto = new VagtDTO { Id = 5, Dato = new DateTime(2025, 1, 3, 20, 0, 0), Ædru = true, Frigivet = false, Medarbejdere = new List<MedarbejderDTO>() };

        var model = dto.ToModel();

        Assert.Equal(5, model.Id);
        Assert.Equal(dto.Dato, model.Dato);
        Assert.True(model.Ædru);
    }

    [Fact]
    public void VagtToModel_Test_Med_Medarbejdere()
    {
        var dto = new VagtDTO
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 10, Name = "Simon", Vagter = new List<VagtDTO>() } }
        };

        var model = dto.ToModel();

        Assert.Single(model.Medarbejdere);
        Assert.Equal(10, model.Medarbejdere[0].Id);
    }

    // ── List<VagtDTO>.ToModelList ─────────────────────────────

    [Fact]
    public void VagtToModelList_Test()
    {
        var dtos = new List<VagtDTO>
        {
            new VagtDTO { Id = 1, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() },
            new VagtDTO { Id = 2, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() }
        };

        var models = dtos.ToModelList();

        Assert.Equal(2, models.Count);
    }

    // ── MedarbejderDTO.ToModel ────────────────────────────────

    [Fact]
    public void MedarbejderToModel_Test()
    {
        var dto = new MedarbejderDTO { Id = 3, Name = "Mads", Bestyrelsesmedlem = true, Vagter = new List<VagtDTO>() };

        var model = dto.ToModel();

        Assert.Equal(3, model.Id);
        Assert.Equal("Mads", model.Name);
        Assert.True(model.Bestyrelsesmedlem);
    }

    [Fact]
    public void MedarbejderToModel_Test_Med_Vagter()
    {
        var dto = new MedarbejderDTO
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<VagtDTO>
            {
                new VagtDTO { Id = 50, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 99 } } }
            }
        };

        var model = dto.ToModel();

        Assert.Single(model.Vagter);
        Assert.Null(model.Vagter[0].Medarbejdere); // undgår cirkulær reference
    }

    // ── List<MedarbejderDTO>.ToModelList ──────────────────────

    [Fact]
    public void MedarbejderToModelList_Test()
    {
        var dtos = new List<MedarbejderDTO>
        {
            new MedarbejderDTO { Id = 1, Name = "Simon", Vagter = new List<VagtDTO>() },
            new MedarbejderDTO { Id = 2, Name = "Mads",  Vagter = new List<VagtDTO>() }
        };

        Assert.Equal(2, dtos.ToModelList().Count);
    }
}