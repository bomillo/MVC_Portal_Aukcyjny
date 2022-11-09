using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Threading.Tasks;
using WebApp.Services;

namespace WebApp.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class VisitCounterMiddleware
    {
        private int _visitCounter = 0;
        private readonly RequestDelegate _next;

        public VisitCounterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, VisitCounterService counter)
        {
            
            if (httpContext.Request.Cookies["PA_RECENTLY_VISITED"] == null && httpContext.Request.Cookies["CONSENT_COOKIE"] != null)
            {
                
                if(httpContext.Request.Cookies["CONSENT_COOKIE"].ToString().ToLower() == "yes")
                {
                    counter.incrementVisitCounter();
                    httpContext.Response.Cookies.Append("PA_RECENTLY_VISITED", "true", new CookieOptions()
                    {
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.Now.AddMonths(1)
                    });
                }
            }

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class VisitCounterMiddlewareExtensions
    {
        public static IApplicationBuilder UseVisitCounterMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitCounterMiddleware>();
        }
    }
}
