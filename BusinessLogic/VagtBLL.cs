using DataAccess.Repositories;
using DTO.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic
{
    public class VagtBLL
    {
        public VagtDTO GetVagt(int vagtId)
        {
            using var uow = new UnitOfWork();
            return uow.GetVagt(vagtId);
        }

        public List<VagtDTO> GetAllVagt()
        {
            using var uow = new UnitOfWork();
            return uow.GetAllVagt();
        }

        public void AddVagt(VagtDTO vagtDTO)
        {
            using var uow = new UnitOfWork();
            uow.AddVagt(vagtDTO);
        }

        public void DeleteVagt(int vagtId)
        {
            using var uow = new UnitOfWork();

            uow.DeleteVagt(vagtId);
        }
    }
}
