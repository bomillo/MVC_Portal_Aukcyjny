using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class DMService
    {
        private readonly PortalAukcyjnyContext context;

        public DMService(PortalAukcyjnyContext context)
        {
            this.context = context;
        }
        public void SendDM(DirectMessage dm)
        {
            context.Add(dm);
            context.SaveChanges();
        }
    }
}
