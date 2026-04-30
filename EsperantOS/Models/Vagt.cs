namespace EsperantOS.Models
{
    // Repræsenterer en vagt i Esperanto-baren.
    // Alle vagter ligger på en fredag og har et af tre faste tidspunkter: 16:00, 20:00 eller 00:00.
    public class Vagt
    {
        // Private felter – gemmer de faktiske værdier
        private int _id;
        private DateTime _dato;
        private bool _ædru;
        private bool _frigivet;
        private List<Medarbejder> _medarbejdere;

        // Tom konstruktør – kræves af Entity Framework
        public Vagt() { }

        // Konstruktør med parametre – til direkte oprettelse med kendte værdier
        public Vagt(int id, DateTime dato, bool ædru, bool frigivet)
        {
            this._id = id;
            this._dato = dato;
            this._ædru = ædru;
            this._frigivet = frigivet;
            this._medarbejdere = new List<Medarbejder>(); // Ingen medarbejdere endnu
        }

        // Unik ID for vagten – sættes automatisk af databasen
        public int Id { get => _id; set => _id = value; }

        // Dato og tidspunkt for vagten.
        // Skal altid være en fredag. Lukketidsvagten (00:00) gemmes teknisk som lørdag 00:00.
        public DateTime Dato { get => _dato; set => _dato = value; }

        // Angiver om vagten kræver en ædru medarbejder.
        // Lukketidsvagten (00:00) har altid Ædru = true.
        public bool Ædru { get => _ædru; set => _ædru = value; }

        // Angiver om vagten er frigivet, dvs. at den er ledig og kan tages af en anden.
        // En frigivet vagt vises i "Frigivede vagter"-visningen.
        public bool Frigivet { get => _frigivet; set => _frigivet = value; }

        // Liste over de medarbejdere der er sat på vagten.
        // Entity Framework styrer denne mange-til-mange relation via en mellemtabel i databasen.
        public List<Medarbejder> Medarbejdere { get => _medarbejdere; set => _medarbejdere = value; }
    }
}
