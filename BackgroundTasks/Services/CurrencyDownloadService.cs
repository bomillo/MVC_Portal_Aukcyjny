using BackgroundTasks.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks.Services
{
    public class CurrencyDownloadService
    {
        private readonly string exchangeTable = "a";
        private readonly string format = "json";

        public List<CurrencyTable> Get(string currCode)
        {

            //var url = $"https://api.nbp.pl/api/exchangerates/tables/{exchangeTable}/?format={format}";
            var url = $"https://api.nbp.pl/api/exchangerates/rates/{exchangeTable}/{currCode}/?format={format}";
            var web = new WebClient();

            var response = web.DownloadString(url);

            var myDeserializedClass = JsonConvert.DeserializeObject<List<CurrencyTable>>(response);

            return myDeserializedClass;
        }

        public List<CurrencyTable> GetAll()
        {

            var url = $"https://api.nbp.pl/api/exchangerates/tables/{exchangeTable}?format={format}";
            //var url = $"https://api.nbp.pl/api/exchangerates/rates/{exchangeTable}/{currCode}/?format={format}";
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
}
