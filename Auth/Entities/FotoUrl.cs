using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class FotoUrl
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public int Chamado_id { get; set; }

        [ForeignKey("Chamado_id")]
        public Chamado Chamado { get; set; }
    }
}
