using AuthAPI.Models;
using AuthAPI.Models.ModelExtensions;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserProvider;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Services.JWT
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
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
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = myIssuer,
                Audience = myAudience,
                SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateAccessToken(string token)
        {
            string mySecret = _configuration["JWT:Key"]!;
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

            var myIssuer = _configuration["JWT:Issuer"];
            var myAudience = _configuration["JWT:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = myIssuer,
                    ValidAudience = myAudience,
                    IssuerSigningKey = mySecurityKey
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
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

        public async Task<JWTPair> CreateJWTPairAsync(IUserProvider userProvider, string username)
        {
            string accessToken = GenerateAccessToken(await userProvider.GetUserByUsernameAsync(username) 
                ?? throw new ArgumentException("User is not registered"));

            IRefreshToken refreshToken = GenerateRefreshToken();

            return new JWTPair()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private IRefreshToken GenerateRefreshToken()
        {
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
            };
        }
    }
}
