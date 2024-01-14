using Auth.DbContext;
using Auth.DTO;
using Auth.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Auth.Controllers
{
    [Route("api/ativo")]
    [ApiController]
    public class AtivoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AtivoController> _logger;

        public AtivoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AtivoController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.User},{UserRoles.Admin}")]
        [Route("create")]
        public async Task<IActionResult> AdicionarAtivo([FromBody] CriarAtivoDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Ativo ativo = new Ativo();
                    ativo.Nome = dto.Nome;

                    string userId = User.FindFirstValue("userId");

                    _context.Ativos.Add(ativo);

                    // Associar o Ativo ao usuário que o criou
                    AtivoxUser ativoxUser = new AtivoxUser
                    {
                        User_id = userId,
                        Ativo_id = ativo.Id,
                        Ativo = ativo
                    };

                    _context.AtivoxUsers.Add(ativoxUser);

                    // Salva as mudanças no banco de dados
                    await _context.SaveChangesAsync();

                    return Ok(new ResponseDTO { Status = "Success", Message = $"{ativo.Nome}" });
                }
                
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao AdicionarAtivo");
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpDelete("{ativoId}")]
        [Authorize(Roles = $"{UserRoles.Admin}")]        
        public async Task<IActionResult> deletarAtivo(int ativoId)
        {

            try
            {
                string userId = User.FindFirstValue("userId");
                Ativo ativo = await _context.Ativos.FindAsync(ativoId);

                // Verificar se o usuário atual possui relação com o ativo na tabela Userxativos
                AtivoxUser ativoxUser = await _context.AtivoxUsers
                    .FirstOrDefaultAsync(au => au.User_id == userId && au.Ativo_id == ativoId);                               

                if (ativo == null || ativoxUser == null)
                {
                    return NotFound(new ResponseDTO { Status = "Error", Message = "Ativo não encontrado ou não autorizado" });
                }

                _context.Ativos.Remove(ativo);
                await _context.SaveChangesAsync();


                return Ok(new ResponseDTO { Status = "Success", Message = "deletado" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao deletarAtivo");
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Route("adicionaruser")]
        public async Task<IActionResult> addUserAoAtivo([FromBody] CriarUserxAtivoDTO dto)
        {
            try
            {
                Ativo ativo = await _context.Ativos.FindAsync(dto.ativoId);
                if (ativo == null)
                {
                    return NotFound(new ResponseDTO { Status = "Error", Message = "Ativo não encontrado" });
                }

                var _ativoId = ativo.Id;
                List<AtivoxUser> novosAtivoxUsers = new List<AtivoxUser>();

                foreach (var email in dto.emails)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    var userId = user?.Id;

                    if (userId != null)
                    {
                        // Verificar se o usuário atual possui relação com o ativo na tabela Userxativos
                        AtivoxUser ativoxUser = await _context.AtivoxUsers
                            .FirstOrDefaultAsync(au => au.User_id == userId && au.Ativo_id == _ativoId);

                        if (ativoxUser == null)
                        {
                            novosAtivoxUsers.Add(new AtivoxUser
                            {
                                User_id = userId,
                                Ativo_id = ativo.Id,
                                Ativo = ativo
                            });
                        }
                    }
                }

                if(novosAtivoxUsers.Count == 0)
                {
                    return NotFound(new ResponseDTO { Status = "Error", Message = "Usuarios não encontrados, Ativo inexistente ou Usuarios ja tem esse ativo" });
                }
                               
                _context.AtivoxUsers.AddRange(novosAtivoxUsers);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { Status = "Success", Message = "Adicionado com Sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao inserir addUserAoAtivo");
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        

    }
}
