using AuthAPI.Services.AuthenticationManager.Implementations.Jwt.Models;
using AuthAPI.Services.UserArea.UserProvider;
using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.ModelExtensions;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Services.AuthenticationManager.Implementations.Jwt.Implementation;

public class JwtAuthenticationManager : IJwtAuthenticationManager, IAuthenticationManager<JwtPair>
{
    private readonly SecureJwt _secureJwt;
    private readonly IUserProvider _userProvider;

    public JwtAuthenticationManager(IConfiguration configuration, IUserProvider userProvider)
    {
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));

        // Initialize SecureJwt with configuration settings
        var secretKey = configuration["JWT:Key"] ?? string.Empty;
        var issuer = configuration["JWT:Issuer"] ?? string.Empty;
        var audience = configuration["JWT:Audience"] ?? string.Empty;
        var accessTokenExpirationMinutes = configuration["JWT:AccessTokenExpirationMinutes"] ?? string.Empty;
        var refreshTokenExpirationDays = configuration["JWT:RefreshTokenExpirationDays"] ?? string.Empty;

        _secureJwt = new SecureJwt(secretKey, issuer, audience, accessTokenExpirationMinutes,
            refreshTokenExpirationDays);
    }

    /// <summary>
    ///     Validates the given JWT credentials.
    /// </summary>
    public Task<bool> ValidateCredentials(JwtPair credentials)
    {
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));

        return Task.FromResult(ValidateAccessToken(credentials.AccessToken));
    }

    /// <summary>
    ///     Refreshes the JWT credentials by issuing a new JWT pair.
    /// </summary>
    public async Task<JwtPair?> RefreshCredentials(JwtPair credentials)
    {
        if (credentials == null || string.IsNullOrWhiteSpace(credentials.RefreshToken?.Token))
            throw new ArgumentException("Invalid refresh token.");

        var user = await _userProvider.GetUserByRefreshTokenAsync(credentials.RefreshToken.Token);
        if (user == null) throw new ArgumentException("Invalid refresh token.");

        // Check if the refresh token has expired
        if (credentials.RefreshToken.Expires <= DateTime.UtcNow)
            throw new SecurityTokenExpiredException("Refresh token has expired.");

        // Generate a new JWT pair
        var jwtPair = CreateJwtPair(user);

        // Update the user's refresh token in the data store
        user.RefreshToken = jwtPair.RefreshToken.Token;
        await _userProvider.SaveRefreshTokenAsync(jwtPair, user);

        return jwtPair;
    }

    /// <summary>
    ///     Validates the access token using SecureJwt.
    /// </summary>
    public bool ValidateAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return false;

        return _secureJwt.ValidateToken(accessToken);
    }

    /// <summary>
    ///     Creates a new JWT pair (access token and refresh token).
    /// </summary>
    public JwtPair CreateJwtPair(IUser user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var accessToken = GenerateAccessToken(user);
        var refreshToken = _secureJwt.GenerateRefreshToken();

        return new JwtPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    ///     Generates an access token using SecureJwt.
    /// </summary>
    private string GenerateAccessToken(IUser user)
    {
        var claims = new Dictionary<string, object>
        {
            { "unique_name", user.Username },
            { "claims", user.Claims?.Select(c => c.ToClaim()) ?? [] }
        };

        return _secureJwt.GenerateToken(claims);
    }
}