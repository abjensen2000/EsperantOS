using DataAccess.Mappers;
using DTO.Model;
using EsperantOS.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly EsperantOSContext _context = new EsperantOSContext();
        private bool _disposed = false; // track if already disposed


        private MedarbejderRepository _medarbejdere;
        private VagtRepository _vagter;

        public UnitOfWork()
        {
            _medarbejdere = new MedarbejderRepository(_context);
            _vagter = new VagtRepository(_context);
        }

        public MedarbejderDTO GetMedarbejder(int medarbejderId)
        {
            return _medarbejdere.GetMedarbejder(medarbejderId);
        }

        public List<MedarbejderDTO> GetAllMedarbejder()
        {
            return _medarbejdere.GetAllMedarbejder();
        }

        public void AddMedarbejder(MedarbejderDTO medarbejderDTO)
        {
            _medarbejdere.AddMedarbejder(medarbejderDTO);
        }

        public void DeleteMedarbejder(int medarbejderId)
        {
            _medarbejdere.DeleteMedarbejder(medarbejderId);
        }

        //_______--__----___-
        public VagtDTO GetVagt(int vagtId)
        {
            return _vagter.GetVagt(vagtId);
        }

        public List<VagtDTO> GetAllVagt()
        {
            return _vagter.GetAllVagt();
        }

        public void AddVagt(VagtDTO vagtDTO)
        {
            _vagter.AddVagt(vagtDTO);
        }

        public void DeleteVagt(int vagtId)
        {
            _vagter.DeleteVagt(vagtId);
        }




















        public void Save()
        {
            _context.SaveChanges();
        }


        // IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _context.Dispose();
                }

                // free unmanaged resources if any

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // prevent finalizer from running
        }
    }
}
