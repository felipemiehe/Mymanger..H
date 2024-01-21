using Auth.DbContext;
using Auth.DTO;
using Auth.DTO.Response;
using Auth.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
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
                    ativo.Endereco = dto.Endereco;
                    ativo.NumeroAptos = dto.NumeroAptos;
                    ativo.Responsavel_email = dto.Responsavel_email;

                    var check_email =  await _userManager.FindByEmailAsync(dto.Responsavel_email);

                    if (check_email == null) {
                        return BadRequest(new ResponseDTO { Status = "Error", Message = "Email tem que existir nos cadastros" });
                    }

                    string userId = User.FindFirstValue("userId");
                    if (userId.Any())
                    {
                        var userxUserEntry = await _context.UserxUsers
                            .FirstOrDefaultAsync(u => u.User_Admin_Id == userId && u.User_Agregado_Id == check_email.Id);
                        if (userxUserEntry != null)
                        {
                            _context.Ativos.Add(ativo);

                            // Associar o Ativo ao usuário que o criou
                            AtivoxUser ativoxUser = new AtivoxUser
                            {
                                User_id = userId,
                                Ativo_id = ativo.Id,
                                Ativo = ativo,

                            };

                            _context.AtivoxUsers.Add(ativoxUser);

                            // Salva as mudanças no banco de dados
                            await _context.SaveChangesAsync();

                            return Ok(new ResponseDTO { Status = "Success", Message = $"{ativo.Nome}" });
                        }
                    }
                }
                
                return BadRequest(new ResponseDTO { Status = "Error", Message = "Erro na requisição ou usuario não cadastrado" });
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
                    var userAdiconarId = user?.Id;
                    var userAdminId = User.FindFirstValue("userId");

                    if (userAdminId != null && userAdiconarId != null)
                    {
                        // Verificar se o usuário admin tem relação com o tabela
                        AtivoxUser ativoxUserAdmin = await _context.AtivoxUsers
                            .FirstOrDefaultAsync(au => au.User_id == userAdminId && au.Ativo_id == _ativoId);
                        // Verificar se o usuário admin tem relação com o tabela
                        AtivoxUser ativoxUserAdicionar = await _context.AtivoxUsers
                            .FirstOrDefaultAsync(au => au.User_id == userAdiconarId && au.Ativo_id == _ativoId);

                        if (ativoxUserAdmin != null && ativoxUserAdicionar == null)
                        {
                            novosAtivoxUsers.Add(new AtivoxUser
                            {
                                User_id = userAdiconarId,
                                Ativo_id = ativo.Id,
                                Ativo = ativo
                            });
                        }
                        else
                        {
                            return NotFound(new ResponseDTO { Status = "Error", Message = "Usuario nao tem acesso a modificações" });
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

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("pegarativoxuser")]
        public async Task<IActionResult> PegarUserxUser(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5, 
            [FromQuery] string? emailResponsavel = "",
            [FromQuery] string? Endereco = "",
            [FromQuery] string? nome = ""
            )
        {
            try
            {
                var userAdminId = User.FindFirstValue("userId");


                // pegar apenas ativos com Id da requisição
                var query = _context.Ativos
                    .Include(a => a.AtivoxUsers)
                    .Where(ativo => ativo.AtivoxUsers.Any(au => au.User_id == userAdminId));               
                
                // filtros
                if (!string.IsNullOrEmpty(emailResponsavel) || !string.IsNullOrEmpty(Endereco) || !string.IsNullOrEmpty(nome))
                {
                    query = query
                           .Where(uu =>
                               (string.IsNullOrEmpty(emailResponsavel) || uu.Responsavel_email ==  emailResponsavel) &&
                               (string.IsNullOrEmpty(Endereco) || uu.Endereco.Contains(Endereco)) &&
                               (string.IsNullOrEmpty(nome) || uu.Nome.Contains(nome))                             
                           );
                }

                var userxAtivosRecords = await query.ToListAsync();
                var totalRecords = userxAtivosRecords.Count();

                var paginatedUsersxativos = userxAtivosRecords
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var ativosXUserList = new List<UserxAtivosEdificiosResponseDTO>();

                foreach (var userxAtivo in paginatedUsersxativos)
                {
                    
                        var userXAtivosedficiosDTO = new UserxAtivosEdificiosResponseDTO(
                            userxAtivo.Id,
                            userxAtivo.Nome,
                            userxAtivo.Endereco,
                            userxAtivo.NumeroAptos,
                            userxAtivo.Responsavel_email,                            
                            totalRecords,
                            (int)Math.Ceiling((double)totalRecords / pageSize)
                        );

                       ativosXUserList.Add(userXAtivosedficiosDTO);
                    
                }

                return Ok(ativosXUserList);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Erro ao pegarUserxUser");
                return NotFound(new ResponseDTO { Status = "Error", Message = e.ToString() });
            }
        }



    }
}
