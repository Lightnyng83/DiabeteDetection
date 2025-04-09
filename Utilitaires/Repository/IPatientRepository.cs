using PatientService.Models;

namespace Utilitaires.Repository
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllPatientsAsync();
    }
}
