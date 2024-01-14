using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Auth.Entities
{
    public class Ativo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

       // public string? Endereco { get; set; }

        // Propriedade de navegação para representar o relacionamento
        public ICollection<AtivoxUser> AtivoxUsers { get; set; } = new List<AtivoxUser>();
        public ICollection<Chamado> Chamados { get; set; } = new List<Chamado>();

    }
}
