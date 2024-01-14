using Auth.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace front.Helpers
{
    public class JwtCookieValidationMiddleware
    {
        private readonly RequestDelegate _next;
               

        public JwtCookieValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var currentPath = context.Request.Path.Value;

            if (IsPathExcluded(currentPath))
            {
                await _next(context);
                return;
            }

            var jwtCookie = context.Request.Cookies["X-Access-Token"];

            if (string.IsNullOrEmpty(jwtCookie))
            {
                
                if (!IsUserAuthenticated(context))
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                context.Response.Redirect("/Login");
                return;
            }

            await _next(context);
        }

        private bool IsPathExcluded(string currentPath)
        {
            return currentPath.Equals("/", StringComparison.OrdinalIgnoreCase) ||
                   currentPath.StartsWith("/Login", StringComparison.OrdinalIgnoreCase);

        }
        private bool IsUserAuthenticated(HttpContext context)
        {
            // Verifique se o usuário está autenticado no contexto do MVC
            return context.User.Identity.IsAuthenticated;
        }
    }
}
