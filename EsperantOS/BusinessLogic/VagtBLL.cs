using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DataAccess.Mappers;
using EsperantOS.DTO.Model;
using EsperantOS.Models;

namespace EsperantOS.BusinessLogic
{
    // Forretningslogiklaget for vagter.
    // Denne klasse indeholder al logik der handler om vagter – hentning, oprettelse, opdatering og sletning.
    // Controllerne kalder udelukkende denne klasse og arbejder altid med DTO'er, aldrig direkte med entiteter.
    public class VagtBLL
    {
        // UnitOfWork giver adgang til begge repositories via én fælles databaseforbindelse
        private readonly IUnitOfWork _unitOfWork;

        // Konstruktør – IUnitOfWork injiceres automatisk af ASP.NET Core
        public VagtBLL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Henter alle vagter fra databasen inklusiv tilknyttede medarbejdere.
        // Konverterer entiteter til DTO'er før returnering.
        public async Task<List<VagtDTO>> GetAllVagterAsync()
        {
            var vagter = await _unitOfWork.VagtRepository.GetAllVagterWithMedarbejdereAsync();
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        // Henter kun fredagsvagter – det er de eneste vagter der vises i vagtplanen
        public async Task<List<VagtDTO>> GetFridayVagterAsync()
        {
            var vagter = await _unitOfWork.VagtRepository.GetFridayVagterAsync();
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        // Henter én bestemt vagt ud fra ID.
        // Returnerer null hvis vagten ikke eksisterer.
        public async Task<VagtDTO?> GetVagtByIdAsync(int id)
        {
            var vagt = await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(id);
            return vagt != null ? VagtMapper.ToDto(vagt) : null;
        }

        // Henter alle vagter for en bestemt medarbejder ud fra medarbejder-ID
        public async Task<List<VagtDTO>> GetVagterByMedarbejderAsync(int medarbejderId)
        {
            var vagter = await _unitOfWork.VagtRepository.GetVagterByMedarbejderAsync(medarbejderId);
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        // Henter alle vagter for en medarbejder ud fra navn.
        // Bruges på forsiden til at vise den indloggede brugers egne vagter.
        // Hvis medarbejderen ikke findes i databasen, returneres en tom liste.
        public async Task<List<VagtDTO>> GetVagterByMedarbejderNameAsync(string name)
        {
            // Slår medarbejderen op via navn (case-insensitiv søgning i repository)
            var medarbejder = await _unitOfWork.MedarbejderRepository.GetMedarbejderByNameAsync(name);
            if (medarbejder == null)
                return new List<VagtDTO>(); // Ingen medarbejder fundet – returner tom liste

            // Hent vagterne via medarbejderens ID
            var vagter = await _unitOfWork.VagtRepository.GetVagterByMedarbejderAsync(medarbejder.Id);
            return vagter.Select(VagtMapper.ToDto).ToList();
        }

        // Sikrer at der findes en ædru-vagt for alle fredage.
        // Kaldes automatisk når vagtplanen indlæses (VagtsController.Index).
        // Hvis en fredag mangler en vagt med Ædru = true, oprettes der automatisk én.
        public async Task EnsureAedruVagterAsync()
        {
            var fridayVagter = await _unitOfWork.VagtRepository.GetFridayVagterAsync();

            // Find alle unikke fredagsdatoer i vagtplanen
            var uniqueDates = fridayVagter.Select(v => v.Dato.Date).Distinct().ToList();
            bool changesMade = false;

            foreach (var date in uniqueDates)
            {
                // Tjek om der allerede eksisterer en ædru-vagt på denne dato
                if (!fridayVagter.Any(v => v.Dato.Date == date && v.Ædru))
                {
                    // Ingen ædru-vagt fundet – opret en ny ledig (frigivet) ædru-vagt kl. 20:00
                    var nyVagt = new Vagt
                    {
                        Dato = date.AddHours(20),
                        Ædru = true,
                        Frigivet = true,       // Ledig – kan tages af en medarbejder
                        Medarbejdere = new List<Medarbejder>()
                    };
                    await _unitOfWork.VagtRepository.AddAsync(nyVagt);
                    changesMade = true;
                }
            }

            // Gem kun til databasen hvis der rent faktisk blev oprettet nye vagter
            if (changesMade)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // Opretter en ny vagt i databasen ud fra et DTO.
        // Konverterer DTO til entitet via mapper, og gemmer derefter.
        public async Task CreateVagtAsync(VagtDTO vagtDto)
        {
            var vagt = VagtMapper.ToEntity(vagtDto);
            await _unitOfWork.VagtRepository.AddAsync(vagt);
            await _unitOfWork.SaveChangesAsync();
        }

        // Opdaterer en eksisterende vagt.
        // Vi henter den sporede (tracked) entitet fra databasen først,
        // så EF Core kan beregne præcis hvad der er ændret og generere den rette SQL.
        // Medarbejder-relationen opdateres manuelt via sporede entiteter.
        public async Task UpdateVagtAsync(VagtDTO vagtDto)
        {
            // Hent den eksisterende vagt fra databasen (sporét af EF Core)
            var existing = await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(vagtDto.Id);
            if (existing == null) return; // Vagten findes ikke – gør ingenting

            // Opdater de simple felter direkte på den sporede entitet
            existing.Dato = vagtDto.Dato;
            existing.Ædru = vagtDto.Ædru;
            existing.Frigivet = vagtDto.Frigivet;

            // Opdater mange-til-mange relationen til medarbejdere.
            // Vi rydder listen og bygger den op igen med sporede entiteter,
            // ellers vil EF Core forsøge at indsætte duplikater.
            existing.Medarbejdere.Clear();
            foreach (var mDto in vagtDto.Medarbejdere)
            {
                var medarbejder = await _unitOfWork.MedarbejderRepository.GetByIdAsync(mDto.Id);
                if (medarbejder != null)
                    existing.Medarbejdere.Add(medarbejder);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // Sletter en vagt fra databasen ud fra ID
        public async Task DeleteVagtAsync(int id)
        {
            await _unitOfWork.VagtRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        // Tjekker om en vagt med det givne ID eksisterer i databasen.
        // Bruges af controlleren inden sletning for at undgå fejl.
        public async Task<bool> VagtExistsAsync(int id)
        {
            return await _unitOfWork.VagtRepository.GetVagtWithMedarbejdereAsync(id) != null;
        }
    }
}
