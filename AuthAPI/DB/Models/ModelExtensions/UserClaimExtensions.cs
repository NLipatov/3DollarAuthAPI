using System.Security.Claims;

namespace AuthAPI.DB.Models.ModelExtensions
{
    public static class UserClaimExtensions
    {
        public static Claim ToClaim(this UserClaim uClaim)
        {
            return new Claim(uClaim.Type, uClaim.Value);
        }
    }
}
