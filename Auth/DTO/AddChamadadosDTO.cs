using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AddChamadadosDTO
    {
        [Required(ErrorMessage = "Titulo é obrigatório")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "Descricao é obrigatório")]
        public string Descricao { get; set; }        
        public string? Responsavel_User_Email { get; set; }       
        [Required(ErrorMessage = "Codigo Unico do Edificio é obrigatório")]  
        public string Ativo_CodigoUnico { get; set; }

        // URLs de fotos path do PC
        [Required(ErrorMessage = "imagem é obrigatória")]  
        public string foto_url { get; set; }


    }
}
