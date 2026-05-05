namespace EsperantOS.Models
{
    public class Vagt
    {
        private int _id;
        private DateTime _dato;
        private bool _ædru;
        private bool _frigivet;
        private List<Medarbejder> _medarbejdere;

        public Vagt() { }

        public Vagt(int id, DateTime dato, bool ædru, bool frigivet)
        {
            this._id = id;
            this._dato = dato;
            this._ædru = ædru;
            this._frigivet = frigivet;
            this._medarbejdere = new List<Medarbejder>();
        }

        public int Id { get => _id; set => _id = value; }
        // Lukketidsvagten (00:00) gemmes teknisk som lørdag 00:00
        public DateTime Dato { get => _dato; set => _dato = value; }
        public bool Ædru { get => _ædru; set => _ædru = value; }
        public bool Frigivet { get => _frigivet; set => _frigivet = value; }
        // EF Core mange-til-mange relation – styres via implicit mellemtabel
        public List<Medarbejder> Medarbejdere { get => _medarbejdere; set => _medarbejdere = value; }
    }
}