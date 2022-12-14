using BackgroundTasks;
using BackgroundTasks.Context;
using BackgroundTasks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using System.Data.Common;

IHost host = Host.CreateDefaultBuilder(args)
     .ConfigureServices((hostContext, services) =>
    {
        //services.AddHostedService<Worker>();
        services.AddHostedService<NBPWorker>();
        services.AddTransient<CurrencyDownloadService>();
        services.AddDbContext<PortalAukcyjnyContext2>(options => options.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalAukcyjnyContext")));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    })
    .Build();

await host.RunAsync();
