using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WebApp.Context;
using WebApp.Services;
using WebApp.Middlewares;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<PortalAukcyjnyContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PortalAukcyjnyContext")).EnableSensitiveDataLogging());

builder.Services.AddControllersWithViews();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.ConsentCookie = new CookieBuilder()
    {
        Name = "CONSENT_COOKIE",
        Expiration = TimeSpan.FromDays(366),
        SecurePolicy = CookieSecurePolicy.None
    };
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "admin"));
});

builder.Services.AddAuthentication("CookieAuthentication")
.AddCookie("CookieAuthentication", config =>
{
    config.Cookie.HttpOnly = true;
    config.Cookie.SecurePolicy = CookieSecurePolicy.None;
    config.Cookie.Name = "UserLoginCookie";
    config.LoginPath = "/Login/Index";
    config.LogoutPath = "/Login/LogOut";
    config.AccessDeniedPath = "/Denied";
    config.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddScoped<UsersService>();
builder.Services.AddTransient<DbSeeder>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders= ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto});

var supportedCultures = new[] { "en-US", "fr-FR", "pl-PL" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<PortalAukcyjnyContext>();
    context.Database.EnsureCreated();

    var service = scope.ServiceProvider.GetService<DbSeeder>();
    service.Seed();
}

app.UseCookiePolicy();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ThemeMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
