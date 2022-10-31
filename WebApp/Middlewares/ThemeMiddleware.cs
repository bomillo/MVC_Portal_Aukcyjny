using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Context;

namespace WebApp.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ThemeMiddleware
    {
        
        private readonly RequestDelegate _next;
        
        public ThemeMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public Task Invoke(HttpContext httpContext, PortalAukcyjnyContext _dbContext)
        {
            
            string theme = string.Empty;
            if (httpContext.User.Claims.Any())
            {
                var userId = int.Parse(httpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString());
                var user = _dbContext.Users.FirstOrDefault(x => x.UserId == userId);
                if(user != null)
                {
                    theme = user.ThemeType.ToString();
                }
            }
            else if (httpContext.Request.Cookies["THEME_COOKIE"] != null)
            {
                theme = httpContext.Request.Cookies["THEME_COOKIE"];
            }
            else
            {
                theme = "dark";
                httpContext.Response.Cookies.Append("THEME_COOKIE", theme, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1)});
            }
            theme = theme.ToLower();
            httpContext.Items.Add("cssFile", theme);
            theme += "-mode";
            httpContext.Items.Add("theme", theme);
            
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ThemeMiddlewareExtensions
    {
        public static IApplicationBuilder UseThemeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThemeMiddleware>();
        }
    }
}
