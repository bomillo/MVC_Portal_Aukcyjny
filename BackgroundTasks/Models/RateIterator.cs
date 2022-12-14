using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using BackgroundTasks.Models;

namespace BackgroundTasks.Models
{
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
}
