using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.Model;
using DTO.Model;

namespace DataAccess.Mappers
{
    internal class MedarbejderMapper
    {
        public static MedarbejderDTO medarbejderTilDTO(Medarbejder medarbejder)
        {
            return new MedarbejderDTO(medarbejder.Id, medarbejder.Name, medarbejder.Bestyrelsesmedlem, medarbejder.PasswordHash);
        }

        public static List<MedarbejderDTO> medarbejdereTilDTO(List<Medarbejder> medarbejdere)
        {
            var mappedMedarbejdere = new List<MedarbejderDTO>();
            foreach (Medarbejder medarbejderDTO in medarbejdere)
            {
                mappedMedarbejdere.Add(medarbejderTilDTO(medarbejderDTO));
            }
            return mappedMedarbejdere;
        }


        public static Medarbejder DTOTilMedarbejder(MedarbejderDTO medarbejder)
        {
            return new Medarbejder(medarbejder.Id, medarbejder.Name, medarbejder.Bestyrelsesmedlem, medarbejder.PasswordHash);
        }

        public static List<Medarbejder> DTOTilMedarbejdere(List<MedarbejderDTO> dtoMedarbejdere)
        {
            var mappedMedarbejdere = new List<Medarbejder>();
            foreach (MedarbejderDTO medarbejder in dtoMedarbejdere)
            {
                mappedMedarbejdere.Add(DTOTilMedarbejder(medarbejder));
            }
            return mappedMedarbejdere;
        }


    }
}
