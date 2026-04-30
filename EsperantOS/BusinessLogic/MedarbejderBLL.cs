using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.BusinessLogic
{
    public class MedarbejderBLL
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedarbejderBLL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MedarbejderDTO>> GetAllMedarbejdereAsync()
        {
            var medarbejdere = await _unitOfWork.MedarbejderRepository.GetAllAsync();
            return medarbejdere.Select(MedarbejderMapper.ToDto).ToList();
        }

        public async Task<MedarbejderDTO?> GetMedarbejderByIdAsync(int id)
        {
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderWithVagterAsync(id);
            return medarbejder != null ? MedarbejderMapper.ToDto(medarbejder) : null;
        }

        public async Task<MedarbejderDTO?> GetMedarbejderByNameAsync(string name)
        {
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderByNameAsync(name);
            return medarbejder != null ? MedarbejderMapper.ToDto(medarbejder) : null;
        }

        public async Task<List<MedarbejderDTO>> GetBestyrelsesmedlemmerAsync()
        {
            var medlemmer = await _unitOfWork.MedarbejderRepository.GetBestyrelsesmedlemmerAsync();
            return medlemmer.Select(MedarbejderMapper.ToDto).ToList();
        }

        public async Task CreateMedarbejderAsync(MedarbejderDTO medarbejderDto)
        {
            var medarbejder = MedarbejderMapper.ToEntity(medarbejderDto);
            await _unitOfWork.MedarbejderRepository.AddAsync(medarbejder);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateMedarbejderAsync(MedarbejderDTO medarbejderDto)
        {
            var medarbejder = MedarbejderMapper.ToEntity(medarbejderDto);
            await _unitOfWork.MedarbejderRepository.UpdateAsync(medarbejder);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteMedarbejderAsync(int id)
        {
            await _unitOfWork.MedarbejderRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
