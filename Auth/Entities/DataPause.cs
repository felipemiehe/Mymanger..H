using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class DataPause
    {
        [Key]
        public int Id { get; set; }
        public int Chamado_id { get; set; }
        public DateTime Data { get; set; }
        public int pausado_por_id { get; set; }

        [ForeignKey("Chamado_id")]
        public Chamado Chamado { get; set; }

        [ForeignKey("pausado_por_id")]
        public Chamado PausadoPor { get; set; }
    }
}