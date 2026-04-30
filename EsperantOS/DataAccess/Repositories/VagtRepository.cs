using EsperantOS.DataAccess.Context;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    // Interface der udvider det generiske IRepository<Vagt> med vagtspecifikke forespørgsler.
    // BLL bruger dette interface – ikke den konkrete klasse – så implementationen kan udskiftes.
    public interface IVagtRepository : IRepository<Vagt>
    {
        // Henter alle vagter for en bestemt ugedag (f.eks. fredag)
        Task<List<Vagt>> GetVagterByDayOfWeekAsync(DayOfWeek dayOfWeek);

        // Henter alle fredagsvagter (specialiseret udgave af ovenstående)
        Task<List<Vagt>> GetFridayVagterAsync();

        // Henter alle vagter som en bestemt medarbejder er tilknyttet
        Task<List<Vagt>> GetVagterByMedarbejderAsync(int medarbejderId);

        // Henter én vagt og inkluderer dens medarbejdere (nødvendigt for mange-til-mange relationen)
        Task<Vagt?> GetVagtWithMedarbejdereAsync(int id);

        // Henter alle vagter og inkluderer deres medarbejdere
        Task<List<Vagt>> GetAllVagterWithMedarbejdereAsync();
    }

    // Konkret implementering af IVagtRepository.
    // Arver de basale CRUD-operationer fra Repository<Vagt> og tilføjer vagtspecifikke metoder.
    public class VagtRepository : Repository<Vagt>, IVagtRepository
    {
        public VagtRepository(EsperantOSContext context) : base(context)
        {
        }

        // Henter vagter filtreret på ugedag.
        // BEMÆRK: DateTime.DayOfWeek kan ikke oversættes til SQL af EF Core,
        // så vi henter ALLE vagter fra databasen og filtrerer bagefter i C#.
        // Dette er en bevidst afvejning – antallet af vagter er lille nok til at det er acceptabelt.
        public async Task<List<Vagt>> GetVagterByDayOfWeekAsync(DayOfWeek dayOfWeek)
        {
            // Hent alle vagter inklusiv deres medarbejdere fra databasen
            var vagter = await _dbSet
                .Include(v => v.Medarbejdere)
                .ToListAsync();

            // Filtrer i C# på ugedag (kan ikke gøres i SQL via EF Core)
            return vagter
                .Where(v => v.Dato.DayOfWeek == dayOfWeek)
                .ToList();
        }

        // Henter alle fredagsvagter – bruger ovenstående metode med fredag som parameter
        public async Task<List<Vagt>> GetFridayVagterAsync()
        {
            return await GetVagterByDayOfWeekAsync(DayOfWeek.Friday);
        }

        // Henter alle vagter som en bestemt medarbejder (givet ved ID) er sat på.
        // Any() tjekker om medarbejder-listen indeholder en medarbejder med det givne ID.
        public async Task<List<Vagt>> GetVagterByMedarbejderAsync(int medarbejderId)
        {
            return await _dbSet
                .Where(v => v.Medarbejdere.Any(m => m.Id == medarbejderId))
                .Include(v => v.Medarbejdere) // Inkluder medarbejderdata i resultatet
                .OrderBy(v => v.Dato)          // Sorter kronologisk
                .ToListAsync();
        }

        // Henter én bestemt vagt og inkluderer dens medarbejdere.
        // Bruges når vi skal vise eller redigere en vagt – vi har brug for medarbejderdata.
        public async Task<Vagt?> GetVagtWithMedarbejdereAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Medarbejdere)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        // Henter alle vagter med tilhørende medarbejdere.
        // Bruges til at vise den samlede vagtplan.
        public async Task<List<Vagt>> GetAllVagterWithMedarbejdereAsync()
        {
            return await _dbSet
                .Include(v => v.Medarbejdere)
                .ToListAsync();
        }
    }
}
