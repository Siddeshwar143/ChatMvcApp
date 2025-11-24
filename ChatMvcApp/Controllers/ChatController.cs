using ChatMvcApp.Data;
using ChatMvcApp.Models;
using ChatMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatMvcApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ChatMessages
                .Where(m => !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var viewModel = new ChatViewModel
            {
                Messages = messages,
                CurrentUser = currentUser
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return RedirectToAction("Index");
            }

            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chatMessage = new ChatMessage
            {
                Message = message.Trim(),
                SenderId = currentUserId,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (message != null && message.SenderId == currentUserId)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
