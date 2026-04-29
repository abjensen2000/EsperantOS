using EsperantOS.DataAccess.Repositories;

namespace EsperantOS.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IVagtRepository VagtRepository { get; }
        IMedarbejderRepository MedarbejderRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
