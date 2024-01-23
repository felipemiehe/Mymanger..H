using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AtualizaEdficiosDTO
    {
        public AtualizaEdficiosDTO()
        {
        } 
        public AtualizaEdficiosDTO(AtualizaEdficiosDTO model)
        {
           this.Nome = model.Nome;
           this.Endereco = model.Endereco;
           this.CodigoUnico = model.CodigoUnico;
        }

        [Required(ErrorMessage = "Nome is required")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "Endereco is required")]
        public string Endereco {  get; set; }

        [Required(ErrorMessage = "CodigoUnico is required")]
        public string CodigoUnico { get; set; }




    }
}
