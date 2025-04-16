namespace RiskReportService.Models
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Content { get; set; }
    }

}
