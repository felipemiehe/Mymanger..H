namespace Auth.DTO.Response
{
    public class EdificioFindForEditResponseDTO
    {
        public EdificioFindForEditResponseDTO(string endereco, string nome, string codigoUnico)
        {
            this.Endereco = endereco;
            this.Nome = nome;
            this.CodigoUnico  = codigoUnico;
        }

        public EdificioFindForEditResponseDTO()
        {

        }

        public string Endereco { get; set; }
        public string Nome { get; set; }
        public string CodigoUnico { get; set; }
        
    }
}
