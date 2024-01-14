using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage= "Email is required")]
        [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
        public String? Email { get; set; }
    }
}
