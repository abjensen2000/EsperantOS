using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.DataAccess.Mappers
{
    // Statisk hjælpeklasse der konverterer mellem Vagt-entiteten og VagtDTO.
    // Bruges i datalaget lige efter data er hentet fra databasen,
    // så BLL og controllere aldrig arbejder direkte med databaseentiteter.
    public static class VagtMapper
    {
        // Konverterer en Vagt-entitet (fra databasen) til en VagtDTO.
        // Inkluderer de tilknyttede medarbejdere – men nulstiller deres vagter-liste
        // for at undgå uendelig cirkulær reference (Vagt → Medarbejder → Vagt → ...).
        public static VagtDTO ToDto(Vagt vagt)
        {
            return new VagtDTO
            {
                Id = vagt.Id,
                Dato = vagt.Dato,
                Ædru = vagt.Ædru,
                Frigivet = vagt.Frigivet,
                // Konverter hver tilknyttet medarbejder til en MedarbejderDTO.
                // Vagter-listen sættes bevidst til tom for at bryde den cirkulære reference.
                Medarbejdere = vagt.Medarbejdere?.Select(m => new MedarbejderDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Bestyrelsesmedlem = m.Bestyrelsesmedlem,
                    Vagter = new List<VagtDTO>() // Bevidst tom – undgår cirkulær reference
                }).ToList() ?? new()
            };
        }

        // Konverterer en VagtDTO til en Vagt-entitet klar til indsættelse i databasen.
        // VIGTIGT: Medarbejdere-listen mappes IKKE her – den håndteres separat i BLL
        // ved opdatering, fordi EF Core skal arbejde med sporede (tracked) entiteter.
        public static Vagt ToEntity(VagtDTO dto)
        {
            return new Vagt
            {
                Id = dto.Id,
                Dato = dto.Dato,
                Ædru = dto.Ædru,
                Frigivet = dto.Frigivet,
                Medarbejdere = new List<Medarbejder>() // Tom – sættes af BLL ved behov
            };
        }
    }
}
