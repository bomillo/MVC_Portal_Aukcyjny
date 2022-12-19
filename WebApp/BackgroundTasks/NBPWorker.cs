﻿using Elastic.Clients.Elasticsearch.Core.GetScriptContext;
using WebApp.Context;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.BackgroundTasks
{
    public class NBPWorker : BackgroundService
    {
        public ILogger<NBPWorker> Logger;
        private readonly CurrencyDownloadService service;
        private readonly IServiceScopeFactory factory;
        private readonly TimeSpan downloadIdle = TimeSpan.FromDays(4);

        public NBPWorker(ILogger<NBPWorker> logger, CurrencyDownloadService service, IServiceScopeFactory factory)
        {
            Logger = logger;
            this.service = service;
            this.factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Logger.LogInformation("NBP WORKER: Starting downloading at " + DateTime.Now.ToShortTimeString());

                UpdateCurrencyDatabase();
                await Task.Delay(downloadIdle);
            }
        }

        private void UpdateCurrencyDatabase()
        {
            PortalAukcyjnyContext context = factory.CreateScope().ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();

            var currList = service.GetAll();

            if (currList != null)
            {
                IRateIterator<Rate> rateIterator = currList[0].getIterator();

                while (rateIterator.hasNext())
                {
                    var item = rateIterator.next();

                    var currency = context.CurrencyExchangeRates.Where(x => x.CurrencyCode == item.code).FirstOrDefault();
                    if (currency != null)
                    {
                        currency.ExchangeRate = item.mid;
                        currency.LastUpdatedTime = DateTime.UtcNow;
                        context.Update(currency);
                    }
                    else
                    {
                        CurrencyExchangeRate rate = new CurrencyExchangeRate(item.currency, item.code, item.mid, DateTime.Now.ToUniversalTime());
                        context.CurrencyExchangeRates.Add(rate);
                    }
                }

                var pln = context.CurrencyExchangeRates.Where(x => x.CurrencyCode == "PLN").FirstOrDefault();
                if (pln == null)
                {
                    CurrencyExchangeRate złoty = new CurrencyExchangeRate("złoty polski", "PLN", 1, DateTime.Now.ToUniversalTime());
                    context.CurrencyExchangeRates.Add(złoty);
                }

                context.SaveChanges();
                Logger.LogInformation("NBP WORKER: Data downloaded - idle: " + downloadIdle + " days");
            }
        }
    }
}
