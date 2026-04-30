using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.BusinessLogic
{
    // Forretningslogiklaget for medarbejdere.
    // Håndterer al logik relateret til medarbejdere – hentning, oprettelse, opdatering og sletning.
    // Controllerne bruger kun denne klasse og arbejder altid med DTO'er.
    public class MedarbejderBLL
    {
        // UnitOfWork giver adgang til repositories og styrer databasetransaktioner
        private readonly IUnitOfWork _unitOfWork;

        // Konstruktør – IUnitOfWork injiceres automatisk af ASP.NET Core
        public MedarbejderBLL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Henter alle medarbejdere fra databasen som en liste af DTO'er
        public async Task<List<MedarbejderDTO>> GetAllMedarbejdereAsync()
        {
            var medarbejdere = await _unitOfWork.MedarbejderRepository.GetAllAsync();
            return medarbejdere.Select(MedarbejderMapper.ToDto).ToList();
        }

        // Henter én medarbejder ud fra ID, inklusiv deres vagter.
        // Returnerer null hvis medarbejderen ikke findes.
        public async Task<MedarbejderDTO?> GetMedarbejderByIdAsync(int id)
        {
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderWithVagterAsync(id);
            return medarbejder != null ? MedarbejderMapper.ToDto(medarbejder) : null;
        }

        // Henter en medarbejder ud fra navn.
        // Søgningen er case-insensitiv – "simon" matcher "Simon".
        // Bruges ved login og til vagtopslag via brugernavn.
        public async Task<MedarbejderDTO?> GetMedarbejderByNameAsync(string name)
        {
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderByNameAsync(name);
            return medarbejder != null ? MedarbejderMapper.ToDto(medarbejder) : null;
        }

        // Henter alle medarbejdere der er markeret som bestyrelsesmedlemmer
        public async Task<List<MedarbejderDTO>> GetBestyrelsesmedlemmerAsync()
        {
            var medlemmer = await _unitOfWork.MedarbejderRepository.GetBestyrelsesmedlemmerAsync();
            return medlemmer.Select(MedarbejderMapper.ToDto).ToList();
        }

        // Opretter en ny medarbejder i databasen ud fra et DTO.
        // Bruges bl.a. i TagVagt-handlingen når en ukendt bruger tager en vagt.
        public async Task CreateMedarbejderAsync(MedarbejderDTO medarbejderDto)
        {
            var medarbejder = MedarbejderMapper.ToEntity(medarbejderDto);
            await _unitOfWork.MedarbejderRepository.AddAsync(medarbejder);
            await _unitOfWork.SaveChangesAsync();
        }

        // Opdaterer en eksisterende medarbejder i databasen.
        // Konverterer DTO til entitet og gemmer ændringerne.
        public async Task UpdateMedarbejderAsync(MedarbejderDTO medarbejderDto)
        {
            var medarbejder = MedarbejderMapper.ToEntity(medarbejderDto);
            await _unitOfWork.MedarbejderRepository.UpdateAsync(medarbejder);
            await _unitOfWork.SaveChangesAsync();
        }

        // Sletter en medarbejder fra databasen ud fra ID
        public async Task DeleteMedarbejderAsync(int id)
        {
            await _unitOfWork.MedarbejderRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
