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
            get
            {
                if (_vagtRepository == null)
                {
                    _vagtRepository = new VagtRepository(_context);
                }

                return _vagtRepository;
            }
        }

        public IMedarbejderRepository MedarbejderRepository
        {
            get
            {
                if (_medarbejderRepository == null)
                {
                    _medarbejderRepository = new MedarbejderRepository(_context);
                }

                return _medarbejderRepository;
            }
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
