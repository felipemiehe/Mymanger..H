using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class ListResponsaveisAtivos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string email_responsavel_criado { get; set; }

        [Required]
        public int Ativo_id { get; set; }

        [ForeignKey("Ativo_id")]
        public Ativo Ativo { get; set; }
        
        public ApplicationUser ResponsavelEmail{ get; set; }


    }
}
