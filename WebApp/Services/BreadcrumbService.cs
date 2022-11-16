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

        public IEnumerable<BreadcrumbItem> CreateCurrentPath(Product product)
        {
            if(product == null)
            {
                throw new Exception("Are you fucking stupid???? Prodcut cannot be null.");
            }

            var items = new List<BreadcrumbItem>() { new BreadcrumbItem() { ItemName = product.Name, RelativePath = $"/Products/Details/{product.ProductId}" } };

            int previousItemId;
            previousItemId = product.CategoryId;
            Category category = _dbContext.Categories.Single(x => x.CategoryId == previousItemId);
            items.Add(new BreadcrumbItem() { ItemName = category.Name, RelativePath = $"/Categories/Product/{category.CategoryId}" });

            while (category.ParentCategoryId != null)
            {
                previousItemId = (int)category.ParentCategoryId;
                category = _dbContext.Categories.Single(x => x.CategoryId == previousItemId);
                items.Add(new BreadcrumbItem() { ItemName = category.Name, RelativePath = $"/Categories/Product/{category.CategoryId}" });

            }


            items.Reverse();
            return items;
        }

    }
}
