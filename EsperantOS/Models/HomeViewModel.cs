namespace EsperantOS.Models
{
    // ViewModel til forsiden (Home/Index).
    // En ViewModel er et dataobjekt der er skræddersyet til én bestemt visning.
    // Den indeholder præcis de data som forsiden har brug for – ikke mere, ikke mindre.
    public class HomeViewModel
    {
        // En velkomstbesked der vises øverst på forsiden
        public string VelkomstBesked { get; set; } = "Velkommen til Esperanto!";

        // Besked fra bestyrelsen – vises i infoboksen på forsiden
        public string BestyrelsesBesked { get; set; } = "Husk at tjekke frigivede vagter. Vi mangler folk til fredagsbaren d. 25.!";

        // Liste over den indloggede brugers kommende vagter.
        // Bruges til at vise "Mine kommende vagter" på forsiden.
        public List<Vagt> MineVagter { get; set; } = new List<Vagt>();
    }
}
