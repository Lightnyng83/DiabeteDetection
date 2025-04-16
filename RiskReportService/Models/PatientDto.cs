namespace RiskReportService.Models
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime DateDeNaissance { get; set; }
        public int Genre { get; set; }
    }

}
