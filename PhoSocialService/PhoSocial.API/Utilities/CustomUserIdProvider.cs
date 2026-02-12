using Microsoft.AspNetCore.SignalR;

namespace PhoSocial.API.Utilities
{
    /// <summary>
    /// Custom user ID provider for SignalR that uses the "id" claim from JWT
    /// instead of the default ClaimTypes.NameIdentifier
    /// </summary>
    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("id")?.Value;
        }
    }
}
