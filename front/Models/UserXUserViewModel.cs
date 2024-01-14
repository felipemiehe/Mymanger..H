using Auth.DTO.Response;

namespace front.Models
{
    public class UserXUserViewModel : UserXUserResponseDTO
    {
        public UserXUserViewModel(int id, string userId, string email, string nome, string numero, string cpf, string codigoUnico, List<string> roles, int totalAchados, int totalPages) : base(id, userId, email, nome, numero, cpf, codigoUnico, roles, totalAchados, totalPages)
        {
        }
    }
}
