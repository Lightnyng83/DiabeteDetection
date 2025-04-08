namespace MicroFrontend.Models
{
    public class PatientDetailViewModel
    {
        public PatientViewModel Patient { get; set; } = default!;
        public List<NoteViewModel> Notes { get; set; } = new List<NoteViewModel>();
    }
}