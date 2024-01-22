using Auth.DbContext;
using Auth.DTO;
using Auth.DTO.Response;
using Auth.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                    var ativo = new Ativo
                    {
                        Nome = dto.Nome,
                        Endereco = dto.Endereco,
                        NumeroAptos = dto.NumeroAptos,
                        CodigoUnico = dto.CodigoUnico
                    };

                    // Verifica se o e-mail do responsável existe nos cadastros
                    var check_email = await _userManager.FindByEmailAsync(dto.Responsavel_email);

                    if (check_email == null)
                    {
                        return BadRequest(new ResponseDTO { Status = "Error", Message = "Email tem que existir nos cadastros" });
                    }

                    string userId = User.FindFirstValue("userId");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Verifica se o usuário atual tem permissão para associar o ativo ao responsável
                        var userxUserEntry = await _context.UserxUsers
                            .FirstOrDefaultAsync(u => u.User_Admin_Id == userId && u.User_Agregado_Id == check_email.Id);

                        var responsavel = await _userManager.FindByEmailAsync(dto.Responsavel_email);

                        if (userxUserEntry != null)
                        {
                            // Adiciona o responsável à lista de responsáveis do ativo
                            ativo.Responsaveis.Add(new ListResponsaveisAtivos
                            {
                                email_responsavel = dto.Responsavel_email,                                
                                Ativo = ativo,
                                ResponsavelEmail = responsavel
                            });

                            _context.Ativos.Add(ativo);

                            // Associa o ativo ao usuário que o criou
                            AtivoxUser ativoxUser = new AtivoxUser
                            {
                                User_id = userId,
                                Ativo_id = ativo.Id,
                                Ativo = ativo,
                            };
                            _context.AtivoxUsers.Add(ativoxUser);

                            // Associa edificio ao resposavel
                            var ativoxUserEntry = new AtivoxUser
                            {
                                User_id = check_email.Id,
                                Ativo_id = ativo.Id,
                                Ativo = ativo,
                            };
                            _context.AtivoxUsers.Add(ativoxUserEntry);

                            // Salva as mudanças no banco de dados
                            await _context.SaveChangesAsync();

                            return Ok(new ResponseDTO { Status = "Success", Message = $"{ativo.Nome}" });
                        }
                    }
                }

                return BadRequest(new ResponseDTO { Status = "Error", Message = $"{dto.Responsavel_email} não cadastrado na sua lista" });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    // Número 2601 é a exceção específica para violação de restrição única no SQL Server
                    var errorMessage = sqlException.Message;

                    // Extrai o valor duplicado da mensagem de erro
                    var startIndex = errorMessage.IndexOf("(") + 1;
                    var endIndex = errorMessage.IndexOf(")");
                    var valorDuplicado = errorMessage.Substring(startIndex, endIndex - startIndex);

                    return BadRequest(new ResponseDTO { Status = "Error", Message = $"O valor '{valorDuplicado}' já está em uso." });
                }

                _logger.LogWarning("Erro ao AdicionarAtivo");
                throw;
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
                var userAdminId = User.FindFirstValue("userId");

                foreach (var email in dto.emails)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    var userAdiconarId = user?.Id;                    

                    if (userAdminId != null && userAdiconarId != null)
                    {
                        // Verificar se o usuário admin tem relação com o tabela
                        AtivoxUser ativoxUserAdmin = await _context.AtivoxUsers
                            .FirstOrDefaultAsync(au => au.User_id == userAdminId && au.Ativo_id == _ativoId);
                        // Verificar se o usuário admin tem relação com o tabela
                        AtivoxUser ativoxUserAdicionar = await _context.AtivoxUsers
                            .FirstOrDefaultAsync(au => au.User_id == userAdiconarId && au.Ativo_id == _ativoId);

                        if (ativoxUserAdmin != null )
                        {
                            if(ativoxUserAdicionar == null)
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
                                novosAtivoxUsers.Clear();
                                return BadRequest(new ResponseDTO { Status = "Error", Message = $"{email} ja tem vinculo com Edificio {ativoxUserAdicionar.Ativo.Nome}" });
                            }                        
                        }
                        else
                        {
                            return BadRequest(new ResponseDTO { Status = "Error", Message = "Usuario nao tem acesso a modificações"});
                        }
                    }
                }
                                               
                _context.AtivoxUsers.AddRange(novosAtivoxUsers);
                await _context.SaveChangesAsync();                

                return Ok(new ResponseDTO { Status = "Success", Message = "Usuarios vinculados com sucesso" });
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
        public async Task<IActionResult> pegarativoxuser(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5, 
            [FromQuery] string? emailResponsavel = "",
            [FromQuery] string? Endereco = "",
            [FromQuery] string? nome = "",
            [FromQuery] string? CodUnico = ""
            )
        {
            try
            {
                var userAdminId = User.FindFirstValue("userId");


                // pegar apenas ativos com Id da requisição
                var query = _context.Ativos
                    .Include(a => a.AtivoxUsers)
                    .Include(a => a.Responsaveis)
                        .ThenInclude(r => r.ResponsavelEmail)
                    .Where(ativo => ativo.AtivoxUsers.Any(au => au.User_id == userAdminId));

                // Filtros
                if (!string.IsNullOrEmpty(emailResponsavel) || !string.IsNullOrEmpty(Endereco) || !string.IsNullOrEmpty(nome) || !string.IsNullOrEmpty(CodUnico))
                {
                    query = query
                        .Where(uu =>
                            (string.IsNullOrEmpty(emailResponsavel) || uu.Responsaveis.Any(responsavel =>
                                 responsavel.ResponsavelEmail.Email == emailResponsavel)) &&
                            (string.IsNullOrEmpty(CodUnico) || uu.CodigoUnico.Contains(CodUnico)) &&
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

                var ativosXUserList = paginatedUsersxativos.Select(userxAtivo => new UserxAtivosEdificiosResponseDTO(
                    userxAtivo.Id,
                    userxAtivo.Nome,
                    userxAtivo.Endereco,
                    userxAtivo.NumeroAptos,
                    userxAtivo.Responsaveis
                        .Where(responsavel => responsavel.ResponsavelEmail != null && responsavel.ResponsavelEmail.IsActive)
                        .Select(responsavel => responsavel.email_responsavel)
                        .ToList(),
                    userxAtivo.CodigoUnico,
                    totalRecords,
                    (int)Math.Ceiling((double)totalRecords / pageSize)
                )).ToList();

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
