using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class Chamado
    {
        [Key] public int Id { get; set; }
        [Required] public string Titulo { get; set; }
        [Required] public string? Descricao { get; set; }
        public ICollection<FotoUrl> Foto_URLs { get; set; } = new List<FotoUrl>();
        public bool Finalizado { get; set; }
        public bool Executando { get; set; }
        public DateTime Data_criacao { get; set; } = DateTime.Now;
        public DateTime? Data_finalizacao { get; set; }
        public ICollection<DataPause> Data_pause { get; set; } = new List<DataPause>();
        public bool Em_pause { get; set; }
        public bool Em_vistoria{ get; set; }        
        public bool Em_orcamento { get; set; }      
        public string? Responsavel_user_ID { get; set; }
        [Required] public string Dono_chamado_user_ID { get; set; }
        [Required] public int Ativo_Id { get; set; }

        [ForeignKey("Ativo_Id")]
        public Ativo Ativo { get; set; }

        [ForeignKey("Responsavel_user_ID")]
        public ApplicationUser Responsavel { get; set; }

        [ForeignKey("Dono_chamado_user_ID")]
        public ApplicationUser DonoChamado { get; set; }

        

    }
}
