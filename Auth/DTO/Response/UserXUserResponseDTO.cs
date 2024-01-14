namespace Auth.DTO.Response
{
    public class UserXUserResponseDTO
    {
        public UserXUserResponseDTO(int id, string userId, string email, string nome, string numero, string cpf, string codigoUnico, List<string> roles)
        {
            Id = id;
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Numero = numero ?? throw new ArgumentNullException(nameof(numero));
            Cpf = cpf ?? throw new ArgumentNullException(nameof(cpf));
            CodigoUnico = codigoUnico ?? throw new ArgumentNullException(nameof(codigoUnico));
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
        public string Numero { get; set; }
        public string Cpf { get; set; }
        public string CodigoUnico { get; set; }
        public List<string> Roles { get; set; }
    }
}
