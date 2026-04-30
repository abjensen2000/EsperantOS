using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.DataAccess.Mappers
{
    // Statisk hjælpeklasse der konverterer mellem Medarbejder-entiteten og MedarbejderDTO.
    // Bruges i datalaget så BLL og controllere aldrig ser rå databaseentiteter.
    public static class MedarbejderMapper
    {
        // Konverterer en Medarbejder-entitet (fra databasen) til en MedarbejderDTO.
        // Inkluderer medarbejderens vagter – men nulstiller vagternes medarbejderliste
        // for at undgå cirkulær reference (Medarbejder → Vagt → Medarbejder → ...).
        public static MedarbejderDTO ToDto(Medarbejder medarbejder)
        {
            return new MedarbejderDTO
            {
                Id = medarbejder.Id,
                Name = medarbejder.Name,
                Bestyrelsesmedlem = medarbejder.Bestyrelsesmedlem,
                // Konverter hver tilknyttet vagt til en VagtDTO.
                // Medarbejdere-listen sættes bevidst til tom for at bryde den cirkulære reference.
                Vagter = medarbejder.Vagter?.Select(v => new VagtDTO
                {
                    Id = v.Id,
                    Dato = v.Dato,
                    Ædru = v.Ædru,
                    Frigivet = v.Frigivet,
                    Medarbejdere = new List<MedarbejderDTO>() // Bevidst tom – undgår cirkulær reference
                }).ToList() ?? new()
            };
        }

        // Konverterer en MedarbejderDTO til en Medarbejder-entitet klar til databasen.
        // VIGTIGT: Vagter-listen mappes IKKE her – den håndteres separat af BLL,
        // da EF Core skal bruge sporede (tracked) entiteter for at opdatere relationer korrekt.
        public static Medarbejder ToEntity(MedarbejderDTO dto)
        {
            return new Medarbejder
            {
                Id = dto.Id,
                Name = dto.Name,
                Bestyrelsesmedlem = dto.Bestyrelsesmedlem,
                Vagter = new List<Vagt>() // Tom – sættes af BLL ved behov
            };
        }
    }
}
