using System;

namespace PhoSocial.API.Models
{
    public class Message
    {
    public Guid Id {get; set;}
    public Guid SenderId {get; set;}
    public Guid ReceiverId {get; set;}
    public string? Content {get; set;}
    public string Status {get; set;} = "Sent";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserName { get; set; } // Sender's username
    }
}
