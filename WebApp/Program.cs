using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebApp.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PortalAukcyjnyContext>(options =>
            options.UseNpgsql("Host=localhost:5432;Database=PortalAukcyjny;Username=postgres;Password=admin"));

builder.Services.AddControllersWithViews(); 
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders= ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<PortalAukcyjnyContext>();
    context.Database.EnsureCreated();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
