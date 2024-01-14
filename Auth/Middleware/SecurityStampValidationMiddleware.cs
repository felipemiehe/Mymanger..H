using Auth.DTO;
using Auth.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Middleware
{
    public class SecurityStampValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;


        public SecurityStampValidationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            // Criar um escopo para resolver o UserManager
            using (var scope = _scopeFactory.CreateScope())
            {
                // Obter o serviço UserManager dentro do escopo
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                
                var userNameClaim = context.User.FindFirst(ClaimTypes.Name);
                var securityStampClaim = context.User.FindFirst("securityStamp");


                if (userNameClaim != null && !string.IsNullOrEmpty(userNameClaim.Value) && securityStampClaim != null)
                {
                    var user = await userManager.FindByEmailAsync(userNameClaim.Value);

                    if (user != null)
                    {
                        var currentSecurityStampDB = await userManager.GetSecurityStampAsync(user);
                        

                        if (currentSecurityStampDB != securityStampClaim.Value)
                        {
                            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                            context.Response.StatusCode = 401; // Unauthorized
                            return;
                        }
                    }                                       
                }                               
            }
                        
            await _next(context);
        }
    }
}
