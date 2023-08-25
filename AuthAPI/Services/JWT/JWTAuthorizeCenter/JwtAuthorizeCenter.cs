using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthAPI.Models;
using AuthAPI.Models.ModelExtensions;
using LimpShared.Models.Authentication.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Services.JWT.JWTAuthorizeCenter;

public class JwtAuthorizeCenter : IJwtAuthorizeCenter
{
    private readonly IConfiguration _configuration;

    public JwtAuthorizeCenter(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public bool ValidateAccessToken(string accessToken)
    {
        string mySecret = _configuration["JWT:Key"]!;
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

        var myIssuer = _configuration["JWT:Issuer"];
        var myAudience = _configuration["JWT:Audience"];

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = myIssuer,
                ValidAudience = myAudience,
                IssuerSigningKey = mySecurityKey,
                ValidateLifetime = true,
                LifetimeValidator = CustomLifetimeValidator
            }, out SecurityToken validatedToken);
        }
        catch
        {
            return false;
        }
        return true;
    }
    
    private bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
    {
        if (expires != null)
        {
            return expires > DateTime.UtcNow;
        }
        return false;
    }

    public JWTPair CreateJwtPair(IUser user)
    {
        string accessToken = GenerateAccessToken(user);

        RefreshToken refreshToken = GenerateRefreshToken();

        return new JWTPair()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
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
    
    private RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
        };
    }
}