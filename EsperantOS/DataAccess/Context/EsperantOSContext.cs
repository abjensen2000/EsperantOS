using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Context
{
    public class EsperantOSContext : DbContext
    {
        public EsperantOSContext(DbContextOptions<EsperantOSContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Vagt> Vagter { get; set; } = null!;
        public DbSet<Medarbejder> Medarbejdere { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // SQLite bruges fremfor LocalDB så applikationen virker på alle platforme
            optionsBuilder.UseSqlite("Data Source=EsperantOS.db");
        }
    }
}
