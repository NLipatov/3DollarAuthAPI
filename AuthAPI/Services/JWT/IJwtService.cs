using AuthAPI.Models;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserProvider;
using System.Security.Claims;

namespace AuthAPI.Services.JWT
{
    public interface IJwtService
    {
        public bool ValidateAccessToken(string jwtToken);
        public Task<JWTPair> CreateJWTPairAsync(IUserProvider userProvider, string username);
        public List<TokenClaim>? GetTokenClaims(string token);
        public TokenClaim? GetClaim(string token, string claimName);
    }
}