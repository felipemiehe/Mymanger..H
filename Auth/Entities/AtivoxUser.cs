using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class AtivoxUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string User_id { get; set; }

        [Required]
        public int Ativo_id { get; set; }

        // Propriedade de navegação para representar o relacionamento com Ativo
        [ForeignKey("Ativo_id")]
        public Ativo Ativo { get; set; }

    }
}
