using Sym_Connect__Capstone2_.ViewModels.Notification;
namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAdminAsync(NotificationViewModel vm);
        Task NotifyGroupMembersAsync(int groupId,
                                                string title,
                                                string message,
                                                string? url = null);
        Task NotifyUserAsync(string userId,string title,string message,string? url = null);


        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId);
        Task<List<NotificationViewModel>> GetRecentNotificationsAsync(string userId, int count = 5);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);



    }
}
