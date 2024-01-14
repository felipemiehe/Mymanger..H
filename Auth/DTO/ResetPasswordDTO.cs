using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "email is required")]        
        public String? Email {  get; set; }

        [Required(ErrorMessage = "token is required")]
        public String? Token { get; set; }

        [Required(ErrorMessage = "password is required")]
        public String? NewPassword { get; set; }  
        
    }
}
