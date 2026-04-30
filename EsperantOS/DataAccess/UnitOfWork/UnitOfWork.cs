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

        public IVagtRepository VagtRepository
        {
            get
            {
                _vagtRepository ??= new VagtRepository(_context);
                return _vagtRepository;
            }
        }

        public IMedarbejderRepository MedarbejderRepository
        {
            get
            {
                _medarbejderRepository ??= new MedarbejderRepository(_context);
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
