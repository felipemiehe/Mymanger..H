using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class CriarAtivoDTO
    {
               
        [Required(ErrorMessage = "Nome do Ativo é obrigatório")]
        [MinLength(6, ErrorMessage = "O Nome do Ativo deve ter pelo menos 6 caracteres")]
        [DataType(DataType.Text, ErrorMessage = "O campo deve ser uma string.")]
        public string Nome { get; set; }

        public string? Endereco { get; set; }

        public int? AtivoId { get; set;}
       
    }
}
