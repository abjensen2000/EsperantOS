using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.DataAccess.Mappers
{
    public static class VagtMapper
    {
        public static VagtDTO ToDto(Vagt vagt)
        {
            return new VagtDTO
            {
                Id = vagt.Id,
                Dato = vagt.Dato,
                Ædru = vagt.Ædru,
                Frigivet = vagt.Frigivet,
                Medarbejdere = vagt.Medarbejdere?.Select(MedarbejderMapper.ToDto).ToList() ?? new()
            };
        }

        public static Vagt ToEntity(VagtDTO dto)
        {
            return new Vagt
            {
                Id = dto.Id,
                Dato = dto.Dato,
                Ædru = dto.Ædru,
                Frigivet = dto.Frigivet,
                Medarbejdere = new List<Medarbejder>()
            };
        }
    }
}
