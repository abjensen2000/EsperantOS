namespace EsperantOS.DTO.Model
{
    public class MedarbejderDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Bestyrelsesmedlem { get; set; }
        public List<VagtDTO> Vagter { get; set; } = new();
    }
}
