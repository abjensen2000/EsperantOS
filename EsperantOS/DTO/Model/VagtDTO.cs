namespace EsperantOS.DTO.Model
{
    // DTO = Data Transfer Object.
    // VagtDTO bruges til at flytte vagtdata mellem datalaget (repositories) og forretningslogikken (BLL).
    // Controllerne modtager altid en DTO fra BLL – aldrig direkte en databaseentitet.
    // Det betyder at databasestrukturen er skjult fra resten af applikationen.
    public class VagtDTO
    {
        // Vagtens unikke ID fra databasen
        public int Id { get; set; }

        // Dato og tidspunkt for vagten
        public DateTime Dato { get; set; }

        // Om vagten kræver en ædru medarbejder
        public bool Ædru { get; set; }

        // Om vagten er frigivet (ledig til at blive taget)
        public bool Frigivet { get; set; }

        // Liste over de medarbejdere der er tilknyttet vagten.
        // Initialiseres som tom liste så vi aldrig får null-fejl.
        public List<MedarbejderDTO> Medarbejdere { get; set; } = new();
    }
}
