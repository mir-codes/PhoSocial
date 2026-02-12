using System;

namespace PhoSocial.API.Models.V2
{
    public class MessageV2
    {
        public long Id { get; set; }
        public long ConversationId { get; set; }
        public long SenderId { get; set; }
        public string? Username { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationListItem
    {
        public long ConversationId { get; set; }
        public long OtherUserId { get; set; }
        public string? OtherUsername { get; set; }
        public string? OtherProfileImageUrl { get; set; }
        public long? LastMessageId { get; set; }
        public long? LastMessageSenderId { get; set; }
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
