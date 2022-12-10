﻿using BackgroundTasks.Context;
using BackgroundTasks.Models;
using BackgroundTasks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks
{
    public class NBPWorker : BackgroundService
    {
        public ILogger<NBPWorker> Logger;
        private readonly CurrencyDownloadService service;
        private readonly PortalAukcyjnyContext2 context;
        private readonly TimeSpan downloadIdleDays = TimeSpan.FromDays(4);

        public NBPWorker(ILogger<NBPWorker> logger, CurrencyDownloadService service, IServiceScopeFactory factory)
        {
            Logger = logger;
            this.service = service;
            this.context = factory.CreateScope().ServiceProvider.GetRequiredService<PortalAukcyjnyContext2>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Logger.LogInformation("NBP WORKER: Starting downloading at " + DateTime.Now.ToShortTimeString());

                new Timer(UpdateCurrencyDatabase, null, TimeSpan.Zero, downloadIdleDays);
                await Task.Delay(downloadIdleDays);
            }
        }

        private void UpdateCurrencyDatabase(object? state)
        {
            var currList = service.GetAll();

            foreach (var item in currList[0].rates)
            {
                CurrencyExchangeRate rate = new CurrencyExchangeRate(item.code, item.mid, DateTime.Now.ToUniversalTime());

                context.CurrencyExchangeRates.Add(rate);
            }

            context.SaveChanges();

            Logger.LogInformation("NBP WORKER: Data downloaded - idle: " + downloadIdleDays + " days");
        }
    }
}
