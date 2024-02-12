using Auth.DbContext;
using Auth.DTO;
using Auth.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Auth.Controllers
{
    [Route("api/chamados")]
    [ApiController]
    public class ChamadosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AtivoController> _logger;
        private readonly SqlErrorHandler _sqlErrorHandler;

        public ChamadosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AtivoController> logger, SqlErrorHandler sqlErrorHandler)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _sqlErrorHandler = sqlErrorHandler;
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.User},{UserRoles.Admin}")]
        [Route("criarChamado")]
        public async Task<IActionResult> AdicionarAtivo(AddChamadadosDTO dto)
        {
            try
            {
                string userAdminId = User.FindFirstValue("userId");

                var userResponsavel = await pegaUserResponsavel(dto.Responsavel_User_Email, userAdminId);               

                var edificio = await pegaEdificioPorCodUnico(dto.Ativo_CodigoUnico, userAdminId);
                if (edificio == null)
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = $"O Codigo do edificio '{dto.Ativo_CodigoUnico}' não pertence a voce!." });
                }
               

                var chamado = new Chamado
                {
                    Responsavel_user_ID = userResponsavel.Id,
                    Dono_chamado_user_ID = userAdminId,
                    Criado = true,                    
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,

                };

                _context.Chamados.Add(chamado);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { Status = "Success" , Message= "ta pegando" });                

            }
            catch (Exception ex)
            {
                return _sqlErrorHandler.HandleSqlException(ex, nameof(AdicionarAtivo));
                throw;
            }
        }


        // ! --------- PRIVATES ----- ! //

        private async Task<ApplicationUser> pegaUserResponsavel(string email, string idAdminUser)
        {
            var userResponsavel = await _userManager.FindByEmailAsync(email);
            if (userResponsavel != null &&
                await _context.UserxUsers.AnyAsync(q => q.User_Admin_Id == idAdminUser && q.User_Agregado_Id == userResponsavel.Id))
            {
                return userResponsavel;
            }
            return null;
        }

        private async Task<Ativo?> pegaEdificioPorCodUnico(string codUnico, string idAdminUser)
        {
            var edificio = await _context.Ativos.FirstOrDefaultAsync(q => q.CodigoUnico == codUnico);            
            if(await _context.AtivoxUsers
                            .FirstOrDefaultAsync(u => u.Ativo_id == edificio.Id && u.User_id == idAdminUser) != null)
            {
                 return edificio;

            }

            return null;
        }
              

    }
}
