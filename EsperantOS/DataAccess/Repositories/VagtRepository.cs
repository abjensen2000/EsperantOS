using EsperantOS.DataAccess.Context;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    public interface IVagtRepository : IRepository<Vagt>
    {
        Task<List<Vagt>> GetVagterByDayOfWeekAsync(DayOfWeek dayOfWeek);
        Task<List<Vagt>> GetFridayVagterAsync();
        Task<List<Vagt>> GetVagterByMedarbejderAsync(int medarbejderId);
        Task<Vagt?> GetVagtWithMedarbejdereAsync(int id);
        Task<List<Vagt>> GetAllVagterWithMedarbejdereAsync();
    }

    public class VagtRepository : Repository<Vagt>, IVagtRepository
    {
        public VagtRepository(EsperantOSContext context) : base(context) { }

        // DateTime.DayOfWeek kan ikke oversættes til SQL – vi henter alt og filtrerer i C#
        public async Task<List<Vagt>> GetVagterByDayOfWeekAsync(DayOfWeek dayOfWeek)
        {
            var vagter = await _dbSet
                .Include(v => v.Medarbejdere)
                .ToListAsync();

            return vagter.Where(v => v.Dato.DayOfWeek == dayOfWeek).ToList();
        }

        public async Task<List<Vagt>> GetFridayVagterAsync()
        {
            return await GetVagterByDayOfWeekAsync(DayOfWeek.Friday);
        }

        public async Task<List<Vagt>> GetVagterByMedarbejderAsync(int medarbejderId)
        {
            return await _dbSet
                .Where(v => v.Medarbejdere.Any(m => m.Id == medarbejderId))
                .Include(v => v.Medarbejdere)
                .OrderBy(v => v.Dato)
                .ToListAsync();
        }

        public async Task<Vagt?> GetVagtWithMedarbejdereAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Medarbejdere)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<Vagt>> GetAllVagterWithMedarbejdereAsync()
        {
            return await _dbSet
                .Include(v => v.Medarbejdere)
                .ToListAsync();
        }
    }
}
