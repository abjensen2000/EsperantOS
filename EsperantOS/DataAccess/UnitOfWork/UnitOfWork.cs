using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.Repositories;

namespace EsperantOS.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EsperantOSContext _context;
        private IVagtRepository? _vagtRepository;
        private IMedarbejderRepository? _medarbejderRepository;

        public UnitOfWork(EsperantOSContext context)
        {
            _context = context;
        }

        // ??= opretter kun repository-instansen første gang den tilgås (lazy initialisering)
        public IVagtRepository VagtRepository
        {
            get { return _vagtRepository ??= new VagtRepository(_context); }
        }

        public IMedarbejderRepository MedarbejderRepository
        {
            get { return _medarbejderRepository ??= new MedarbejderRepository(_context); }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
