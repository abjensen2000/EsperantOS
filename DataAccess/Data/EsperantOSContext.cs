using DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using DataAccess.Utilities;

namespace EsperantOS.Data
{
    internal class EsperantOSContext : DbContext
    {

        public EsperantOSContext(DbContextOptions<EsperantOSContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public EsperantOSContext()
        {
            Database.EnsureCreated();
        }


        public DbSet<Vagt> Vagter { get; set; } = null!;

        public DbSet<Medarbejder> Medarbejdere { get; set; } = null!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed testdata with hashed passwords using PBKDF2
            // Password: "password123"
            var passwordHash = PasswordHelper.HashPassword("password123");

            modelBuilder.Entity<Medarbejder>().HasData(
                new Medarbejder(1, "Christian", true, passwordHash),
                new Medarbejder(2, "Anders", false, passwordHash)
            );

            modelBuilder.Entity<Vagt>().HasData(
                new Vagt(1, DateTime.Now, false, true));
        }
    }
}
