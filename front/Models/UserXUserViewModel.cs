using Auth.DTO.Response;

namespace front.Models
{
    public class UserXUserViewModel : UserXUserResponseDTO
    {
        public UserXUserViewModel(int id, string userId, string email, string nome, string numero, string cpf, string codigoUnico, List<string> roles) : base(id, userId, email, nome, numero, cpf, codigoUnico, roles)
        {
        }
    }
}
