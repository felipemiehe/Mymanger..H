using Auth;
using front.Helpers;
using front.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace front.Controllers
{
    public class LoginController : Controller
    {

        private readonly HttpClient _client;
        Uri baseUrl = new Uri("https://localhost:7273");

        public LoginController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseUrl;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult listatempoe()

        {

            HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
            HttpResponseMessage response = configuredClient.GetAsync(configuredClient.BaseAddress + "weatherforecast").Result;

            if (response.IsSuccessStatusCode)
            {
                string responseData = response.Content.ReadAsStringAsync().Result;
                List<WeatherForecast> weatherForecasts = new List<WeatherForecast>();

                weatherForecasts = JsonConvert.DeserializeObject<List<WeatherForecast>>(responseData);

                return View(weatherForecasts);
            }
            else
            {

                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]        
        public IActionResult Index()
        {

            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "api/auth/login", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseData = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseData, new
                    {
                        message = "",
                        name = "",
                        token = ""
                    });

                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(responseObject?.token) as JwtSecurityToken;

                    if (!string.IsNullOrEmpty(responseObject?.token))
                    {
                        // Obtenha as claims do token JWT, incluindo as roles
                        var claims = jsonToken?.Claims;

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            // Defina o tempo de expiração do cookie
                            ExpiresUtc = DateTime.Now.AddMinutes(90),                            
                            IsPersistent = true, 
                            AllowRefresh = true 
                        };

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

                        HttpContext.Response.Cookies.Append("X-Access-Token", responseObject.token, new CookieOptions
                        {
                            Expires = DateTime.Now.AddMinutes(90),
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true,
                            SameSite = SameSiteMode.None
                        });

                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string errorMessage = response.Content.ReadAsStringAsync().Result;
                    return Json(new { success = false, errorMessage = errorMessage });
                }

                return Json(new { success = false, errorMessage = "An unexpected error occurred" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                HttpClient configuredClient = new ClienteComCookie(Request).ConfiguredClient;
                HttpResponseMessage response = configuredClient.PostAsync(configuredClient.BaseAddress + "api/auth/logout", null).Result;

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    // Remova o cookie personalizado (X-Access-Token)
                    HttpContext.Response.Cookies.Delete("X-Access-Token");

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Login") });

                }
                else
                {
                    return Json(new { success = false, errorMessage = "Erro ao efetuar logout", redirectUrl = Url.Action("Index", "Login") });

                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {

            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            string data = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "api/auth/forgotpassword", content).Result;

            if (response.IsSuccessStatusCode)
            {

                string SucessMessage = response.Content.ReadAsStringAsync().Result;
                return Json(new { success = true, successMessage = SucessMessage });
            }

            return Json(new { success = false, errorMessage = response.Content.ReadAsStringAsync().Result});
        }

        [HttpGet]
        public IActionResult ResetPassword(string token,string email)
        {
            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            string data = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "api/auth/resetpassword", content).Result;

            if (response.IsSuccessStatusCode)
            {

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

            }

            return Json(new { success = false, errorMessage = response.Content.ReadAsStringAsync().Result });

        }

    }
}
