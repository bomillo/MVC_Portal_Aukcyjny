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
        public DbSet<User> Users { get; set; }

        public PortalAukcyjnyContext(DbContextOptions<PortalAukcyjnyContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>()
                .Property(a => a.Title)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Auction>()
                .Property(a => a.Description)
                .HasMaxLength(2000);
            modelBuilder.Entity<Auction>()
                .Property(a => a.IsDraft)
                .HasDefaultValue(true);
            modelBuilder.Entity<Auction>()
                .Property(a => a.CreationTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());


            modelBuilder.Entity<AuctionEditHistoryEntry>()
                .Property(a => a.ChangedTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());
            modelBuilder.Entity<AuctionEditHistoryEntry>()
                .Property(a => a.JsonChange)
                .IsRequired()
                .HasMaxLength(2000);
            modelBuilder.Entity<AuctionEditHistoryEntry>()
                .Property(a => a.Type)
                .IsRequired();


            modelBuilder.Entity<AuctionQuestion>()
                .Property(a => a.Question)
                .HasMaxLength(1500)
                .IsRequired();
            modelBuilder.Entity<AuctionQuestion>()
                .Property(a => a.PublishedTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());

            modelBuilder.Entity<Bid>()
                .Property(a => a.BidTime)
                .HasDefaultValue(DateTime.Now.ToUniversalTime());
            modelBuilder.Entity<Bid>()
                .Property(a => a.Price)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Company>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Company>()
                .Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(150);
            modelBuilder.Entity<Company>()
                .Property(a => a.NIP)
                .IsRequired()
                .HasMaxLength(10)
                .IsFixedLength();

            modelBuilder.Entity<CurrencyExchangeRate>()
                .Property(a => a.CurrencyCode)
                .IsRequired();
            modelBuilder.Entity<CurrencyExchangeRate>()
                .Property(a => a.ExchangeRate)
                .IsRequired();
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

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Product>()
                .Property(p => p.VatRate)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.IsVatExclueded);

            modelBuilder.Entity<User>()
                .Property(p => p.Name)
                .HasMaxLength(50);
            modelBuilder.Entity<User>()
                .Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(150);
            modelBuilder.Entity<User>()
                .Property(p => p.PasswordHashed)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(p => p.UserType)
                .HasDefaultValue(UserType.Normal);
            modelBuilder.Entity<User>()
                .Property(p => p.ThemeType)
                .HasDefaultValue(ThemeType.Dark);
            modelBuilder.Entity<User>()
                .Property(p => p.Language)
                .HasDefaultValue(Language.PL);
        }
    }
}
