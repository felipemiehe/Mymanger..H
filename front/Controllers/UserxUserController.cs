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


        private List<UserXUserViewModel> ObterListaUserXUser(int page, int pageSize)
        {
            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress + $"api/auth/pegaruserxuser?page={page}&pageSize={pageSize}").Result;

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
        public IActionResult Index(int page = 1, int pageSize = 5)
        {
            List<UserXUserViewModel> userList = ObterListaUserXUser(page, pageSize);

            if (userList != null)
            {                
                return View(userList);
            }
            else
            {                
                return View(new List<UserXUserViewModel>());
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
        public IActionResult GetUserList(int page = 1, int pageSize = 10)
        {
            var userList = ObterListaUserXUser(page, pageSize);
            return PartialView("_UserxUserPartial", userList);
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

                    return Json(new { success = true, html = Helper.RenderRazorViewToString(this, "_AtualizaOutroUserPartial", model) });
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


    }
}
