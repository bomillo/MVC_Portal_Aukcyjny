using Microsoft.Extensions.Hosting;

namespace WebApp.Models.DTO
{
    public class ChatModel
    {
        public int Id { get; set; }
        public List<DirectMessage> directMessages = new List<DirectMessage>();
        public List<DirectMessage> directMessagesHeadlines = new List<DirectMessage>();
    }
}
