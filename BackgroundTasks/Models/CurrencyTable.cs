using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks.Models
{
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
}
