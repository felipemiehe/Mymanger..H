using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace Auth.Entities
{
    // Ativo é o edificio
    public class Ativo
    {
        [Key]
        public int Id { get; set; }

        [Required]        
        public string Nome { get; set; }

        public string? Endereco { get; set; }
        // pode ser numero so de referencia como 50 unidades separadas dentro do edificio
        public int? NumeroAptos { get; set; }

        [Required]
        public string Responsavel_email { get; set; }

        // responsavel pelo edificio
        [ForeignKey("Responsavel_email")]


        // Propriedade de navegação para representar o relacionamento
        public ICollection<AtivoxUser> AtivoxUsers { get; set; } = new List<AtivoxUser>();
        public ICollection<Chamado> Chamados { get; set; } = new List<Chamado>();

    }
}
