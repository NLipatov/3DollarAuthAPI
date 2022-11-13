using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserProvider;
using System.Security.Claims;

namespace AuthAPI.Services.JWT
{
    public interface IJwtService
    {
        public ClaimsPrincipal ValidateAccessToken(string jwtToken);
        public Task<JWTPair> CreateJWTPairAsync(IUserProvider userProvider, string username);
    }
}