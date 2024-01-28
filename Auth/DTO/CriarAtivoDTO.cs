using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class CriarAtivoDTO
    {
               
        [Required(ErrorMessage = "Nome do Ativo é obrigatório")]
        [MinLength(6, ErrorMessage = "O Nome do Ativo deve ter pelo menos 6 caracteres")]
        [DataType(DataType.Text, ErrorMessage = "O campo deve ser um texto.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "endereço obrigatório")]
        public string? Endereco { get; set; }

        [Required(ErrorMessage = "Codigo Unico é obrigatório")]
        public String CodigoUnico { get; set; }        

        public int? NumeroAptos { get;  set; }

        [Required(ErrorMessage = "Email do responsavel é obrigatório")]
        public String Responsavel_email { get; set; }


    }
}
