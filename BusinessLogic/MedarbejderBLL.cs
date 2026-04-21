using DTO.Model;
using DataAccess.Repositories;

namespace BusinessLogic
{
    public class MedarbejderBLL
    {
        public MedarbejderDTO GetMedarbejder(int medarbejderId)
        {
            using var uow = new UnitOfWork();

            return uow.GetMedarbejder(medarbejderId);
        }

        public List<MedarbejderDTO> GetAllMedarbejder()
        {
            using var uow = new UnitOfWork();

            return uow.GetAllMedarbejder();
        }

        public void AddMedarbejder(MedarbejderDTO medarbejderDTO)
        {
            using var uow = new UnitOfWork();

            uow.AddMedarbejder(medarbejderDTO);
        }

        public void DeleteMedarbejder(int medarbejderId)
        {
            using var uow = new UnitOfWork();

            uow.DeleteMedarbejder(medarbejderId);
        }
    }
}
