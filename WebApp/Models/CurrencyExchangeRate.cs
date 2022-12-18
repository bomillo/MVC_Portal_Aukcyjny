using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class CurrencyExchangeRate {
        public CurrencyExchangeRate() { }

        public CurrencyExchangeRate(string name, string currencyCode, double exchangeRate, DateTime lastUpdatedTime)
        {
            CurrencyName = name;
            CurrencyCode = currencyCode;
            ExchangeRate = exchangeRate;
            LastUpdatedTime = lastUpdatedTime;
        }

        [Key]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public double ExchangeRate { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
