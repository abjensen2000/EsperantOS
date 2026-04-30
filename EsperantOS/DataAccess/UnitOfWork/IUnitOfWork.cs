using EsperantOS.DataAccess.Repositories;

namespace EsperantOS.DataAccess.UnitOfWork
{
    // Unit of Work-mønsteret samler alle repositories under én fælles databaseforbindelse.
    // Det sikrer at alle ændringer i en enkelt request enten gemmes samlet eller rulles tilbage samlet.
    //
    // BLL-klasserne bruger dette interface i stedet for at kalde repositories direkte.
    // IDisposable bruges til at frigive databaseforbindelsen når den ikke længere er nødvendig.
    public interface IUnitOfWork : IDisposable
    {
        // Adgang til vagtrepositoriet – bruges til at hente og ændre vagter
        IVagtRepository VagtRepository { get; }

        // Adgang til medarbejderrepositoriet – bruges til at hente og ændre medarbejdere
        IMedarbejderRepository MedarbejderRepository { get; }

        // Gemmer alle ventende ændringer til databasen i én samlet transaktion.
        // Returnerer antal rækker der blev påvirket.
        Task<int> SaveChangesAsync();
    }
}
