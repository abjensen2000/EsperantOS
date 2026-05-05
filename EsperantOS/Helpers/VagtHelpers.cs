using Microsoft.AspNetCore.Mvc.Rendering;

namespace EsperantOS.Helpers
{
    public static class VagtHelpers
    {
        public static List<SelectListItem> GetUpcomingFridaysSelectList(DateTime? selectedDate = null)
        {
            var upcomingFridays = new List<SelectListItem>();
            var date = DateTime.Today;

            while (date.DayOfWeek != DayOfWeek.Friday)
                date = date.AddDays(1);

            for (int i = 0; i < 10; i++)
            {
                // Lukketidsvagten (00:00) gemmes som lørdag
                bool isSelected = selectedDate.HasValue &&
                    (selectedDate.Value.Date == date.Date ||
                     (selectedDate.Value.Hour == 0 && selectedDate.Value.Date == date.AddDays(1).Date));

                upcomingFridays.Add(new SelectListItem
                {
                    Text = date.ToString("dd. MMMM yyyy"),
                    Value = date.ToString("yyyy-MM-dd"),
                    Selected = isSelected
                });

                date = date.AddDays(7);
            }

            return upcomingFridays;
        }

        public static List<SelectListItem> GetShiftTimesSelectList(DateTime? selectedDate = null)
        {
            string currentTimeVal = selectedDate?.Hour switch
            {
                16 => "16:00:00",
                20 => "20:00:00",
                0  => "00:00:00",
                _  => "16:00:00"
            };

            return new List<SelectListItem>
            {
                new SelectListItem { Text = "16:00 - 20:00", Value = "16:00:00", Selected = currentTimeVal == "16:00:00" },
                new SelectListItem { Text = "20:00 - 00:00", Value = "20:00:00", Selected = currentTimeVal == "20:00:00" },
                new SelectListItem { Text = "00:00 - 02:00", Value = "00:00:00", Selected = currentTimeVal == "00:00:00" }
            };
        }

        public static DateTime GetShiftDateTime(DateTime selectedDate, string selectedTime)
        {
            var timeSpan = TimeSpan.Parse(selectedTime);
            var date = selectedDate.Date.Add(timeSpan);

            // 00:00-vagten hører til natten efter fredag – gem som lørdag
            if (timeSpan.Hours == 0)
                date = date.AddDays(1);

            return date;
        }
    }
}
