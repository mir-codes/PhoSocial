using System;
using System.Security.Claims;

namespace PhoSocial.API.Utilities
{
    public static class ClaimsExtensions
    {
        public static long? GetUserIdLong(this ClaimsPrincipal user)
        {
            if (user == null) return null;
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("id") ?? user.FindFirst("sub");
            if (idClaim == null) return null;
            if (long.TryParse(idClaim.Value, out var val)) return val;
            // fallback: try parse as Guid -> not supported here
            return null;
        }
    }
}
