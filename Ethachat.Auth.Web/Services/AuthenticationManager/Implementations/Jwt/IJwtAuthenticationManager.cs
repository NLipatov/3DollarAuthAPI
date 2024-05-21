using Ethachat.Auth.Domain.Models;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;

namespace AuthAPI.Services.AuthenticationManager.Implementations.Jwt;

/// <summary>
/// Creates and validates JWT-tokens
/// </summary>
public interface IJwtAuthenticationManager
{
    /// <summary>
    /// Validates given access token
    /// </summary>
    public bool ValidateAccessToken(string accessToken);
    
    /// <summary>
    /// Create new JWT pair with no validation
    /// </summary>
    public JwtPair CreateJwtPair(IUser user);
}