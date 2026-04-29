using EsperantOS.Data;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    public interface IMedarbejderRepository : IRepository<Medarbejder>
    {
        Task<Medarbejder?> GetMedarbejderByNameAsync(string name);
        Task<List<Medarbejder>> GetBestyrelsesmedlemmerAsync();
        Task<Medarbejder?> GetMedarbejderWithVagterAsync(int id);
    }

    public class MedarbejderRepository : Repository<Medarbejder>, IMedarbejderRepository
    {
        public MedarbejderRepository(EsperantOSContext context) : base(context)
        {
        }

        public async Task<Medarbejder?> GetMedarbejderByNameAsync(string name)
        {
            return await _dbSet
                .Include(m => m.Vagter)
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<List<Medarbejder>> GetBestyrelsesmedlemmerAsync()
        {
            return await _dbSet
                .Where(m => m.Bestyrelsesmedlem)
                .Include(m => m.Vagter)
                .ToListAsync();
        }

        public async Task<Medarbejder?> GetMedarbejderWithVagterAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Vagter)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
