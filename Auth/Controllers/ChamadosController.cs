using Auth.DbContext;
using Auth.DTO;
using Auth.Entities;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
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

        public ChamadosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AtivoController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;          
            
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.User},{UserRoles.Admin}")]
        [Route("criarChamado")]
        public async Task<IActionResult> AdicionarChamado([FromBody] AddChamadadosDTO dto)
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
                    Foto_URLs = new List<FotoUrl>(),
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                };

               // var urlImagem = await _blobRepository.UpLoadBlobFile(dto.foto_url, "abacaherh");

                //var novaFotoUrl = new FotoUrl
                //{
                //    Url = urlImagem,
                //    Chamado = chamado ,
                //    Chamado_id = chamado.Id                    
                //};

                //chamado.Foto_URLs.Add(novaFotoUrl);


                _context.Chamados.Add(chamado);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { Status = "Success" , Message= "ta pegando" });                

            }
            catch (Exception ex)
            {
                return HandleSqlException(ex);
                throw;
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadBlob(IFormFile teste)
        {
            if (teste == null || teste.Length == 0)
                return BadRequest("No file uploaded.");

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=pastadasimagens;AccountKey=xuMkuZ+6isPEJ6SnBQ8uoPQxHgtxDhTJXyDkO9LcAQolipPdfrJXR23Us1DPpmLg4j8Mxi+cocwO+AStQL0Sqg==;EndpointSuffix=core.windows.net";
            string blobContainerName = "blolbcontainerimgs";
            string blobName = Guid.NewGuid().ToString() + "--|&&|" + teste.FileName;

            try
            {
                // Criar um BlobServiceClient
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Obter uma referência para o container
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);

                // Criar o container se ele não existir
                await containerClient.CreateIfNotExistsAsync();

                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Determinar o tipo de conteúdo do arquivo automaticamente
                string contentType = MimeUtility.GetMimeMapping(blobName);

                BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                // Upload do arquivo para o blob
                using (Stream stream = teste.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, blobHttpHeaders);
                }

                return Ok($"Blob {blobName} uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

        private IActionResult HandleSqlException(Exception ex)
        {
            if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
            {
                // Número 2601 é a exceção específica para violação de restrição única no SQL Server
                var errorMessage = sqlException.Message;

                // Extrai o valor duplicado da mensagem de erro
                var startIndex = errorMessage.IndexOf("(") + 1;
                var endIndex = errorMessage.IndexOf(")");
                var valorDuplicado = errorMessage.Substring(startIndex, endIndex - startIndex);

                return new BadRequestObjectResult(new ResponseDTO { Status = "Error", Message = $"O valor '{valorDuplicado}' já está em uso." });
            }
            return StatusCode(500, ex);
        }


    }
}
