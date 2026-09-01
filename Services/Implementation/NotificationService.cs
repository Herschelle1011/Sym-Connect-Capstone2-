using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Hubs;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Notification;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Notification;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }
        public async Task NotifyAdminAsync(NotificationViewModel vm)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            foreach(var admin in admins)
            {
                var notification = new Notification
                {
                    Title = vm.Title,
                    Message = vm.Message,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    UserId = admin.Id,
                    Url = vm.Url
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveNotification", new
                {
                    title = vm.Title,
                    message = vm.Message,
                    createdAt = DateTime.Now,
                    url = vm.Url
                });
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Url = n.Url
                })
                .ToListAsync();
        }




        public async Task NotifyUserAsync(
                string userId,
                string title,
                string message,
                string? url = null)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            var notification = new Notification
            {
                Title = title,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = userId,
                Url = url
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                title,
                message,
                createdAt = DateTime.Now,
                url
            });

        }

        public async Task NotifyGroupMembersAsync(int groupId,
                                                string title,
                                                string message,
                                                string? url = null)
        {
            var memberUserIds = await _context.GroupDetails
              .Where(gd => gd.GroupId == groupId)
              .Include(gd => gd.Member)
              .Where(gd => gd.Member != null &&
                           gd.Member.UserId != null)
              .Select(gd => gd.Member.UserId!)
              .Distinct()
              .ToListAsync();


            foreach (var userId in memberUserIds)
            {

                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    UserId = userId,
                    Url = url
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Group_{groupId}")
                .SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    createdAt = DateTime.Now,
                    url
                });

        }




        public async Task<List<NotificationViewModel>> GetRecentNotificationsAsync(string userId, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Select(n => new NotificationViewModel
                {
                    Title = n.Title,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Url = n.Url
                }).ToListAsync();

        }


        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }


        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false).CountAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }


    }
}
