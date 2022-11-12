using AuthAPI.Models;
using AuthAPI.Services.JWT.Models;
using System.Security.Claims;

namespace AuthAPI.Services.JWT
{
    public interface IJwtService
    {
        public string GenerateAccessToken(IUser user);
        public ClaimsPrincipal ValidateAccessToken(string jwtToken);
        public IRefreshToken GenerateRefreshToken();
    }
}