using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class CurrencyExchangeRate {
        [Key]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public double ExchangeRate { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
