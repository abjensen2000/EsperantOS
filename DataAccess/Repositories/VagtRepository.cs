using DataAccess.Mappers;
using DTO.Model;
using EsperantOS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    internal class VagtRepository
    {
        private EsperantOSContext _context;

        public VagtRepository(EsperantOSContext context)
        {
            _context = context;
        }

        public List<MedarbejderDTO> GetMedarbejdereInVagt(VagtDTO vagt) {
            var foundVagt = _context.Vagter.Include((i) => i.Medarbejdere).FirstOrDefault((i) => i.Id == vagt.Id);

            if (foundVagt == null) {
                return new List<MedarbejderDTO>();
            }

            return MedarbejderMapper.medarbejdereTilDTO(foundVagt.Medarbejdere);
        }

        public VagtDTO GetVagt(int vagtId)
        {
            return VagtMapper.vagtTilDTO(_context.Vagter.Find(vagtId));
        }

        public List<VagtDTO> GetAllVagt()
        {
            return VagtMapper.vagterTilDTO(_context.Vagter.ToList());
        }

        public void AddVagt(VagtDTO vagtDTO)
        {
            _context.Vagter.Add(VagtMapper.DTOTilVagt(vagtDTO));
        }

        public void DeleteVagt(int vagtId)
        {
            _context.Vagter.Remove(_context.Vagter.Find(vagtId));
        }

        public VagtDTO GetVagtMedMedarbejdere(int vagtId)
        {
            return VagtMapper.vagtTilDTO(
                _context.Vagter
                    .Include(v => v.Medarbejdere)
                    .FirstOrDefault(v => v.Id == vagtId)
            );
        }

        public List<VagtDTO> GetAllVagtMedMedarbejdere()
        {
            return VagtMapper.vagterTilDTO(
                _context.Vagter
                    .Include(v => v.Medarbejdere)
                    .ToList()
            );
        }

        public void AddMedarbejderToVagt(int vagtId, int medarbejderId)
        {
            var vagt = _context.Vagter
                .Include(v => v.Medarbejdere)
                .FirstOrDefault(v => v.Id == vagtId);

            var medarbejder = _context.Medarbejdere.Find(medarbejderId);

            if (vagt != null && medarbejder != null && !vagt.Medarbejdere.Contains(medarbejder))
            {
                vagt.Medarbejdere.Add(medarbejder);
            }
        }

        public void RemoveMedarbejderFromVagt(int vagtId, int medarbejderId)
        {
            var vagt = _context.Vagter
                .Include(v => v.Medarbejdere)
                .FirstOrDefault(v => v.Id == vagtId);

            var medarbejder = _context.Medarbejdere.Find(medarbejderId);

            if (vagt != null && medarbejder != null)
            {
                vagt.Medarbejdere.Remove(medarbejder);
            }
        }

    }
}
