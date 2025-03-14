using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.Utils.Extensions
{
    public static class ClaimPrincipalExtension
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdStr == null) throw new BusinessException("Authentication Error");

            int.TryParse(userIdStr.Value, out int userId);

            return userId;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var userEmail = user.FindFirst(ClaimTypes.Email);
            if (userEmail == null) throw new BusinessException("Authentication Error");

            return userEmail.Value;
        }

        public static Guid GetCompanyId(this ClaimsPrincipal user)
        {
            var companyIdClaim = user.FindFirst("CompanyId");

            if (companyIdClaim == null) throw new BusinessException("Authentication Error");

            var result = Guid.Parse(companyIdClaim.Value);

            return result;
        }

        public static bool IsValidSubscription(this ClaimsPrincipal user)
        {
            var subscriptionLimitClaim = user.FindFirst("SubscriptionLimit");

            if (subscriptionLimitClaim == null) throw new BusinessException("Authentication Error");

            if (string.IsNullOrEmpty(subscriptionLimitClaim.Value)) return false;

            var dateTimeOffset = DateTimeOffset.Parse(subscriptionLimitClaim.Value);            
            
            return dateTimeOffset.UtcDateTime > DateTimeOffset.Now.UtcDateTime ? true : false;
        }

        public static Guid GetTokenId(this ClaimsPrincipal user) 
        {
            var jtiClaim = user.FindFirst(JwtRegisteredClaimNames.Jti);            
            if (string.IsNullOrEmpty(jtiClaim?.Value)) throw new BusinessException("Authentication Error");

            if (Guid.TryParse(jtiClaim.Value, out var jtiGuid))            
                return jtiGuid;            
            else
                throw new BusinessException("Authentication Error");
        }
    }
}
