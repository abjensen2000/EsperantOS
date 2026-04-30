using EsperantOS.DataAccess.Context;
using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    // Interface der udvider det generiske IRepository<Medarbejder> med medarbejderspecifikke forespørgsler.
    public interface IMedarbejderRepository : IRepository<Medarbejder>
    {
        // Henter en medarbejder ud fra navn (bruges ved login og vagtopslag)
        Task<Medarbejder?> GetMedarbejderByNameAsync(string name);

        // Henter alle bestyrelsesmedlemmer (bruges til admin-funktioner)
        Task<List<Medarbejder>> GetBestyrelsesmedlemmerAsync();

        // Henter én medarbejder og inkluderer deres vagter
        Task<Medarbejder?> GetMedarbejderWithVagterAsync(int id);
    }

    // Konkret implementering af IMedarbejderRepository.
    // Arver basale CRUD-operationer fra Repository<Medarbejder>.
    public class MedarbejderRepository : Repository<Medarbejder>, IMedarbejderRepository
    {
        public MedarbejderRepository(EsperantOSContext context) : base(context)
        {
        }

        // Henter en medarbejder ud fra navn.
        // Sammenligningen er gjort case-insensitiv med ToLower() på begge sider,
        // så "simon", "Simon" og "SIMON" alle matcher den samme medarbejder.
        public async Task<Medarbejder?> GetMedarbejderByNameAsync(string name)
        {
            return await _dbSet
                .Include(m => m.Vagter)
                .FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower());
        }

        // Henter alle medarbejdere der er markeret som bestyrelsesmedlemmer
        public async Task<List<Medarbejder>> GetBestyrelsesmedlemmerAsync()
        {
            return await _dbSet
                .Where(m => m.Bestyrelsesmedlem)
                .Include(m => m.Vagter)
                .ToListAsync();
        }

        // Henter én medarbejder med alle tilhørende vagter.
        // Bruges når vi har brug for at vise en medarbejders fulde vagthistorik.
        public async Task<Medarbejder?> GetMedarbejderWithVagterAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Vagter)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
