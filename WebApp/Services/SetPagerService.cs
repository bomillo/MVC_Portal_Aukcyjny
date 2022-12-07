using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class SetPagerService
    {
        private readonly PortalAukcyjnyContext _context;
        private int PageSize = 20;

        public SetPagerService(PortalAukcyjnyContext context)
        {
            this._context = context;
        }


        public async Task<int> SetPager(int userId)
        {
            if (userId != -1)
            {
                var usr = await _context.Users.Where(x => x.UserId == userId).FirstOrDefaultAsync();

                if (usr.itemsOnPage != null)
                    PageSize = (int)usr.itemsOnPage;
            }

            return PageSize;
        }       
    }
}
