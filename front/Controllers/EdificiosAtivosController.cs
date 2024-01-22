using Auth.Entities;
using front.Helpers;
using front.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace front.Controllers
{
    public class EdificiosAtivosController : Controller
    {
        private List<EdifioxUserModel> ObterListaEdificiosXUser(int page, int pageSize, string? emailResponsavel = null, string? Endereco = null, string? nome = null)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress
                + $"api/ativo/pegarativoxuser?page={page}&pageSize={pageSize}&emailResponsavel={emailResponsavel}&Endereco={Endereco}&nome={nome}")
                .Result;

            if (response.IsSuccessStatusCode)
            {
                string content = response.Content.ReadAsStringAsync().Result;
                List<EdifioxUserModel> userList = JsonConvert.DeserializeObject<List<EdifioxUserModel>>(content);
                if (userList != null)
                {
                    return userList;
                }

            }

            return new List<EdifioxUserModel>();
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("EdificiosAtivos")]
        public IActionResult Index(int page = 1, int pageSize = 5, string? emailResponsavel = null, string? Endereco = null, string? nome = null)
        {
            List<EdifioxUserModel> userList = ObterListaEdificiosXUser(page, pageSize, emailResponsavel, Endereco, nome);

            if (userList != null)
            {
                return View(userList);
            }
            else
            {
                return View(new List<EdifioxUserModel>());
            }
        }
    }
}
