using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class CriarUserxAtivoDTO
    {
        [Required(ErrorMessage = "Id do Ativo é obrigatório")]
        public int ativoId { get; set; }
        [Required(ErrorMessage = "email do User é obrigatório")]
        public List<string> emails { get; set; }
    }
}
