using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AddRolesDTO
    {
        [Required(ErrorMessage = "cargos é obrigatório")]
        public List<string> roles { get; set; }
    }
}
