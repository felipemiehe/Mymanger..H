using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AddResponsavelEdificioDTO
    {
        [Required(ErrorMessage = "Codigo Unico Ativo é obrigatório")]
        public string CodigoUnicoAtivo { get; set; }
        [Required(ErrorMessage = "email Responsavel é obrigatório")]
        public string emailResponsavel { get; set; }
    }
}
