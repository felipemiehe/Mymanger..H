using Auth.DTO;
using Auth.Entities;
using front.Helpers;
using front.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Text;

namespace front.Controllers
{
    public class UserxUserController: Controller
    {


        private List<UserXUserViewModel> ObterListaUserXUser(int page, int pageSize, string? roleFIlter = null, string? cpf = null, string? email = null, string? codigoUnico = null, string? nome = null)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress 
                +$"api/auth/pegaruserxuser?page={page}&pageSize={pageSize}&roleFilter={roleFIlter}&cpf={cpf}&email={email}&codigoUnico={codigoUnico}&nome={nome}")
                .Result;

            if (response.IsSuccessStatusCode)
            {
                string content = response.Content.ReadAsStringAsync().Result;
                List<UserXUserViewModel> userList = JsonConvert.DeserializeObject<List<UserXUserViewModel>>(content);
                if(userList != null)
                {
                    return userList;
                }
                
            }

            return new List<UserXUserViewModel>();
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("UserxUser")]
        public IActionResult Index(int page = 1, int pageSize = 5, string? roleFIlter = null, string? cpf = null, string? email = null, string? codigoUnico = null, string? nome = null)
        {
            List<UserXUserViewModel> userList = ObterListaUserXUser(page, pageSize, roleFIlter, cpf, email, codigoUnico, nome);

            if (userList != null && userList.Count() >= 1)
            {                
                return View(userList);
            }
            else
            {                
                return View(ObterListaUserXUser(1, 5));
            }
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser(RegisterUserViewModel model)
        {
            string data = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.PostAsync(configuredClient.BaseAddress + "api/auth/register", content).Result;

            if (!response.IsSuccessStatusCode)
            {

                string Json = response.Content.ReadAsStringAsync().Result;
                var responseObject = JsonConvert.DeserializeObject<ResponseDTO>(Json);                
                if (responseObject != null && responseObject.Message != null)
                {
                    ModelState.AddModelError(string.Empty, responseObject.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erro inesperado ou sem permição");
                }
                return View("Index", new List<UserXUserViewModel>(ObterListaUserXUser(1, 5)));
            }           

            TempData["Sucesso"] = "Usuario adicionado com sucesso.";

            return View("Index", new List<UserXUserViewModel>(ObterListaUserXUser(1, 5)));
        }

        [HttpGet]
        public IActionResult GetUserList(int page = 1, int pageSize = 5, string? roleFIlter = null, string? cpf = null, string? email = null, string? codigoUnico = null, string? nome = null)
        {
            var userList = ObterListaUserXUser(page, pageSize, roleFIlter, cpf, email, codigoUnico, nome);  
            if(userList != null && userList.Count > 0 || roleFIlter != null || cpf != null || email != null || codigoUnico!= null || nome != null )
            {
                return PartialView("_UserxUserPartial", userList);
            }

            return PartialView("_UserxUserPartial",  ObterListaUserXUser(1, 5));
        }                
     


        [HttpGet]
        public async Task<IActionResult> getcodigounico(string codigounico)
        {

            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress + $"api/auth/getuserbycodigounico/{codigounico}").Result;


            string userJson = await response.Content.ReadAsStringAsync();
            AtualizaOutroUserModel userModel = JsonConvert.DeserializeObject<AtualizaOutroUserModel>(userJson);
            
            return PartialView("_AtualizaOutroUserPartial", userModel);
        }



        [HttpPost]
        public async Task<IActionResult> atualizaruserxuser(AtualizaOutroUserModel model)
        {
            if (ModelState.IsValid)
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;

                HttpResponseMessage response = await configuredClient.PutAsync(configuredClient.BaseAddress + "api/auth/autualizaoutrouser", content);

                if (response.IsSuccessStatusCode)
                {
                    string newUser = await response.Content.ReadAsStringAsync();
                    AtualizaOutroUserModel userModel = JsonConvert.DeserializeObject<AtualizaOutroUserModel>(newUser);

                    return Json(new { success = true });
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();

                    List<string> errorMessages = JsonConvert.DeserializeObject<List<string>>(errorMessage);
                    foreach (var error in errorMessages)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }                    
                    return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AtualizaOutroUserPartial", model) });
                }
            }

            return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AtualizaOutroUserPartial", model) });
        }

        [HttpPost("deletauserxuser")]
        public async Task<IActionResult> deletauserxuser(string email)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = await configuredClient.DeleteAsync(configuredClient.BaseAddress + $"api/auth/deletaruserxuser/{email}");

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Usuario excluido com sucesso!"});

            }
            return Json(new { success = false, message = "erro ao excluir!" });

        }

        [HttpPost("associaRoles")]
        public async Task<IActionResult> associaRoles(AssociaRolesModel model)
        {
            ViewData["requestForm"] = "associaRoles";
            if (ModelState.IsValid)
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
                HttpResponseMessage response = await configuredClient.PostAsync(configuredClient.BaseAddress + "api/auth/associarole", content);


                ResponseDTO Response = JsonConvert.DeserializeObject<ResponseDTO>(await response.Content.ReadAsStringAsync());
                if (response.IsSuccessStatusCode)              
                {
                    return Json(new { success = true, message = Response?.Message ?? "Sucesso Ao adicionar Função" });
                }

                ModelState.AddModelError(string.Empty, Response?.Message ?? "Erro inesperado!");

                return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AssociaRolesPartial", model) });
            }

            return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AssociaRolesPartial", model) });

        }
        
        [HttpPost("removerRolesUser")]
        public async Task<IActionResult> removerRolesUser(AssociaRolesModel model)
        {

            if (ModelState.IsValid)
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
                HttpResponseMessage response = await configuredClient.PutAsync(configuredClient.BaseAddress + "api/auth/desssacociarroles", content);


                ResponseDTO Response = JsonConvert.DeserializeObject<ResponseDTO>(await response.Content.ReadAsStringAsync());
                if (response.IsSuccessStatusCode)              
                {
                    return Json(new { success = true, message = Response?.Message ?? "Sucesso Ao remover Função" });
                }

                ModelState.AddModelError(string.Empty, Response?.Message ?? "Erro inesperado!");

                return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AssociaRolesPartial", model) });
            }

            return Json(new { success = false, html = Helper.RenderRazorViewToString(this, "_AssociaRolesPartial", model) });

        }


    }
}
