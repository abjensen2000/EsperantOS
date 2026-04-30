using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Extensions
{
    // Udvidelsesmetoder (extension methods) der konverterer DTO'er til Model-objekter.
    // Controllerne bruger disse metoder til at konvertere BLL-resultater til
    // de Model-typer som Razor-viewene arbejder med.
    //
    // Extension methods bruges ved at skrive dto.ToModel() direkte på objektet –
    // det er mere læsevenligt end at kalde en statisk konverteringsklasse.
    public static class DtoModelExtensions
    {
        // Konverterer en VagtDTO til en Vagt-model som viewet kan vise.
        // Inkluderer tilknyttede medarbejdere ved at konvertere dem rekursivt.
        public static Vagt ToModel(this VagtDTO dto)
        {
            return new Vagt
            {
                Id = dto.Id,
                Dato = dto.Dato,
                Ædru = dto.Ædru,
                Frigivet = dto.Frigivet,
                // Konverter hver MedarbejderDTO til en Medarbejder-model.
                // Null-tjek via ?? sikrer at vi altid returnerer en liste, aldrig null.
                Medarbejdere = dto.Medarbejdere?.Select(m => m.ToModel()).ToList() ?? new List<Medarbejder>()
            };
        }

        // Konverterer en hel liste af VagtDTO'er til en liste af Vagt-modeller.
        // Praktisk hjælper der undgår at kalde ToModel() manuelt i en løkke.
        public static List<Vagt> ToModelList(this List<VagtDTO> dtos)
        {
            return dtos.Select(dto => dto.ToModel()).ToList();
        }

        // Konverterer en MedarbejderDTO til en Medarbejder-model.
        // Vagter inkluderes som enkle Vagt-objekter uden at rekursivt inkludere deres medarbejdere
        // (det ville skabe en cirkulær reference).
        public static Medarbejder ToModel(this MedarbejderDTO dto)
        {
            return new Medarbejder
            {
                Id = dto.Id,
                Name = dto.Name,
                Bestyrelsesmedlem = dto.Bestyrelsesmedlem,
                // Konverter vagtlisten uden medarbejdere for at undgå cirkulær reference
                Vagter = dto.Vagter?.Select(v => new Vagt
                {
                    Id = v.Id,
                    Dato = v.Dato,
                    Ædru = v.Ædru,
                    Frigivet = v.Frigivet
                    // Medarbejdere udelades bevidst her
                }).ToList() ?? new List<Vagt>()
            };
        }

        // Konverterer en hel liste af MedarbejderDTO'er til Medarbejder-modeller
        public static List<Medarbejder> ToModelList(this List<MedarbejderDTO> dtos)
        {
            return dtos.Select(dto => dto.ToModel()).ToList();
        }
    }
}
