using PatientService.Models;

namespace PatientService.Repository
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllPatientsAsync();
    }
}
