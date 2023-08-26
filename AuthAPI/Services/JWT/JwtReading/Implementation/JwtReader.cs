using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.DB.Models;
using AuthAPI.DB.Models.ModelExtensions;
using AuthAPI.Services.JWT.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Services.JWT.JwtReading.Implementation
{
    public class JwtReader : IJwtReader
    {
        private readonly IConfiguration _configuration;

        public JwtReader(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GenerateAccessToken(IUser user)
        {
            var mySecret = _configuration["JWT:Key"]!;
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

            var myIssuer = _configuration["JWT:Issuer"];
            var myAudience = _configuration["JWT:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(user?.Claims?.Select(x => x.ToClaim()).ToList()),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = myIssuer,
                Audience = myAudience,
                SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public TokenClaim? GetClaim(string token, string claimName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var stringClaimValue = securityToken?.Claims.FirstOrDefault(claim => claim.Type == claimName)?.Value;
            return String.IsNullOrWhiteSpace(stringClaimValue) ? null : new TokenClaim { Name = claimName, Value = stringClaimValue };
        }

        public List<TokenClaim>? GetTokenClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            List<TokenClaim>? claimList = tokenS?.Claims?.Select(x => new TokenClaim { Name = x.Type, Value = x.Value }).ToList();
            return claimList;
        }

        public string GetUsernameFromAccessToken(string accessToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(accessToken))
                throw new ArgumentException("Given access token is not readable.");

            JwtSecurityToken? securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            string? username = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Access token does not contain a username-containing property.");

            return username;
        }
    }
}
