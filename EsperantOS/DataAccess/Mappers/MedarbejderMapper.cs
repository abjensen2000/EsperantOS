using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.DataAccess.Mappers
{
    public static class MedarbejderMapper
    {
        public static MedarbejderDTO ToDto(Medarbejder medarbejder)
        {
            return new MedarbejderDTO
            {
                Id = medarbejder.Id,
                Name = medarbejder.Name,
                Bestyrelsesmedlem = medarbejder.Bestyrelsesmedlem,
                Vagter = medarbejder.Vagter?.Select(v => new VagtDTO
                {
                    Id = v.Id,
                    Dato = v.Dato,
                    Ædru = v.Ædru,
                    Frigivet = v.Frigivet,
                    Medarbejdere = new List<MedarbejderDTO>()
                }).ToList() ?? new()
            };
        }

        public static Medarbejder ToEntity(MedarbejderDTO dto)
        {
            return new Medarbejder
            {
                Id = dto.Id,
                Name = dto.Name,
                Bestyrelsesmedlem = dto.Bestyrelsesmedlem,
                Vagter = new List<Vagt>()
            };
        }
    }
}
