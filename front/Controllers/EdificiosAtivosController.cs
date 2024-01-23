﻿using Auth.Entities;
using front.Helpers;
using front.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

            if (edificioList != null)
            {
                return View(edificioList);
            }
            else
            {
                return View(new List<EdifioxUserModel>());
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
    }
}