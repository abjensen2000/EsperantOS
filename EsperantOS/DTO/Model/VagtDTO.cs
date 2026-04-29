namespace EsperantOS.DTO.Model
{
    public class VagtDTO
    {
        public int Id { get; set; }
        public DateTime Dato { get; set; }
        public bool Ædru { get; set; }
        public bool Frigivet { get; set; }
        public List<MedarbejderDTO> Medarbejdere { get; set; } = new();
    }
}
