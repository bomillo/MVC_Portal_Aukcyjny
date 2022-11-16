using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class BreadcrumbService
    {
        private readonly PortalAukcyjnyContext _dbContext;

        public BreadcrumbService(PortalAukcyjnyContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        public IEnumerable<BreadcrumbItem> CreateCurrentPath(Auction auction)
        {
            if(auction == null || auction.Product == null)
            {
                throw new Exception("Are you fucking stupid???? Auction and product asociated with auction cannot be null.");
            }

            var items = new List<BreadcrumbItem>() { new BreadcrumbItem() { ItemName = auction.Title, RelativePath = $"/Auctions/Details/{auction.AuctionId}" } };
            var category = _dbContext.Categories.Single(c => c.CategoryId == auction.Product.CategoryId);
            return CreateCurrentPath(auction.Product.Category, items);
        }

        public IEnumerable<BreadcrumbItem> CreateCurrentPath(Category category, List<BreadcrumbItem> items = null)
        {
            if(items == null)
            {
                items = new List<BreadcrumbItem>();
            }

            int previousItemId;
            items.Add(new BreadcrumbItem() { ItemName = category.Name, RelativePath = $"/Category/Auctions/{category.CategoryId}" });


            while (category.ParentCategoryId != null)
            {
                previousItemId = (int)category.ParentCategoryId;
                category = _dbContext.Categories.Single(x => x.CategoryId == previousItemId);
                items.Add(new BreadcrumbItem() { ItemName = category.Name, RelativePath = $"/Category/Auctions/{category.CategoryId}" });
            }
            
            items.Reverse();

            return items;

        }

    }
}
