using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AssociaRolesDTO
    {
        [Required(ErrorMessage = "Role nome é obrigatorio")]
        public string RoleName { get; set; }
        [Required(ErrorMessage = "User id é obrigatorio")]
        public string UserId { get; set; }
        

    }
}
