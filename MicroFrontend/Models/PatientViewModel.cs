namespace MicroFrontend.Models
{
    public class PatientViewModel
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateOnly DateNaissance { get; set; }
        public int Genre { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }

    }
}
