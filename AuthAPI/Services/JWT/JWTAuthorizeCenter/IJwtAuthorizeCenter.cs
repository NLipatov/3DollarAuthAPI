using AuthAPI.Models;
using LimpShared.Models.Authentication.Models;

namespace AuthAPI.Services.JWT.JWTAuthorizeCenter;

public interface IJwtAuthorizeCenter
{
    /// <summary>
    /// Validates given access token
    /// </summary>
    public bool ValidateAccessToken(string accessToken);
    
    /// <summary>
    /// Create new JWT pair with no validation
    /// </summary>
    public JWTPair CreateJwtPair(IUser user);
}