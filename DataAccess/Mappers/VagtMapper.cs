using DataAccess.Model;
using DTO.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Mappers
{
    internal class VagtMapper
    {
        public static VagtDTO vagtTilDTO(Vagt vagt)
        {
            return new VagtDTO(vagt.Id, vagt.Dato, vagt.Ædru, vagt.Frigivet);
        }

        public static List<VagtDTO> vagterTilDTO(List<Vagt> vagter)
        {
            var mappedVagter = new List<VagtDTO>();
            foreach (Vagt vagtDTO in vagter)
            {
                mappedVagter.Add(vagtTilDTO(vagtDTO));
            }
            return mappedVagter;
        }


        public static Vagt DTOTilVagt(VagtDTO vagt)
        {
            return new Vagt(vagt.Id, vagt.Dato, vagt.Ædru, vagt.Frigivet);
        }

        public static List<Vagt> DTOTilVagter(List<VagtDTO> dtoVagter)
        {
            var mappedVagter = new List<Vagt>();
            foreach (VagtDTO vagt in dtoVagter)
            {
                mappedVagter.Add(DTOTilVagt(vagt));
            }
            return mappedVagter;
        }
    }
}
