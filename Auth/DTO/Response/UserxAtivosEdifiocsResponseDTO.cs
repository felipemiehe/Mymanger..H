namespace Auth.DTO.Response
{
    public class UserxAtivosEdificiosResponseDTO
    {
        public UserxAtivosEdificiosResponseDTO()
        {

        }

        public UserxAtivosEdificiosResponseDTO(int Id, string Nome, string Endereco, int? NumeroAptos, List<string> ResponsaveisEmails, string CodigoUnico, int TotalAchados, int TotalPages)
        {
            this.Id = Id;
            this.Nome = Nome;
            this.Endereco = Endereco ?? "sem endereco";
            this.NumeroAptos = NumeroAptos ?? 0;
            this.ResponsaveisEmails = ResponsaveisEmails;
            this.CodigoUnico = CodigoUnico;
            this.TotalAchados = TotalAchados;
            this.TotalPages = TotalPages;
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string CodigoUnico { get; set; }
        public int NumeroAptos { get; set; }      
        public List<string> ResponsaveisEmails { get; set; }
        public int TotalAchados { get; set; }
        public int TotalPages { get; set; }
    }
}
