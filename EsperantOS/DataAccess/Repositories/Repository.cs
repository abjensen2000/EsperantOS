using EsperantOS.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    // Generisk implementering af IRepository<T>.
    // Denne klasse håndterer de basale databaseoperationer for alle entitetstyper.
    // VagtRepository og MedarbejderRepository arver herfra og tilføjer specialiserede metoder ovenpå.
    public class Repository<T> : IRepository<T> where T : class
    {
        // EF Core databaseforbindelsen – deles på tværs af alle repositories i samme request
        protected readonly EsperantOSContext _context;

        // Den specifikke tabel vi arbejder med (f.eks. Vagter eller Medarbejdere)
        protected readonly DbSet<T> _dbSet;

        // Konstruktør – modtager context via dependency injection
        public Repository(EsperantOSContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // Henter den rigtige tabel baseret på typen T
        }

        // Henter alle rækker fra tabellen som en liste
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Henter én række ud fra dens ID.
        // FindAsync er effektiv – tjekker først i hukommelsen før den spørger databasen.
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Tilføjer en ny entitet til EF Core's sporingsmekanisme (change tracker).
        // Data gemmes ikke i databasen før UnitOfWork.SaveChangesAsync() kaldes.
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Markerer entiteten som ændret i EF Core's change tracker.
        // EF Core finder selv ud af hvad der er ændret og genererer den rigtige SQL UPDATE.
        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask; // Ingen asynkron handling nødvendig her
        }

        // Sletter en entitet fra databasen ud fra dens ID.
        // Henter først entiteten, og fjerner den derefter fra tabellen.
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
    }
}
