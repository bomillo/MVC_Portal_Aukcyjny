using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;

namespace WebApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public ChatController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Send(int id, string message)
        {
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString().ToString().ToString());
            _context.DirectMessages.Add(new DirectMessage
            {
                Message = message,
                ReceiverId = id,
                SenderId = userId,
                SentTime = DateTime.Now.ToUniversalTime(),
                Recieved = false
            });
            _context.SaveChanges();

            return LocalRedirect($"~/chat?id={id}");
        }

        public async Task<IActionResult> Index(int? id)
        {
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString().ToString().ToString());
            var portalAukcyjnyContext = _context.DirectMessages.Include(d => d.Receiver).Include(d => d.Sender).Where(dm => dm.SenderId == userId).ToList().GroupBy(dm => dm.ReceiverId).ToList();
            var portalAukcyjnyContext2 = _context.DirectMessages.Include(d => d.Receiver).Include(d => d.Sender).Where(dm => dm.ReceiverId == userId).ToList().GroupBy(dm => dm.SenderId).ToList();
            


            portalAukcyjnyContext.AddRange(portalAukcyjnyContext2);

            var model = new ChatModel();

            model.directMessagesHeadlines = portalAukcyjnyContext.Select(dms => dms.OrderBy(dm => dm.SentTime).First()).ToList();
            model.directMessages = portalAukcyjnyContext.First(dms => dms.Any(dm => id == null || dm.ReceiverId == id || dm.SenderId == id)).OrderBy(dm => dm.SentTime).ToList();
            var firstDm = model.directMessages.First();

            model.Id = firstDm.SenderId == userId? firstDm.ReceiverId : firstDm.SenderId;
            foreach (var dm in model.directMessages) {
                dm.Recieved = true;
            }

            _context.SaveChanges();
            return View("Index",model);
        }
    }
}
