using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public NotificationsController(INotificationService notificationService, UserManager<User> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // ===== DROPDOWN NOTIF ====

        [HttpGet]
        public async Task<IActionResult> Dropdown()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return PartialView("_NotificationDropdown", new List<Sym_Connect__Capstone2_.ViewModels.Notification.NotificationViewModel>());

            var recent = await _notificationService.GetRecentNotificationsAsync(userId, 5);
            return PartialView("_NotificationDropdown", recent);
        }



        //==== FULL PAGE NOTIFICATION ====== 

        [HttpGet]
        public async Task<IActionResult> Index(bool unreadOnly = false)
        {
            var userId = _userManager.GetUserId(User);
            var all = userId == null ? new List<Sym_Connect__Capstone2_.ViewModels.Notification.NotificationViewModel>() :
                await _notificationService.GetUserNotificationsAsync(userId);

            ViewBag.unreadOnly = unreadOnly;
            var model = unreadOnly ? all.Where(n => n.IsRead == false).ToList() : all;

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            var count = userId == null ? 0 : await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }
    }
}
