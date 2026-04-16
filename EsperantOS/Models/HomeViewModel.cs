namespace EsperantOS.Models
{
    public class HomeViewModel
    {
        public string VelkomstBesked { get; set; } = "Velkommen til Esperanto!";
        public string BestyrelsesBesked { get; set; } = "Husk at tjekke frigivede vagter. Vi mangler folk til fredagsbaren d. 25.!";
        public List<Vagt> MineVagter { get; set; } = new List<Vagt>();
    }
}
