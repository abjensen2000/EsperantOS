using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.BusinessLogic
{
    public class VagtBLL
    {
        private readonly IUnitOfWork _unitOfWork;

        public VagtBLL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<VagtDTO>> GetAllVagterAsync()
        {
            var vagter = await _unitOfWork.VagtRepository.GetAllVagterWithMedarbejdereAsync();
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        public async Task<List<VagtDTO>> GetFridayVagterAsync()
        {
            var vagter = await _unitOfWork.VagtRepository.GetFridayVagterAsync();
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        public async Task<VagtDTO?> GetVagtByIdAsync(int id)
        {
            var vagt = await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(id);

            if (vagt == null)
            {
                return null;
            }

            return VagtMapper.ToDto(vagt);
        }

        public async Task<List<VagtDTO>> GetVagterByMedarbejderAsync(int medarbejderId)
        {
            var vagter = await _unitOfWork.VagtRepository.GetVagterByMedarbejderAsync(medarbejderId);
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        public async Task<List<VagtDTO>> GetVagterByMedarbejderNameAsync(string name)
        {
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderByNameAsync(name);
            if (medarbejder == null)
                return new List<VagtDTO>();

            var vagter = await _unitOfWork.VagtRepository.GetVagterByMedarbejderAsync(medarbejder.Id);
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        // Sikrer at alle fredage har mindst én ædru-vagt – opretter automatisk hvis den mangler
        public async Task EnsureAedruVagterAsync()
        {
            var fridayVagter = await _unitOfWork.VagtRepository.GetFridayVagterAsync();
            var uniqueDates = fridayVagter.Select(v => v.Dato.Date).Distinct().ToList();
            bool changesMade = false;

            foreach (var date in uniqueDates)
            {
                if (!fridayVagter.Any(v => v.Dato.Date == date && v.Ædru))
                {
                    var nyVagt = new Vagt
                    {
                        Dato = date.AddHours(20),
                        Ædru = true,
                        Frigivet = true,
                        Medarbejdere = new List<Medarbejder>()
                    };
                    await _unitOfWork.VagtRepository.AddAsync(nyVagt);
                    changesMade = true;
                }
            }

            if (changesMade)
                await _unitOfWork.SaveChangesAsync();
        }

        public async Task CreateVagtAsync(VagtDTO vagtDto)
        {
            var vagt = VagtMapper.ToEntity(vagtDto);
            await _unitOfWork.VagtRepository.AddAsync(vagt);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateVagtAsync(VagtDTO vagtDto)
        {
            // Hent sporet entitet så EF Core kan diff mange-til-mange relationen korrekt
            var existing = await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(vagtDto.Id);
            if (existing == null) return;

            existing.Dato = vagtDto.Dato;
            existing.Ædru = vagtDto.Ædru;
            existing.Frigivet = vagtDto.Frigivet;

            // Ryd og genopbyg medarbejderlisten med sporede entiteter for at undgå duplikater
            existing.Medarbejdere.Clear();
            foreach (var mDto in vagtDto.Medarbejdere)
            {
                var medarbejder = await _unitOfWork.MedarbejderRepository.GetByIdAsync(mDto.Id);
                if (medarbejder != null)
                    existing.Medarbejdere.Add(medarbejder);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteVagtAsync(int id)
        {
            await _unitOfWork.VagtRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> VagtExistsAsync(int id)
        {
            return await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(id) != null;
        }
    }
}
