using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetNotificationsAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task CreateNotificationAsync(Notification notification);
    }

    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid ReferenceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
