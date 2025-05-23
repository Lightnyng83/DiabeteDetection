﻿namespace MicroFrontend.Models
{
    public class PatientViewModel
    {
        public Guid Id { get; set; }
        public required string Nom { get; set; }
        public required string Prenom { get; set; }
        public required DateOnly DateNaissance { get; set; }
        public required Gender Genre { get; set; }
        public required string Adresse { get; set; }
        public required string Telephone { get; set; }

    }

    public enum Gender
    {
        Homme = 0,
        Femme = 1
    }
}
