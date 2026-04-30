using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;

namespace EsperantOS.DataAccess.UnitOfWork
{
    // Konkret implementering af IUnitOfWork.
    // Holder én fælles databaseforbindelse og opretter repositories lazy (kun når de bruges).
    public class UnitOfWork : IUnitOfWork
    {
        // Den delte databaseforbindelse – alle repositories i denne UnitOfWork bruger den samme
        private readonly EsperantOSContext _context;

        // Repositories oprettes kun én gang pr. request – null indtil første gang de bruges
        private IVagtRepository? _vagtRepository;
        private IMedarbejderRepository? _medarbejderRepository;

        // Konstruktør – modtager context via dependency injection
        public UnitOfWork(EsperantOSContext context)
        {
            _context = context;
        }

        // Lazy initialisering: opretter VagtRepository første gang det tilgås,
        // og genbruger det samme objekt ved efterfølgende kald.
        // ??= betyder: "sæt kun hvis værdien er null"
        public IVagtRepository VagtRepository
        {
            get
            {
                _vagtRepository ??= new VagtRepository(_context);
                return _vagtRepository;
            }
        }

        // Samme lazy initialisering for MedarbejderRepository
        public IMedarbejderRepository MedarbejderRepository
        {
            get
            {
                _medarbejderRepository ??= new MedarbejderRepository(_context);
                return _medarbejderRepository;
            }
        }

        // Sender alle ventende ændringer til databasen.
        // Kaldes af BLL efter Add, Update eller Delete operationer.
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Frigiver databaseforbindelsen når UnitOfWork ikke længere bruges.
        // Kaldes automatisk af ASP.NET Core's dependency injection container.
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
