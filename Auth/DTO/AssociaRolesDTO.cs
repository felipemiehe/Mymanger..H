using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AssociaRolesDTO
    {
        [Required(ErrorMessage = "Função é obrigatorio")]
        public string RoleName { get; set; }
        [Required(ErrorMessage = "Codigo Unico é obrigatorio")]
        public string CodigoUnico { get; set; }
        

    }
}
