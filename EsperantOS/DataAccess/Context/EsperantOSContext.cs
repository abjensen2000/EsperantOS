using EsperantOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Context
{
    // EF Core databasekontekst – det centrale forbindelsespunkt til databasen.
    // DbContext holder styr på alle entiteter, sporer ændringer og genererer SQL-forespørgsler.
    public class EsperantOSContext : DbContext
    {
        // Konstruktør – modtager konfiguration via dependency injection fra Program.cs
        public EsperantOSContext(DbContextOptions<EsperantOSContext> options) : base(options)
        {
            // Sørger for at databasen og dens tabeller eksisterer.
            // Opretter dem automatisk hvis de ikke allerede er der.
            // OBS: Program.cs kalder også EnsureDeleted + EnsureCreated ved opstart
            // for at nulstille databasen og genindlæse testdata.
            Database.EnsureCreated();
        }

        // Repræsenterer tabellen "Vagter" i databasen.
        // Via denne egenskab kan vi forespørge, tilføje og slette vagter.
        public DbSet<Vagt> Vagter { get; set; } = null!;

        // Repræsenterer tabellen "Medarbejdere" i databasen.
        public DbSet<Medarbejder> Medarbejdere { get; set; } = null!;

        // Konfigurerer databaseforbindelsen.
        // Bruges af EF Core når der ikke er sendt nogen konfiguration via konstruktøren.
        // Her bruges SQLite med en lokal fil (EsperantOS.db) som fungerer på alle platforme.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EsperantOS.db");
        }
    }
}
