using WebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SkiaSharp.SKPath;

namespace WebApp.Services
{
    public class CurrencyDownloadService
    {
        private readonly string exchangeTable = "a";
        private readonly string format = "json";

        public List<CurrencyTable> Get(string currCode)
        {
            var url = $"https://api.nbp.pl/api/exchangerates/rates/{exchangeTable}/{currCode}/?format={format}";
            var web = new WebClient();

            var response = web.DownloadString(url);

            var myDeserializedClass = JsonConvert.DeserializeObject<List<CurrencyTable>>(response);

            return myDeserializedClass;
        }

        public List<CurrencyTable> GetAll()
        {
            var url = $"https://api.nbp.pl/api/exchangerates/tables/{exchangeTable}?format={format}";
            var web = new WebClient();

            try
            {
                var response = web.DownloadString(url);

                List<CurrencyTable> myDeserializedClass = JsonConvert.DeserializeObject<List<CurrencyTable>>(response);
         
                return myDeserializedClass;

            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }

    public class CurrencyTable
    {
        public string table { get; set; }
        public string no { get; set; }
        public string effectiveDate { get; set; }
        public List<Rate> rates { get; set; }


        public IRateIterator<Rate> getIterator()
        {
            return new RateIterator(this.rates);
        }
    }

    public class Rate
    {
        public string currency { get; set; }
        public string code { get; set; }
        public double mid { get; set; }
    }


    public class RateIterator : IRateIterator<Rate>
    {
        private readonly List<Rate> rates;
        private int current = 0;
        private int counts = 0;


        public RateIterator(List<Rate> currencyTable)
        {
            this.rates = currencyTable;
            counts = rates.Count;
        }

        public bool hasNext()
        {
            return current < counts;
        }

        public Rate next()
        {
            return rates[current++];
        }

        public int count()
        {
            return counts;
        }

        public Rate first()
        {
            current = 0;
            return rates[current++];
        }

        public Rate last()
        {
            current = count() - 1;
            return rates[current++];
        }
    }

    public interface IRateIterator<T>
    {
        public Boolean hasNext();
        public T next();
        public T first();
        public T last();
        public int count();
    }
}
