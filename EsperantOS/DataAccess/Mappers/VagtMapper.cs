using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.DataAccess.Mappers
{
    public static class VagtMapper
    {
        public static VagtDTO ToDto(Vagt vagt)
        {
            List<MedarbejderDTO> medarbejdere;

            if (vagt.Medarbejdere != null)
            {
                medarbejdere = vagt.Medarbejdere.Select(m => new MedarbejderDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Bestyrelsesmedlem = m.Bestyrelsesmedlem,
                    Vagter = new List<VagtDTO>()
                }).ToList();
            }
            else
            {
                medarbejdere = new List<MedarbejderDTO>();
            }

            return new VagtDTO
            {
                Id = vagt.Id,
                Dato = vagt.Dato,
                Ædru = vagt.Ædru,
                Frigivet = vagt.Frigivet,
                Medarbejdere = medarbejdere
            };
        }

        // Medarbejdere mappes IKKE her – håndteres manuelt i BLL med sporede entiteter
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
