using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Tests.DataAccess.Mappers;

public class MedarbejderMapperTests
{
    // ── ToDto ─────────────────────────────────────────────────

    [Fact]
    public void ToDto_Test()
    {
        var m = new Medarbejder { Id = 4, Name = "Mads", Bestyrelsesmedlem = true, Vagter = new List<Vagt>() };

        var dto = MedarbejderMapper.ToDto(m);

        Assert.Equal(4, dto.Id);
        Assert.Equal("Mads", dto.Name);
        Assert.True(dto.Bestyrelsesmedlem);
    }

    [Fact]
    public void ToDto_Test_Med_Vagter()
    {
        var m = new Medarbejder
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<Vagt> { new Vagt { Id = 20, Dato = new DateTime(2025, 1, 3, 20, 0, 0), Medarbejdere = new List<Medarbejder>() } }
        };

        var dto = MedarbejderMapper.ToDto(m);

        Assert.Single(dto.Vagter);
        Assert.Equal(20, dto.Vagter[0].Id);
    }

    // ── ToEntity ──────────────────────────────────────────────

    [Fact]
    public void ToEntity_Test()
    {
        var dto = new MedarbejderDTO { Id = 5, Name = "Emma", Bestyrelsesmedlem = false, Vagter = new List<VagtDTO>() };

        var entity = MedarbejderMapper.ToEntity(dto);

        Assert.Equal(5, entity.Id);
        Assert.Equal("Emma", entity.Name);
        Assert.False(entity.Bestyrelsesmedlem);
    }

    [Fact]
    public void ToEntity_Test_Med_Vagter()
    {
        var dto = new MedarbejderDTO
        {
            Id = 1,
            Name = "Simon",
            Vagter = new List<VagtDTO> { new VagtDTO { Id = 10, Dato = DateTime.Now } }
        };

        Assert.Empty(MedarbejderMapper.ToEntity(dto).Vagter);
    }
}