using AuthAPI.DB.Models;
using LimpShared.Models.Authentication.Models;

namespace AuthAPI.Services.JWT.JwtAuthentication;

/// <summary>
/// Creates and validates JWT-tokens
/// </summary>
public interface IJwtAuthenticationService
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