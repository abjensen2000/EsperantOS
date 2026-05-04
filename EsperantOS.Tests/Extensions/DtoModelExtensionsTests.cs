using EsperantOS.DTO.Model;
using EsperantOS.Extensions;
using EsperantOS.Models;

namespace EsperantOS.Tests.Extensions;

public class DtoModelExtensionsTests
{
    // ── VagtDTO.ToModel ───────────────────────────────────────

    [Fact]
    public void VagtToModel_MapsAllScalarProperties()
    {
        var dto = new VagtDTO
        {
            Id = 5,
            Dato = new DateTime(2025, 1, 3, 20, 0, 0),
            Ædru = true,
            Frigivet = false,
            Medarbejdere = new List<MedarbejderDTO>()
        };

        var model = dto.ToModel();

        Assert.Equal(5, model.Id);
        Assert.Equal(dto.Dato, model.Dato);
        Assert.True(model.Ædru);
        Assert.False(model.Frigivet);
    }

    [Fact]
    public void VagtToModel_MedarbejdereAreMapped()
    {
        var dto = new VagtDTO
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<MedarbejderDTO>
            {
                new MedarbejderDTO { Id = 10, Name = "Simon", Bestyrelsesmedlem = false, Vagter = new List<VagtDTO>() }
            }
        };

        var model = dto.ToModel();

        Assert.Single(model.Medarbejdere);
        Assert.Equal(10, model.Medarbejdere[0].Id);
        Assert.Equal("Simon", model.Medarbejdere[0].Name);
    }

    [Fact]
    public void VagtToModel_NullMedarbejdere_ReturnsEmptyList()
    {
        var dto = new VagtDTO { Id = 1, Dato = DateTime.Now, Medarbejdere = null! };

        var model = dto.ToModel();

        Assert.NotNull(model.Medarbejdere);
        Assert.Empty(model.Medarbejdere);
    }

    // ── List<VagtDTO>.ToModelList ─────────────────────────────

    [Fact]
    public void VagtToModelList_ConvertsAllItems()
    {
        var dtos = new List<VagtDTO>
        {
            new VagtDTO { Id = 1, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() },
            new VagtDTO { Id = 2, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() }
        };

        var models = dtos.ToModelList();

        Assert.Equal(2, models.Count);
        Assert.Equal(1, models[0].Id);
        Assert.Equal(2, models[1].Id);
    }

    [Fact]
    public void VagtToModelList_EmptyList_ReturnsEmptyList()
    {
        var models = new List<VagtDTO>().ToModelList();
        Assert.Empty(models);
    }

    // ── MedarbejderDTO.ToModel ────────────────────────────────

    [Fact]
    public void MedarbejderToModel_MapsAllScalarProperties()
    {
        var dto = new MedarbejderDTO
        {
            Id = 3,
            Name = "Mads",
            Bestyrelsesmedlem = true,
            Vagter = new List<VagtDTO>()
        };

        var model = dto.ToModel();

        Assert.Equal(3, model.Id);
        Assert.Equal("Mads", model.Name);
        Assert.True(model.Bestyrelsesmedlem);
    }

    [Fact]
    public void MedarbejderToModel_VagterMappedWithoutNestedMedarbejdere()
    {
        var dto = new MedarbejderDTO
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<VagtDTO>
            {
                new VagtDTO
                {
                    Id = 50,
                    Dato = new DateTime(2025, 1, 3, 20, 0, 0),
                    Ædru = false,
                    Frigivet = true,
                    Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 99 } }
                }
            }
        };

        var model = dto.ToModel();

        Assert.Single(model.Vagter);
        Assert.Equal(50, model.Vagter[0].Id);
        // Indlejrede Medarbejdere skal være fraværende for at undgå cirkulær reference
        Assert.Null(model.Vagter[0].Medarbejdere);
    }

    [Fact]
    public void MedarbejderToModel_NullVagter_ReturnsEmptyList()
    {
        var dto = new MedarbejderDTO { Id = 1, Name = "Simon", Vagter = null! };

        var model = dto.ToModel();

        Assert.NotNull(model.Vagter);
        Assert.Empty(model.Vagter);
    }

    // ── List<MedarbejderDTO>.ToModelList ──────────────────────

    [Fact]
    public void MedarbejderToModelList_ConvertsAllItems()
    {
        var dtos = new List<MedarbejderDTO>
        {
            new MedarbejderDTO { Id = 1, Name = "Simon", Vagter = new List<VagtDTO>() },
            new MedarbejderDTO { Id = 2, Name = "Mads",  Vagter = new List<VagtDTO>() }
        };

        var models = dtos.ToModelList();

        Assert.Equal(2, models.Count);
        Assert.Equal("Simon", models[0].Name);
        Assert.Equal("Mads", models[1].Name);
    }

    [Fact]
    public void MedarbejderToModelList_EmptyList_ReturnsEmptyList()
    {
        var models = new List<MedarbejderDTO>().ToModelList();
        Assert.Empty(models);
    }
}
