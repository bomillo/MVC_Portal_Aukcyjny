using BackgroundTasks.Models;
using BackgroundTasks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Collections.Specialized.BitVector32;

namespace BackgroundTasks.Context
{
    public class PortalAukcyjnyContext2 : DbContext
    {
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
        
        public PortalAukcyjnyContext2(DbContextOptions<PortalAukcyjnyContext2> options) : base(options)
        {
        }
    }
}
