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
            // Guard lets tests inject InMemory/SQLite :memory: without being overridden
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite("Data Source=EsperantOS.db");
        }
    }
}
