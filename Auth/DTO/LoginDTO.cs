using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Auth.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "email is required")]
        [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]

        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        
    }
}
