using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Tests.DataAccess.Mappers;

public class VagtMapperTests
{
    // ── ToDto ────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsAllScalarProperties()
    {
        var vagt = new Vagt
        {
            Id = 7,
            Dato = new DateTime(2025, 1, 3, 20, 0, 0),
            Ædru = true,
            Frigivet = false,
            Medarbejdere = new List<Medarbejder>()
        };

        var dto = VagtMapper.ToDto(vagt);

        Assert.Equal(7, dto.Id);
        Assert.Equal(vagt.Dato, dto.Dato);
        Assert.True(dto.Ædru);
        Assert.False(dto.Frigivet);
    }

    [Fact]
    public void ToDto_MedarbejdereAreMapped()
    {
        var vagt = new Vagt
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<Medarbejder>
            {
                new Medarbejder { Id = 10, Name = "Simon", Bestyrelsesmedlem = false, Vagter = new List<Vagt>() }
            }
        };

        var dto = VagtMapper.ToDto(vagt);

        Assert.Single(dto.Medarbejdere);
        Assert.Equal(10, dto.Medarbejdere[0].Id);
        Assert.Equal("Simon", dto.Medarbejdere[0].Name);
    }

    [Fact]
    public void ToDto_MedarbejderVagterListIsEmpty_PreventingCircularReference()
    {
        var vagt = new Vagt
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<Medarbejder>
            {
                new Medarbejder { Id = 10, Name = "Simon", Vagter = new List<Vagt> { new Vagt { Id = 99 } } }
            }
        };

        var dto = VagtMapper.ToDto(vagt);

        Assert.Empty(dto.Medarbejdere[0].Vagter);
    }

    [Fact]
    public void ToDto_NullMedarbejdere_ReturnsEmptyList()
    {
        var vagt = new Vagt { Id = 1, Dato = DateTime.Now, Medarbejdere = null! };

        var dto = VagtMapper.ToDto(vagt);

        Assert.NotNull(dto.Medarbejdere);
        Assert.Empty(dto.Medarbejdere);
    }

    // ── ToEntity ─────────────────────────────────────────────

    [Fact]
    public void ToEntity_MapsAllScalarProperties()
    {
        var dto = new VagtDTO
        {
            Id = 3,
            Dato = new DateTime(2025, 1, 3, 16, 0, 0),
            Ædru = false,
            Frigivet = true,
            Medarbejdere = new List<MedarbejderDTO>()
        };

        var entity = VagtMapper.ToEntity(dto);

        Assert.Equal(3, entity.Id);
        Assert.Equal(dto.Dato, entity.Dato);
        Assert.False(entity.Ædru);
        Assert.True(entity.Frigivet);
    }

    [Fact]
    public void ToEntity_MedarbejdereIsAlwaysEmpty()
    {
        var dto = new VagtDTO
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<MedarbejderDTO>
            {
                new MedarbejderDTO { Id = 5, Name = "Simon" }
            }
        };

        var entity = VagtMapper.ToEntity(dto);

        Assert.Empty(entity.Medarbejdere);
    }
}
