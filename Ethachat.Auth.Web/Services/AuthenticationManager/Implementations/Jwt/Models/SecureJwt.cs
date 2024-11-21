using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EthachatShared.Models.Authentication.Models;

namespace AuthAPI.Services.AuthenticationManager.Implementations.Jwt.Models;

public class SecureJwt
{
    private readonly int _accessTokenExpirationMinutes;
    private readonly string _audience;
    private readonly string _issuer;
    private readonly int _refreshTokenExpirationDays;
    private readonly string _secretKey;

    public SecureJwt(string secretKey, string issuer, string audience, string accessTokenExpirationMinutes,
        string refreshTokenExpirationMinutesDays)
    {
        var secretKeyMinLengthBytes = 128;
        if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < secretKeyMinLengthBytes)
            throw new ArgumentException($"The secret key must be at least {secretKeyMinLengthBytes} bytes long.");

        _secretKey = secretKey;
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
        if (!int.TryParse(refreshTokenExpirationMinutesDays, out var parsedRefreshTokenExpirationDays))
            throw new ArgumentException($"The refresh token expiration days must be an integer.");
        _refreshTokenExpirationDays = parsedRefreshTokenExpirationDays;

        if (!int.TryParse(accessTokenExpirationMinutes, out var parsedAccessTokenExpirationMinutes))
            throw new ArgumentException($"The access token expiration minutes must be an integer.");
        _accessTokenExpirationMinutes = parsedAccessTokenExpirationMinutes;
    }

    /// <summary>
    ///     Generates a JWT token with the provided claims and expiration time.
    /// </summary>
    public string GenerateToken(Dictionary<string, object> claims)
    {
        // Create the JWT header
        var header = new Dictionary<string, object>
        {
            { "alg", "HS256" }, // Algorithm
            { "typ", "JWT" } // Token type
        };

        // Create the JWT payload
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var exp = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(_accessTokenExpirationMinutes)).ToUnixTimeSeconds();

        claims["iss"] = _issuer; // Issuer
        claims["aud"] = _audience; // Audience
        claims["iat"] = now; // Issued At
        claims["exp"] = now + exp; // Expiration Time

        // Serialize header and payload to JSON, then encode in Base64Url
        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(claims));

        // Generate the signature
        var signature = CreateSignature(encodedHeader, encodedPayload);
        var encodedSignature = Base64UrlEncode(signature);

        // Combine header, payload, and signature into the JWT
        return $"{encodedHeader}.{encodedPayload}.{encodedSignature}";
    }

    /// <summary>
    ///     Validates the JWT token and checks its signature, expiration, and required claims.
    /// </summary>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        if (parts.Length != 3)
            return false;

        var encodedHeader = parts[0];
        var encodedPayload = parts[1];
        var encodedSignature = parts[2];

        // Validate the signature
        var signature = CreateSignature(encodedHeader, encodedPayload);
        var computedEncodedSignature = Base64UrlEncode(signature);
        if (computedEncodedSignature != encodedSignature)
            return false;

        // Decode the payload and validate claims
        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(encodedPayload));
        var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

        if (claims == null)
            return false;

        // Validate expiration
        if (!claims.ContainsKey("exp") || !long.TryParse(claims["exp"].ToString(), out var exp))
            return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > exp)
            return false;

        // Validate issuer and audience
        if (!claims.TryGetValue("iss", out var iss) || iss.ToString() != _issuer)
            return false;

        if (!claims.TryGetValue("aud", out var aud) || aud.ToString() != _audience)
            return false;

        return true;
    }

    /// <summary>
    ///     Decodes the payload of a JWT token without validating its signature.
    /// </summary>
    public Dictionary<string, object> DecodeToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid token format.");

        var encodedPayload = parts[1];
        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(encodedPayload));

        return JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson) ?? [];
    }

    /// <summary>
    ///     Generates a secure refresh token.
    /// </summary>
    public RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            Expires = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            Created = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Creates the HMAC SHA256 signature for the JWT.
    /// </summary>
    private byte[] CreateSignature(string encodedHeader, string encodedPayload)
    {
        var data = Encoding.UTF8.GetBytes($"{encodedHeader}.{encodedPayload}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        return hmac.ComputeHash(data);
    }

    /// <summary>
    ///     Encodes a byte array to a Base64Url string.
    /// </summary>
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-') // Replace '+' with '-'
            .Replace('/', '_') // Replace '/' with '_'
            .TrimEnd('='); // Remove padding
    }

    /// <summary>
    ///     Decodes a Base64Url string to a byte array.
    /// </summary>
    private static byte[] Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2: output += "=="; break;
            case 3: output += "="; break;
        }

        return Convert.FromBase64String(output);
    }
}