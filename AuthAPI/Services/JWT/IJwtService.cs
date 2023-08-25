using AuthAPI.Services.JWT.Models;

namespace AuthAPI.Services.JWT
{
    public interface IJwtService
    {
        public string GetUsernameFromAccessToken(string accessToken);
        public List<TokenClaim>? GetTokenClaims(string token);
        public TokenClaim? GetClaim(string token, string claimName);
    }
}