namespace RiskReportService.Models
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public required string Nom { get; set; }
        public required string Prenom { get; set; }
        public required DateTime DateNaissance { get; set; }
        public required int Genre { get; set; }
        public required string Adresse { get; set; }
        public required string Telephone { get; set; }
    }

}
