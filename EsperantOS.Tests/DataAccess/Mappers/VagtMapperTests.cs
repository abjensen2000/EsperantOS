using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Tests.DataAccess.Mappers;

public class VagtMapperTests
{
    // ── ToDto ─────────────────────────────────────────────────

    [Fact]
    public void ToDto_Test()
    {
        var vagt = new Vagt { Id = 7, Dato = new DateTime(2025, 1, 3, 20, 0, 0), Ædru = true, Frigivet = false, Medarbejdere = new List<Medarbejder>() };

        var dto = VagtMapper.ToDto(vagt);

        Assert.Equal(7, dto.Id);
        Assert.Equal(vagt.Dato, dto.Dato);
        Assert.True(dto.Ædru);
        Assert.False(dto.Frigivet);
    }

    [Fact]
    public void ToDto_Test_Med_Medarbejdere()
    {
        var vagt = new Vagt
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<Medarbejder> { new Medarbejder { Id = 10, Name = "Simon", Vagter = new List<Vagt>() } }
        };

        var dto = VagtMapper.ToDto(vagt);

        Assert.Single(dto.Medarbejdere);
        Assert.Equal(10, dto.Medarbejdere[0].Id);
    }


    // ── ToEntity ──────────────────────────────────────────────

    [Fact]
    public void ToEntity_Test()
    {
        var dto = new VagtDTO { Id = 3, Dato = new DateTime(2025, 1, 3, 16, 0, 0), Ædru = false, Frigivet = true, Medarbejdere = new List<MedarbejderDTO>() };

        var entity = VagtMapper.ToEntity(dto);

        Assert.Equal(3, entity.Id);
        Assert.Equal(dto.Dato, entity.Dato);
        Assert.True(entity.Frigivet);
    }

    [Fact]
    public void ToEntity_Test_Med_Medarbejdere()
    {
        var dto = new VagtDTO
        {
            Id = 1,
            Dato = DateTime.Now,
            Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 5, Name = "Simon" } }
        };

        Assert.Empty(VagtMapper.ToEntity(dto).Medarbejdere);
    }
}