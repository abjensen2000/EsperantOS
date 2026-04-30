namespace EsperantOS.DTO.Model
{
    // DTO = Data Transfer Object.
    // MedarbejderDTO bruges til at flytte medarbejderdata mellem datalaget og forretningslogikken.
    // Adskiller databaseentiteten (Medarbejder) fra det data som resten af systemet arbejder med.
    public class MedarbejderDTO
    {
        // Medarbejderens unikke ID fra databasen
        public int Id { get; set; }

        // Medarbejderens navn – bruges også som brugernavn ved login
        public string Name { get; set; } = string.Empty;

        // Om medarbejderen er bestyrelsesmedlem (admin)
        public bool Bestyrelsesmedlem { get; set; }

        // Liste over de vagter som medarbejderen er tilknyttet.
        // Initialiseres som tom liste så vi aldrig får null-fejl.
        public List<VagtDTO> Vagter { get; set; } = new();
    }
}
