using Microsoft.Extensions.Hosting;

namespace WebApp.Models.DTO
{
    public class ChatModel
    {
        public List<DirectMessage> directMessages = new List<DirectMessage>();
        public List<DirectMessage> directMessagesHeadlines = new List<DirectMessage>();
    }

    public class DirectMessageWithDirectionTag : DirectMessage{
        
    }
}
