namespace EsperantOS.Models
{
    // Repræsenterer en medarbejder (frivillig) i Esperanto-baren.
    // Denne klasse bruges af Entity Framework til at oprette og læse rækker i databasen.
    public class Medarbejder
    {
        // Private felter – de gemmer den faktiske værdi bag egenskaberne nedenfor
        private int _id;
        private string _name;
        private bool bestyrelsesmedlem;
        private List<Vagt> vagter;

        // Tom konstruktør – kræves af Entity Framework for at kunne oprette objektet automatisk
        public Medarbejder() { }

        // Konstruktør med parametre – bruges når man selv opretter en medarbejder med kendte værdier
        public Medarbejder(int id, string name, bool bestyrelsesmedlem)
        {
            _id = id;
            _name = name;
            this.bestyrelsesmedlem = bestyrelsesmedlem;
            vagter = new List<Vagt>(); // Tom liste – vagter tilføjes bagefter
        }

        // Unik ID for medarbejderen – sættes automatisk af databasen
        public int Id { get => _id; set => _id = value; }

        // Medarbejderens navn – bruges også som brugernavn ved login
        public string Name { get => _name; set => _name = value; }

        // Angiver om medarbejderen er bestyrelsesmedlem (dvs. har admin-rettigheder)
        public bool Bestyrelsesmedlem { get => bestyrelsesmedlem; set => bestyrelsesmedlem = value; }

        // Liste over de vagter som medarbejderen er tilknyttet.
        // Entity Framework styrer denne mange-til-mange relation automatisk via en mellemtabel.
        public List<Vagt> Vagter { get => vagter; set => vagter = value; }
    }
}
