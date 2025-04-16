namespace MicroFrontend.Models
{
    public class NoteViewModel
    {
        public string? Id { get; set; }
        public Guid PatientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}