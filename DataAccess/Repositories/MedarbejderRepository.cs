using DataAccess.Mappers;
using DataAccess.Model;
using DTO.Model;
using EsperantOS.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    internal class MedarbejderRepository
    {
        private EsperantOSContext _context;

        public MedarbejderRepository(EsperantOSContext context)
        {
            _context = context;
        }

        public MedarbejderDTO GetMedarbejder(int medarbejderId)
        {
            return MedarbejderMapper.medarbejderTilDTO(_context.Medarbejdere.Find(medarbejderId));
        }

        public List<MedarbejderDTO> GetAllMedarbejder()
        {
            return MedarbejderMapper.medarbejdereTilDTO(_context.Medarbejdere.ToList());
        }

        public void AddMedarbejder(MedarbejderDTO medarbejderDTO)
        {
            _context.Medarbejdere.Add(MedarbejderMapper.DTOTilMedarbejder(medarbejderDTO));
        }

        public void DeleteMedarbejder(int medarbejderId)
        {
            _context.Medarbejdere.Remove(_context.Medarbejdere.Find(medarbejderId));
        }
    }
}
