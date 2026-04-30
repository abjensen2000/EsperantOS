namespace EsperantOS.Models
{
    // ViewModel til fejlsiden.
    // Bruges til at vise information om en fejl der er opstået under behandling af en forespørgsel.
    public class ErrorViewModel
    {
        // ID på den HTTP-forespørgsel der fejlede – bruges til fejlsøgning i logfiler
        public string? RequestId { get; set; }

        // Beregnet egenskab: returnerer true hvis RequestId ikke er tom.
        // Bruges i viewet til at afgøre om RequestId skal vises.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
