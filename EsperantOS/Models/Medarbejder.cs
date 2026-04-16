namespace EsperantOS.Models
{
    public class Medarbejder
    {
        private int _id;
        private string _name;
        private bool bestyrelsesmedlem;
        private List<Vagt> vagter = [];

        public Medarbejder(int id, string name, bool bestyrelsesmedlem)
        {
            _id = id;
            _name = name;
            this.bestyrelsesmedlem = bestyrelsesmedlem;
        }
        public Medarbejder() { }

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public bool Bestyrelsesmedlem { get => bestyrelsesmedlem; set => bestyrelsesmedlem = value; }
        public List<Vagt> Vagter { get => vagter; set => vagter = value; }
    }
}
