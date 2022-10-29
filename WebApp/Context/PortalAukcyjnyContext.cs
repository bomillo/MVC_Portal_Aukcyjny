using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using System.Linq;

namespace WebApp.Context
{
    public class PortalAukcyjnyContext : DbContext
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<AuctionAlerts> AuctionAlerts { get; set; }
        public DbSet<AuctionEditHistoryEntry> AuctionEditHistoryEntry { get; set; }
        public DbSet<AuctionQuestion> AuctionQuestion { get; set; }
        public DbSet<Bid> Bid { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
        public DbSet<ObservedAuction> ObservedAuctions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<User> Users { get; set; }

        public PortalAukcyjnyContext(DbContextOptions<PortalAukcyjnyContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Auction>()
                .Property(a => a.CreationTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());
            

            modelBuilder.Entity<AuctionEditHistoryEntry>()
                .Property(a => a.ChangedTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<AuctionQuestion>()
                .Property(a => a.PublishedTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<Bid>()
                .Property(a => a.BidTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<CurrencyExchangeRate>()
                .Property(a => a.LastUpdatedTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<DirectMessage>()
                .Property(a => a.Message)
                .IsRequired();
            modelBuilder.Entity<DirectMessage>()
                .Property(a => a.SentTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<ObservedAuction>()
                .HasKey(o => new { o.UserId, o.AuctionId });


        }
    }
}
