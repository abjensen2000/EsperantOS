using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Tests.DataAccess.Mappers;

public class MedarbejderMapperTests
{
    // ── ToDto ────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsAllScalarProperties()
    {
        var medarbejder = new Medarbejder
        {
            Id = 4,
            Name = "Mads",
            Bestyrelsesmedlem = true,
            Vagter = new List<Vagt>()
        };

        var dto = MedarbejderMapper.ToDto(medarbejder);

        Assert.Equal(4, dto.Id);
        Assert.Equal("Mads", dto.Name);
        Assert.True(dto.Bestyrelsesmedlem);
    }

    [Fact]
    public void ToDto_VagterAreMapped()
    {
        var medarbejder = new Medarbejder
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<Vagt>
            {
                new Vagt { Id = 20, Dato = new DateTime(2025, 1, 3, 20, 0, 0), Ædru = false, Frigivet = true, Medarbejdere = new List<Medarbejder>() }
            }
        };

        var dto = MedarbejderMapper.ToDto(medarbejder);

        Assert.Single(dto.Vagter);
        Assert.Equal(20, dto.Vagter[0].Id);
    }

    [Fact]
    public void ToDto_VagtMedarbejdereIsEmpty_PreventingCircularReference()
    {
        var medarbejder = new Medarbejder
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<Vagt>
            {
                new Vagt
                {
                    Id = 20,
                    Dato = DateTime.Now,
                    Medarbejdere = new List<Medarbejder> { new Medarbejder { Id = 99 } }
                }
            }
        };

        var dto = MedarbejderMapper.ToDto(medarbejder);

        Assert.Empty(dto.Vagter[0].Medarbejdere);
    }

    [Fact]
    public void ToDto_NullVagter_ReturnsEmptyList()
    {
        var medarbejder = new Medarbejder { Id = 1, Name = "Simon", Vagter = null! };

        var dto = MedarbejderMapper.ToDto(medarbejder);

        Assert.NotNull(dto.Vagter);
        Assert.Empty(dto.Vagter);
    }

    // ── ToEntity ─────────────────────────────────────────────

    [Fact]
    public void ToEntity_MapsAllScalarProperties()
    {
        var dto = new MedarbejderDTO
        {
            Id = 5,
            Name = "Emma",
            Bestyrelsesmedlem = false,
            Vagter = new List<VagtDTO>()
        };

        var entity = MedarbejderMapper.ToEntity(dto);

        Assert.Equal(5, entity.Id);
        Assert.Equal("Emma", entity.Name);
        Assert.False(entity.Bestyrelsesmedlem);
    }

    [Fact]
    public void ToEntity_VagterIsAlwaysEmpty()
    {
        var dto = new MedarbejderDTO
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<VagtDTO>
            {
                new VagtDTO { Id = 10, Dato = DateTime.Now }
            }
        };

        var entity = MedarbejderMapper.ToEntity(dto);

        Assert.Empty(entity.Vagter);
    }
}
