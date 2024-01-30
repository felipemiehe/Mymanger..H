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
using System.ComponentModel.DataAnnotations;
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
                                email_responsavel_criado = dto.Responsavel_email,
                                Ativo = ativo,
                                email_logs_quem_fez = userId,
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

                        if (ativoxUserAdmin != null)
                        {
                            if (ativoxUserAdicionar == null)
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
                            return BadRequest(new ResponseDTO { Status = "Error", Message = "Usuario nao tem acesso a modificações" });
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
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Fiscais}")]
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

                var userxAtivosRecords = await query
                    .OrderByDescending(x => x.Data_criacao)
                    .ToListAsync();
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
                        .Select(responsavel => responsavel.ResponsavelEmail.Email)
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


        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("getedificiosbycodigounico/{codUnico}")]
        public async Task<IActionResult> GetEdificiosByCodigoUnico([FromRoute][Required] string codUnico)
        {

            var userAdminId = User.FindFirstValue("userId");

            var ativoAchado = await _context.Ativos
                    .FirstOrDefaultAsync(u => u.CodigoUnico == codUnico);

            if (ativoAchado != null)
            {
                var userxUserAssociation = await _context.AtivoxUsers
                        .FirstOrDefaultAsync(u => u.User_id == userAdminId && u.Ativo_id == ativoAchado.Id);
                if (userxUserAssociation != null)
                {
                    EdificioFindForEditResponseDTO edificioEnviar = new EdificioFindForEditResponseDTO();

                    return Ok(new EdificioFindForEditResponseDTO(ativoAchado.Endereco, ativoAchado.Nome, ativoAchado.CodigoUnico));
                }
                else
                {
                    return NotFound();
                }

            }

            return NotFound();

        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("autualizaedificios")]
        public async Task<IActionResult> autualizaEdificios([FromBody] AtualizaEdficiosDTO dto)
        {
            try
            {
                var edificio = await _context.Ativos.FirstOrDefaultAsync(u => u.CodigoUnico == dto.CodigoUnico);

                if (edificio == null)
                {
                    return NotFound("Edificio não encontrado");
                }

                var userAdminId = User.FindFirstValue("userId");

                var checkResult = await CheckRelationAtivoxUsers(userAdminId, edificio.Id);
                if (checkResult) 
                    return BadRequest(new ResponseDTO { Status = "error", Message = $"sem permisão para alterar o {edificio.Nome}"});

                if (!edificio.Equals(dto))
                {                    
                    _context.Entry(edificio).CurrentValues.SetValues(dto);                    
                    await _context.SaveChangesAsync();
                    return Ok(new EdificioFindForEditResponseDTO(edificio.Endereco,edificio.Nome,edificio.CodigoUnico));
                }

                return BadRequest(new ResponseDTO { Status = "error", Message = $"Error ao alterar o {edificio.Nome}" });
            }
            catch (Exception ex)
            {
               return CheckSqlNumber2061(ex, "autualizaEdificios");                 
               throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Route("adicionarResponsavel")]
        public async Task<IActionResult> addResponsavelAoAtivo([FromBody] AddResponsavelEdificioDTO dto)
        {
            try
            {
                var userAdminId = User.FindFirstValue("userId");

                // Encontra o responsável pelo email
                var responsavel = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.emailResponsavel);

                if (responsavel == null)
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = "O responsável com o email especificado não foi encontrado." });
                }

                
                var roleNames = new List<string> { UserRoles.Fiscais, UserRoles.Admin, UserRoles.Reporter };              
                var roleIds = await _context.Roles
                    .Where(role => roleNames.Contains(role.Name))
                    .Select(role => role.Id)
                    .ToListAsync();

                if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == responsavel.Id && roleIds.Contains(ur.RoleId)))
                {
                    // O usuário não possui nenhuma das roles
                    return BadRequest(new ResponseDTO { Status = "Error", Message = $"O usuário com o {dto.emailResponsavel} não pode ser responsável." });
                }


                if (!await IsUserAuthorizedToAssociate(userAdminId, responsavel.Id))
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = "Usuário não autorizado para associar o responsável ao ativo." });
                }

                Ativo ativo = await _context.Ativos.FirstOrDefaultAsync(a => a.CodigoUnico == dto.CodigoUnicoAtivo);

                if (ativo == null)
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = "Ativo não encontrado." });
                }

                
                if (!await UserHasRelationWithAsset(userAdminId, ativo.Id))
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = "Usuário não tem acesso para realizar modificações." });
                }

                
                if (await UserHasRelationWithAsset(responsavel.Id, ativo.Id))
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = $"{dto.emailResponsavel} já está vinculado ao ativo {ativo.Nome}." });
                }

               
                var novoAtivoxUser = new AtivoxUser
                {
                    User_id = responsavel.Id,
                    Ativo_id = ativo.Id,
                    Ativo = ativo
                };
                
                _context.AtivoxUsers.Add(novoAtivoxUser);

                
                
                ativo.Responsaveis.Add(new ListResponsaveisAtivos
                {
                    email_responsavel_criado = dto.emailResponsavel,
                    Ativo = ativo,
                    email_logs_quem_fez = userAdminId,
                    ResponsavelEmail = responsavel
                });
                
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { Status = "Success", Message = "Responsável adicionado com sucesso ao ativo." });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao inserir adicionarResponsavel: " + ex.Message);
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }



        private async Task<bool> CheckRelationAtivoxUsers(String idRequisição, int IdEdificio)
        {
            var userxUserAssociation = await _context.AtivoxUsers
                       .FirstOrDefaultAsync(u => u.User_id == idRequisição && u.Ativo_id == IdEdificio);            
            if(userxUserAssociation == null)
            {
                return true;
            }

            return false;
        }

        private IActionResult CheckSqlNumber2061(Exception ex, string NomeController)
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
            _logger.LogWarning($"Erro ao {NomeController}");
            return StatusCode(500, new ResponseDTO { Status = "Error", Message = "Erro inesperado" });
        }

        // Esta função verifica se o usuário atual tem permissão para associar o usuario passado
        private async Task<bool> IsUserAuthorizedToAssociate(string userId, string responsibleUserId)
        {
            return await _context.UserxUsers
                .AnyAsync(u => u.User_Admin_Id == userId && u.User_Agregado_Id == responsibleUserId);
        }

        // verificar se ele ja esta enserido na tabela
        private async Task<bool> UserHasRelationWithAsset(string userId, int assetId)
        {
            return await _context.AtivoxUsers
                .AnyAsync(au => au.User_id == userId && au.Ativo_id == assetId);
        }



    }
}
