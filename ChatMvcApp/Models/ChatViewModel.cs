using ChatMvcApp.Models;

namespace ChatMvcApp.ViewModels
{
    public class ChatViewModel
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public string NewMessage { get; set; }
        public ApplicationUser CurrentUser { get; set; }
    }
}