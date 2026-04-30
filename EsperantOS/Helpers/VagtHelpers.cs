using Microsoft.AspNetCore.Mvc.Rendering;

namespace EsperantOS.Helpers
{
    // Statisk hjælpeklasse med hjælpemetoder til vagtrelaterede dropdowns og dato/tid-beregninger.
    // Bruges i VagtsController til at bygge formularer til oprettelse og redigering af vagter.
    public static class VagtHelpers
    {
        // Genererer en liste af de næste 10 fredage til brug i en dropdown-liste.
        // selectedDate bruges til at forudvælge den rigtige dato i redigeringsformularen.
        public static List<SelectListItem> GetUpcomingFridaysSelectList(DateTime? selectedDate = null)
        {
            var upcomingFridays = new List<SelectListItem>();
            var date = DateTime.Today;

            // Find den næste fredag startende fra i dag
            while (date.DayOfWeek != DayOfWeek.Friday)
                date = date.AddDays(1);

            // Tilføj de næste 10 fredage til listen
            for (int i = 0; i < 10; i++)
            {
                // Tjek om denne dato matcher den allerede valgte dato.
                // Specialtilfælde: lukketidsvagten (00:00) gemmes som lørdag – det er fredag + 1 dag.
                bool isSelected = selectedDate.HasValue &&
                    (selectedDate.Value.Date == date.Date ||
                     (selectedDate.Value.Hour == 0 && selectedDate.Value.Date == date.AddDays(1).Date));

                upcomingFridays.Add(new SelectListItem
                {
                    Text = date.ToString("dd. MMMM yyyy"), // Vis-tekst i dropdown
                    Value = date.ToString("yyyy-MM-dd"),   // Værdi der sendes med formularen
                    Selected = isSelected
                });

                date = date.AddDays(7); // Spring til næste fredag
            }

            return upcomingFridays;
        }

        // Genererer en liste af de tre faste vagttidspunkter til brug i en dropdown-liste.
        // selectedDate bruges til at forudvælge det rigtige tidspunkt i redigeringsformularen.
        public static List<SelectListItem> GetShiftTimesSelectList(DateTime? selectedDate = null)
        {
            // Find det aktuelle tidspunkt baseret på vagtens dato
            string currentTimeVal = selectedDate?.Hour switch
            {
                16 => "16:00:00",  // Åbningsvagt
                20 => "20:00:00",  // Aftenvagt
                0  => "00:00:00",  // Lukketidsvagt
                _  => "16:00:00"   // Standard: åbningsvagt
            };

            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "16:00 - 20:00",  // Åbningsvagt
                    Value = "16:00:00",
                    Selected = currentTimeVal == "16:00:00"
                },
                new SelectListItem
                {
                    Text = "20:00 - 00:00",  // Aftenvagt
                    Value = "20:00:00",
                    Selected = currentTimeVal == "20:00:00"
                },
                new SelectListItem
                {
                    Text = "00:00 - 02:00",  // Lukketidsvagt
                    Value = "00:00:00",
                    Selected = currentTimeVal == "00:00:00"
                }
            };
        }

        // Kombinerer en dato og et tidspunkt (som tekst) til én samlet DateTime.
        // Specialtilfælde: lukketidsvagten er sat til 00:00, men hører til fredagens bar-aften –
        // den gemmes derfor som lørdag 00:00 (datoen + 1 dag) for at undgå forvirring.
        public static DateTime GetShiftDateTime(DateTime selectedDate, string selectedTime)
        {
            var timeSpan = TimeSpan.Parse(selectedTime); // Fortolk tidspunktet f.eks. "20:00:00"
            var date = selectedDate.Date.Add(timeSpan);  // Kombiner dato og tidspunkt

            // Tilføj én dag for lukketidsvagten (00:00 hører til natten efter fredag)
            if (timeSpan.Hours == 0)
                date = date.AddDays(1);

            return date;
        }
    }
}
