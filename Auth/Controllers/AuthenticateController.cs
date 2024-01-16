using Auth.DbContext;
using Auth.DTO;
using Auth.DTO.Response;
using Auth.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRoles> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AtivoController> _logger;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRoles> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<AtivoController> logger
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        private async Task<int> GetTotalAchadosUserxAdmin(String userAdminId)
        {
            var totalRecords = await _context.UserxUsers
                   .Where(x => x.User_Admin_Id == userAdminId)
                   .CountAsync();

            return totalRecords;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user != null && user.IsActive == true)
            {
                // Verifica se o usuário está bloqueado
                if (await _userManager.IsLockedOutAsync(user))
                {
                    
                    return Unauthorized("Account bloqueado por varias tentativas, aguarde um tempo até efetuar novas tentativas");
                }
                                
                if (await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    // Se a senha estiver correta, redefina as tentativas de login malsucedidas
                    await _userManager.ResetAccessFailedCountAsync(user);

                    var userRoles = await _userManager.GetRolesAsync(user);
                    var securityStamp = await _userManager.GetSecurityStampAsync(user);
                    var FirstName = user.Name;

                    var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("userId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("securityStamp", user.SecurityStamp)
            };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var token = GetToken(authClaims);

                    HttpContext.Response.Cookies.Append("X-Access-Token", new JwtSecurityTokenHandler().WriteToken(token),
                        new CookieOptions
                        {
                            Expires = DateTime.Now.AddMinutes(120),
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true,
                            SameSite = SameSiteMode.None
                        });

                    return Ok(new
                    {
                        message = "Login successful",
                        name = FirstName,
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                    });
                }
                else
                {
                    // Se a senha estiver incorreta, registre a tentativa malsucedida
                    await _userManager.AccessFailedAsync(user);

                    // Verifica se o usuário está bloqueado após a tentativa malsucedida
                    if (await _userManager.IsLockedOutAsync(user))
                    {                        
                        return Unauthorized("Account bloqueado por varias tentativas, aguarde um tempo até efetuar novas tentativas");
                    }
                }
            }

            return Unauthorized("email ou senha incorretas");
        }

        [HttpPost]
        [Route("forgotpassword")]
        public async Task<IActionResult> Forgopassword([FromBody] ForgotPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {                
                return NotFound("Usuário não encontrado");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // criar função que envia email e enviar massage costumizada
            return Ok(new { Status = "Success", Message = "Email enviado com sucesso" , Token = $"{token}" , email =$"{dto.Email}"});
        }

        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            var token = dto.Token?.Replace(" ", "+");                       

            if (user == null){
                
                return NotFound("Usuário não encontrado, email errado");
            }
           
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (result.Succeeded)
            {

                var loginDTO = new LoginDTO { Email = dto.Email, Password = dto.NewPassword };
                return await Login(loginDTO);
            }
            else
            {
                
                return BadRequest("Falha ao redefinir a senha");
            }
        }


        [HttpPost]
        [Route("register")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
          
            try
            {
                ApplicationUser user = new()
                {
                    Email = dto.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    PhoneNumber = dto.PhoneNumber,
                    Cpf = dto.Cpf,
                    CodigoUnico = dto.CodigoUnico,
                    UserName = Guid.NewGuid().ToString(),
                };
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    var errorMessage = string.Join(", ", errors);

                    return BadRequest(new ResponseDTO { Status = "Error", Message = $"{errorMessage}" });
                }
                if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                    await _roleManager.CreateAsync(new ApplicationRoles(UserRoles.User));

                if (await _roleManager.RoleExistsAsync(UserRoles.User))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.User);
                }

                UserxUser uxu = new()
                {
                    User_Admin_Id = User.FindFirstValue("userId"),
                    User_Agregado_Id = user.Id,
                };

                _context.UserxUsers.Add(uxu);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { Status = "Success", Message = "User created successfully!" });
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
                throw;
            }
        }

       // [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                Cpf = dto.Cpf,
                CodigoUnico = dto.CodigoUnico,
                UserName = Guid.NewGuid().ToString(),
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new ApplicationRoles(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new ApplicationRoles(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return Ok(new ResponseDTO { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(Roles = UserRoles.User)]        
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {

            var email = User.Identity?.Name;            

            if (!string.IsNullOrEmpty(email))
            {
                // Alterar o SecurityStamp
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }            
            }

            Response.Cookies.Delete("X-Access-Token");
            return Ok(new ResponseDTO { Status = "Success", Message = "Logout successful" });


        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("deactivate/{emailId}")]
        public async Task<IActionResult> DeactivateUser(string emailId)
        {
            var user = await _userManager.FindByEmailAsync(emailId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.IsActive = false; 
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { message = "User deactivated successfully" });
            }
            else
            {
                // Se ocorrerem erros durante a atualização, retorne mensagens de erro
                return BadRequest(new { errors = result.Errors });
            }
        }
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("addroles")]        
        public async Task<IActionResult> addRoles(AddRolesDTO dto)
        {

            try
            {
                List<UserAdminRolescontrol> novosUserAdminRolescontrol = new List<UserAdminRolescontrol>();
                foreach (var onlyRole in dto.roles)

                {
                    var novaRoleCheck = _context.UserAdminRolescontrols
                        .FirstOrDefault(r => r.NameRoleVisual == onlyRole && r.UserId == User.FindFirstValue("userId"));
                    if (novaRoleCheck != null)
                    {
                        continue;
                    }

                    var rolesInserida = await _roleManager.CreateAsync(new ApplicationRoles(onlyRole));


                    if (rolesInserida != null)
                    {
                        var novaRole = await _roleManager.FindByNameAsync(onlyRole);

                        novosUserAdminRolescontrol.Add(new UserAdminRolescontrol
                        {
                            UserId = User.FindFirstValue("userId"),
                            RoleId = novaRole.Id,
                            NameRoleVisual = onlyRole
                        });
                    }
                }

                if (novosUserAdminRolescontrol.Count > 0)
                {

                    _context.UserAdminRolescontrols.AddRange(novosUserAdminRolescontrol);
                    _context.SaveChanges();

                    return Ok(new ResponseDTO { Status = "Sucess", Message = "Funcoes criadas com sucesso" });
                }
                else
                {
                    return BadRequest(new ResponseDTO { Status = "Error", Message = "Funcoes ja existentes ou erro ao inserir" });
                }

            }
            catch (Exception)
            {
                _logger.LogWarning("Erro ao inserir addroles");
                return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possivel de Criar roles" });

            }
        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("removeroles/{rolename}")]
        public async Task<IActionResult> removeRoles(string rolename)
         {
             try
             {
                 var findRole = await _roleManager.FindByNameAsync(rolename);

                 if (findRole == null)
                     return NotFound(new ResponseDTO { Status = "Error", Message = "Role nao encontrada." });

                 var userIdFromRequest = User.FindFirstValue("userId");

                 var userRoleControl = await _context.UserAdminRolescontrols
                     .FirstOrDefaultAsync(x => x.RoleId == findRole.Id && x.UserId == userIdFromRequest);

                 if (userRoleControl == null)
                     return NotFound(new ResponseDTO { Status = "Error", Message = "Você não tem permissão para remover esta Role." });

                 var deletaRoles = await _roleManager.FindByIdAsync(userRoleControl.RoleId);
                 var Roledeletada = deletaRoles != null ? await _roleManager.DeleteAsync(deletaRoles) : null;

                 return Roledeletada?.Succeeded == true
                     ? Ok(new ResponseDTO { Status = "Success", Message = "Funcao deletada com sucesso." })
                     : BadRequest(new ResponseDTO { Status = "Error", Message = "Erro ao deletar a Funcao." });
             }
             catch (Exception)
             {

                 _logger.LogWarning("Erro ao inserir removeRoles");
                 return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possivel remover roles" });
             }
         }


        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("listaroles")]
        public async Task<IActionResult> listaRoles()
        {
            try
            {                
                var roles = await _context.Roles
                    .ToListAsync();

                if (roles != null && roles.Any())
                {
                    var roleList = roles.Select(role => new
                    {
                        roleId = role.Id,
                        roleName = role.Name
                    });

                    return Ok(roleList);
                }

                return NotFound(new ResponseDTO { Status = "Error", Message = "Nenhuma Role encontrada para o usuario especifico." });
            }
            catch (Exception)
            {

                _logger.LogWarning("Erro ao inserir listaRoles");
                return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possivel listar roles" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("deletaruserxuser/{emailUser}")]
        public async Task<IActionResult> deletarUeserXuser(string emailUser)
        {
            try
            {
                var userBuscado = await _userManager.FindByEmailAsync(emailUser);

                if (userBuscado != null)
                {
                    var userAdminId = User.FindFirstValue("userId");

                    var userxuser = await _context.UserxUsers
                        .Where(x => x.User_Admin_Id == userAdminId && x.User_Agregado_Id == userBuscado.Id.ToString())
                        .ToListAsync();

                    if (userxuser != null && userxuser.Any())
                    {
                        _context.UserxUsers.RemoveRange(userxuser);
                        userBuscado.IsActive = false;
                        await _userManager.UpdateAsync(userBuscado);
                        await _context.SaveChangesAsync();


                        return Ok(new ResponseDTO { Status = "Success", Message = "Removido com sucesso." });
                    }
                }

                return NotFound(new ResponseDTO { Status = "Error", Message = "Nenhuma associação UserxUser encontrada para o usuário especificado." });
            }
            catch (Exception)
            {

                _logger.LogWarning("Erro ao deletar deletarUeserXuser");
                return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possivel deletar" });
            }
        }

        [HttpGet]        
        [Authorize(Roles = UserRoles.Admin)]
        [Route("pegaruserxuser")]
        public async Task<IActionResult> PegarUserxUser([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var userAdminId = User.FindFirstValue("userId");
                var totalRecords = await GetTotalAchadosUserxAdmin(userAdminId);

                var userxusers = await _context.UserxUsers
                    .Where(x => x.User_Admin_Id == userAdminId)
                    .OrderByDescending(x => x.Data_criacao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userXUserList = new List<UserXUserResponseDTO>();

                foreach (var userxuser in userxusers)
                {
                    var user = await _userManager.FindByIdAsync(userxuser.User_Agregado_Id);

                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        var userXUserDTO = new UserXUserResponseDTO(
                            userxuser.Id,
                            user.Id,
                            user.Email,
                            user.Name,
                            user.PhoneNumber,
                            user.Cpf,
                            user.CodigoUnico,
                            roles.ToList(),
                            totalRecords,
                            (int)Math.Ceiling((double)totalRecords / pageSize)
                        );

                        userXUserList.Add(userXUserDTO);
                    }
                }

                return Ok(userXUserList);
            }
            catch (Exception)
            {
                _logger.LogWarning("Erro ao pegarUserxUser");
                return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possível obter os dados de UserxUser." });
            }
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("associarole")]
        public async Task<IActionResult> associaRoles([FromBody] AssociaRolesDTO dto)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(dto.RoleName);
                var user = await _context.Users.FindAsync(dto.UserId);
                var userAdminId = User.FindFirstValue("userId");
                              

                if (role != null && user != null)
                {
                    var userxUserAssociation = await _context.UserxUsers
                         .FirstOrDefaultAsync(u => u.User_Admin_Id == userAdminId && u.User_Agregado_Id == user.Id);

                    var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

                    if (userRole == null && userxUserAssociation != null)
                    {

                        await _context.UserRoles.AddAsync(new IdentityUserRole<string>
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });

                        await _context.SaveChangesAsync();

                        return Ok(new ResponseDTO { Status = "Success", Message = "Role associada com sucesso." });
                    }
                    else
                    {
                        return BadRequest(new ResponseDTO { Status = "Info", Message = "O usuario ja tem essa role associada ou voce nao tem permissao" });
                    }
                }

                return NotFound(new ResponseDTO { Status = "Error", Message = "Role ou usuário não encontrado." });
            }
            catch (Exception)
            {

                _logger.LogWarning("Erro ao associaRoles");
                return NotFound(new ResponseDTO { Status = "Error", Message = "Não foi possivel associar funcoes" });
            }
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("desssacociarroles")]
        public async Task<IActionResult> desssacociarRoles([FromBody] AssociaRolesDTO dto)
        {
            var role = await _roleManager.FindByNameAsync(dto.RoleName);
            var user = await _context.Users.FindAsync(dto.UserId);
            var userAdminId = User.FindFirstValue("userId");
                                             

            if (role != null && user != null)
            {
                var userxUserAssociation = await _context.UserxUsers
                    .FirstOrDefaultAsync(u => u.User_Admin_Id == userAdminId && u.User_Agregado_Id == user.Id);

                var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

                if (userRole != null && userxUserAssociation != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync();

                    return Ok(new ResponseDTO { Status = "Success", Message = "Role dissociada com sucesso." });
                }
            }

            return NotFound(new ResponseDTO { Status = "Error", Message = "Role ou usuário não encontrado." });
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("autualizaoutrouser")]
        public async Task<IActionResult> autualizaOutroUser([FromBody] AtualizaOutroUserDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.CodigoUnico == dto.CodigoUnico);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var errorMessages = new List<string>();

                var existingUserWithSameEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != user.Id);
                if (existingUserWithSameEmail != null)
                {
                    errorMessages.Add("E-mail já cadastrado por outro usuário.");
                }

                var existingUserWithSamePhoneNumber = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber && u.Id != user.Id);
                if (existingUserWithSamePhoneNumber != null)
                {
                    errorMessages.Add("Número de telefone já cadastrado por outro usuário.");
                }

                var existingUserWithSameCpf = await _context.Users
                    .FirstOrDefaultAsync(u => u.Cpf == dto.Cpf && u.Id != user.Id);
                if (existingUserWithSameCpf != null)
                {
                    errorMessages.Add("CPF já cadastrado por outro usuário.");
                }

                if(errorMessages.Count > 0)
                {
                    return BadRequest(errorMessages);
                }

                if (!user.Equals(dto))
                {
                    _context.Entry(user).CurrentValues.SetValues(dto);
                    user.NormalizedEmail = dto.Email.Normalize().ToUpperInvariant();
                    await _context.SaveChangesAsync();

                    return Ok(user);
                }

                return BadRequest("Os dados enviados são iguais aos dados existentes.");
            }
            catch (DbUpdateConcurrencyException)
            {
                
                return StatusCode(500, "Erro ao atualizar o usuário");
            }
        }
               

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("getuserbycodigounico/{codUnico}")]
        public async Task<IActionResult> GetUserByCodigoUnico([FromRoute][Required] string codUnico)
        {
            var userAchado = await _context.Users
                    .FirstOrDefaultAsync(u => u.CodigoUnico == codUnico);

            if (userAchado != null)
            {
                UserFindForEditResponseDTO userEnviar = new UserFindForEditResponseDTO();

                userEnviar.PhoneNumber = userAchado.PhoneNumber;
                userEnviar.Cpf = userAchado.Cpf;
                userEnviar.Name = userAchado.Name;
                userEnviar.Email = userAchado.Email;                
                userEnviar.CodigoUnico = userAchado.CodigoUnico;                

                return Ok(userEnviar);
            }
            else
            {
                return NotFound();
            }

        }

    }
}
