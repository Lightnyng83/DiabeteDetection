using System.ComponentModel.DataAnnotations;

namespace MicroFrontend.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
    }
}