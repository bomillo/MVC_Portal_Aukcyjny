using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Context
{
    public class PortalAukcyjnyContext : DbContext
    {
        // public DbSet<Blog> Blogs { get; set; }
        public DbSet<Auction> Auctions { get; set; }

        public PortalAukcyjnyContext(DbContextOptions<PortalAukcyjnyContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>().ToTable("Auctions");
        }
    }
}
