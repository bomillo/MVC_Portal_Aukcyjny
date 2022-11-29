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

        // GET: DirectMessages
        public async Task<IActionResult> Index()
        {
            //var portalAukcyjnyContext = _context.DirectMessages.Include(d => d.Receiver).Include(d => d.Sender);
            var model = new ChatModel();

            model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 1,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 2,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 2,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 1,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 2,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            }); model.directMessagesHeadlines.Add(new DirectMessage
            {
                SenderId = 0,
                Sender = new User
                {
                    Name = "NAME!"
                },
                ReceiverId = 1,
                Receiver = new User
                {
                    Name = "NAME 2!"
                },
                Message = "Message",
                SentTime = DateTime.Now
            });

            return View(model);
        }
    }
}
