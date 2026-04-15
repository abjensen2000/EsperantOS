using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.Data
{
    public class EsperantOSContext : DbContext
    {
        public EsperantOSContext(DbContextOptions<EsperantOSContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Vagt> Vagter{ get; set; } = null!;

        public DbSet<Medarbejder> Medarbejdere { get; set; } = null!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-7O1GS44\\SQLEXPRESS;Initial Catalog=EsperantOS;Integrated Security=True;Trust Server Certificate=True");
        }
    }
}
