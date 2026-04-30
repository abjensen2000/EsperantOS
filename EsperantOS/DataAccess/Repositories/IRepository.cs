namespace EsperantOS.DataAccess.Repositories
{
    // Generisk interface der definerer de grundlæggende databaseoperationer (CRUD).
    // T er en typeparameter – det betyder at samme interface kan bruges til både Vagt og Medarbejder.
    // "where T : class" betyder at T skal være en klasse (ikke f.eks. int eller bool).
    //
    // Ved at bruge et interface kan vi udskifte selve implementationen uden at ændre resten af koden.
    public interface IRepository<T> where T : class
    {
        // Henter alle rækker af typen T fra databasen
        Task<IEnumerable<T>> GetAllAsync();

        // Henter én enkelt række ud fra dens ID – returnerer null hvis den ikke findes
        Task<T?> GetByIdAsync(int id);

        // Tilføjer en ny række til databasen (gemmes ikke før SaveChanges kaldes)
        Task AddAsync(T entity);

        // Markerer en eksisterende række som ændret (gemmes ikke før SaveChanges kaldes)
        Task UpdateAsync(T entity);

        // Sletter en række ud fra dens ID (gemmes ikke før SaveChanges kaldes)
        Task DeleteAsync(int id);
    }
}
