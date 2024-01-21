namespace Auth.DTO.Response
{
    public class UserxAtivosEdificiosResponseDTO
    {
        public UserxAtivosEdificiosResponseDTO()
        {

        }

        public UserxAtivosEdificiosResponseDTO(int Id, string Nome, string Endereco, int? NumeroAptos, string Responsavel_email, int TotalAchados, int TotalPages)
        {
            this.Id = Id;
            this.Nome = Nome;
            this.Endereco = Endereco ?? "sem endereco";
            this.NumeroAptos = NumeroAptos ?? 0; 
            this.Responsavel_email = Responsavel_email;
            this.TotalAchados = TotalAchados;
            this.TotalPages = TotalPages;
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public int NumeroAptos { get; set; }      
        public string Responsavel_email { get; set; }
        public int TotalAchados { get; set; }
        public int TotalPages { get; set; }
    }
}
