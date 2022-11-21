using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WebApp.Context;
using WebApp.Services;
using WebApp.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;

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
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
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
    config.Cookie.SameSite = SameSiteMode.Lax;
    config.Cookie.IsEssential = true;
})
.AddGoogle( GoogleDefaults.AuthenticationScheme, config => {
    var authData = builder.Configuration.GetSection("Authentication:Google");
    config.ClientId = authData["ClientId"];
    config.ClientSecret = authData["ClientSecret"];
    config.CorrelationCookie.SameSite = SameSiteMode.Lax;
    config.CorrelationCookie.IsEssential = true;
    config.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
    
})
.AddFacebook(config =>
{
    var authData = builder.Configuration.GetSection("Authentication:Facebook");
    config.AppId = authData["AppId"];
    config.AppSecret = authData["AppSecret"];
    config.CorrelationCookie.SameSite = SameSiteMode.Lax;
    config.CorrelationCookie.IsEssential = true;
    config.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<ObservAuctionService>();
builder.Services.AddTransient<DbSeeder>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddTransient<BreadcrumbService>();
builder.Services.AddMemoryCache();

builder.Services.AddDirectoryBrowser();

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


app.UseStaticFiles();   // for wwwroot

#region retrieve and prepare path
var procesPath = Environment.ProcessPath.Replace('\\', '/');
var length = procesPath.LastIndexOf('/');
var path = Environment.ProcessPath.Remove(length);
path = Path.Combine(path, "Uploads");
Directory.CreateDirectory(path);
#endregion


// for images
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(path),
    RequestPath = "/Uploads",
    EnableDirectoryBrowsing = true
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ThemeMiddleware>();
app.UseMiddleware<VisitCounterMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
