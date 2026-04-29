using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.Extensions
{
    public static class DtoModelExtensions
    {
        public static Vagt ToModel(this VagtDTO dto)
        {
            return new Vagt
            {
                Id = dto.Id,
                Dato = dto.Dato,
                Ædru = dto.Ædru,
                Frigivet = dto.Frigivet,
                Medarbejdere = dto.Medarbejdere?.Select(m => m.ToModel()).ToList() ?? new List<Medarbejder>()
            };
        }

        public static List<Vagt> ToModelList(this List<VagtDTO> dtos)
        {
            return dtos.Select(dto => dto.ToModel()).ToList();
        }

        public static Medarbejder ToModel(this MedarbejderDTO dto)
        {
            return new Medarbejder
            {
                Id = dto.Id,
                Name = dto.Name,
                Bestyrelsesmedlem = dto.Bestyrelsesmedlem,
                Vagter = dto.Vagter?.Select(v => new Vagt
                {
                    Id = v.Id,
                    Dato = v.Dato,
                    Ædru = v.Ædru,
                    Frigivet = v.Frigivet
                }).ToList() ?? new List<Vagt>()
            };
        }

        public static List<Medarbejder> ToModelList(this List<MedarbejderDTO> dtos)
        {
            return dtos.Select(dto => dto.ToModel()).ToList();
        }
    }
}
