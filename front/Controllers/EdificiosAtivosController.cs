using Auth.Entities;
using front.Helpers;
using front.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace front.Controllers
{
    public class EdificiosAtivosController : Controller
    {
        private List<EdifioxUserModel> ObterListaEdificiosXUser(int page, int pageSize, string? emailResponsavel = null, string? Endereco = null, string? nome = null, string? CodUnico = null)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress
                + $"api/ativo/pegarativoxuser?page={page}&pageSize={pageSize}&emailResponsavel={emailResponsavel}&Endereco={Endereco}&nome={nome}&CodUnico={CodUnico}")
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
        public IActionResult Index(int page = 1, int pageSize = 5, string? emailResponsavel = null, string? Endereco = null, string? nome = null, string? CodUnico = null)
        {
            List<EdifioxUserModel> edificioList = ObterListaEdificiosXUser(page, pageSize, emailResponsavel, Endereco, nome);

            if (edificioList != null && edificioList.Count() >= 1)
            {
                return View(edificioList);
            }
            else
            {
                return View(ObterListaEdificiosXUser(1, 5));
            }
        }

        [HttpGet]
        public IActionResult GetEdificioList(int page = 1, int pageSize = 5, string? emailResponsavel = null, string? Endereco = null, string? nome = null, string? CodUnico = null)
        {
            List<EdifioxUserModel> edificioList = ObterListaEdificiosXUser(page, pageSize, emailResponsavel, Endereco, nome, CodUnico);
            if (edificioList != null && edificioList.Count > 0 || emailResponsavel != null || Endereco != null || nome!= null || CodUnico != null)
            {
                return PartialView("_EdificioxUserPartial", edificioList);
            }

            return PartialView("_EdificioxUserPartial", ObterListaEdificiosXUser(1, 5));
        }

        [HttpPost("deletaEdificio")]
        public async Task<IActionResult> deletaEdificio(int id)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = await configuredClient.DeleteAsync(configuredClient.BaseAddress + $"api/ativo/{id}");

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Edifcio excluido com sucesso!" });

            }
            return Json(new { success = false, message = "erro ao excluir!" });

        }


        [HttpGet]
        public async Task<IActionResult> getEdificioByCodUnico(string codigounico)
        {

            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress + $"api/ativo/getedificiosbycodigounico/{codigounico}").Result;


            string userJson = await response.Content.ReadAsStringAsync();
            AtualizaEdificiosModel userModel = JsonConvert.DeserializeObject<AtualizaEdificiosModel>(userJson);

            return PartialView("_EdficioAtualizarPartial", userModel);
        }

        [HttpPost]
        public async Task<IActionResult> atualizarEdificios(AtualizaEdificiosModel model)
        {
            if (ModelState.IsValid)
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;

                HttpResponseMessage response = await configuredClient.PutAsync(configuredClient.BaseAddress + "api/ativo/autualizaedificios", content);

                if (response.IsSuccessStatusCode)
                {
                    string newEdificio = await response.Content.ReadAsStringAsync();
                    AtualizaEdificiosModel userModel = JsonConvert.DeserializeObject<AtualizaEdificiosModel>(newEdificio);

                    return Json(new { success = true });
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();

                    List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(errorMessage);
                    foreach (var error in errorMessages)
                    {
                        
                    }
                    return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_EdficioAtualizarPartial", model) });
                }
            }
            
            return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_EdficioAtualizarPartial", model) });
        }
    }
}
