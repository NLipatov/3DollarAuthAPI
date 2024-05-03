using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserCredentialsValidation;
using EthachatShared.Models.Authentication.Models;
using EthachatShared.Models.Authentication.Models.UserAuthentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AuthAPI.Extensions;
using AuthAPI.Services.AuthenticationManager;
using AuthAPI.Services.JWT.JwtAuthentication;
using AuthAPI.Services.JWT.JwtReading;
using AuthAPI.Services.RefreshHistoryService;
using AuthAPI.Services.UserArea.UserProvider;
using EthachatShared.Models.Authentication.Enums;
using EthachatShared.Models.Authentication.Models.Credentials.CredentialsDTO;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly IUserProvider _userProvider;
    private readonly IJwtReader _jwtReader;
    private readonly IUserCredentialsValidator _credentialsValidator;
    private readonly IJwtAuthenticationManager _jwtManager;
    private readonly IJwtRefreshHistoryService _jwtRefreshHistoryService;
    private readonly IAuthenticationManager<JwtPair> _jwtAuthenticationManager;
    private readonly IAuthenticationManager<WebAuthnPair> _webAuthnAuthenticationManager;

    public AuthController
    (
        IUserProvider userProvider,
        IJwtReader jwtReader,
        IUserCredentialsValidator credentialsValidator,
        IJwtAuthenticationManager jwtManager,
        IJwtRefreshHistoryService jwtRefreshHistoryService,
        IAuthenticationManager<JwtPair> jwtAuthenticationManager,
        IAuthenticationManager<WebAuthnPair> webAuthnAuthenticationManager
    )
    {
        _userProvider = userProvider;
        _jwtReader = jwtReader;
        _credentialsValidator = credentialsValidator;
        _jwtManager = jwtManager;
        _jwtRefreshHistoryService = jwtRefreshHistoryService;
        _jwtAuthenticationManager = jwtAuthenticationManager;
        _webAuthnAuthenticationManager = webAuthnAuthenticationManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserAuthenticationOperationResult>> Register(UserAuthentication request)
    {
        string contextId = HttpContext.Session.Id;
        return await _userProvider.RegisterUser(request, request?.Claims?.ExtractClaims());
    }

    [HttpGet("token/{token}/claims/{claimName}")]
    public ActionResult<TokenClaim> ReadClaim(string token, string claimName)
    {
        TokenClaim? result = _jwtReader.GetClaim(token, claimName);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("token/{token}/claims")]
    public ActionResult<List<TokenClaim>> ReadClaims(string token)
    {
        return Ok(_jwtReader.GetTokenClaims(token));
    }

    [HttpGet("refresh-history")]
    public async Task<ActionResult<List<AccessRefreshEventLog>>> GetUsersRefreshHistory
        (string accessToken)
    {
        var isTokenValid = _jwtManager.ValidateAccessToken(accessToken);
        var username = _jwtReader.GetUsernameFromAccessToken(accessToken);

        if (!isTokenValid)
            throw new ArgumentException($"Given access token is not valid.");

        var history = await _jwtRefreshHistoryService.GetUserHistory(username);

        return Ok(history);
    }

    [HttpPost("refresh")]
    public async Task<AuthResult> Refresh(CredentialsDTO dto)
    {
        AuthResult? authResult = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(dto.JwtPair?.AccessToken))
            {
                var refreshedCredentials = await _jwtAuthenticationManager.RefreshCredentials(dto.JwtPair);
                authResult = new()
                {
                    Result = refreshedCredentials is not null
                        ? AuthResultType.Success
                        : AuthResultType.Fail,
                    JwtPair = refreshedCredentials
                };
            }
            else if (!string.IsNullOrWhiteSpace(dto.WebAuthnPair?.CredentialId))
            {
                var refreshedCredentials = await _webAuthnAuthenticationManager.RefreshCredentials(dto.WebAuthnPair);
                authResult = new()
                {
                    Result = refreshedCredentials is not null
                        ? AuthResultType.Success
                        : AuthResultType.Fail,
                    CredentialId = refreshedCredentials?.CredentialId ?? string.Empty
                };
            }
        }
        catch (Exception e)
        {
            return authResult ?? new AuthResult {Result = AuthResultType.Fail, Message = e.Message };
        }
        
        return authResult ?? new AuthResult {Result = AuthResultType.Fail };
    }

    [HttpPost("username")]
    public async Task<AuthResult> GetUsernameByCredentials(CredentialsDTO credentialsDto)
    {
        return await _userProvider.GetUsernameByCredentials(credentialsDto);
    }

    [HttpPost("validate")]
    public async Task<AuthResult> Validate(CredentialsDTO dto)
    {
        bool isTokenValid = false;
        if (!string.IsNullOrWhiteSpace(dto.JwtPair?.AccessToken))
        {
            isTokenValid = await _jwtAuthenticationManager.ValidateCredentials(dto.JwtPair);
        }
        else if (!string.IsNullOrWhiteSpace(dto.WebAuthnPair?.CredentialId))
        {
            isTokenValid = await _webAuthnAuthenticationManager.ValidateCredentials(dto.WebAuthnPair);
        }

        return new()
        {
            Result = isTokenValid ? AuthResultType.Success : AuthResultType.Fail,
            Message = isTokenValid ? "Token is valid." : "Token is not valid."
        };
    }

    [HttpPost("get-token")]
    public async Task<ActionResult<string>> GetToken(UserAuthentication request)
    {
        var validationResult = await _credentialsValidator.ValidateCredentials(request);

        if (validationResult == ValidationResult.Success)
            return await GenerateJwtPair(request);

        return validationResult switch
        {
            ValidationResult.WrongPassword => (ActionResult<string>)BadRequest("Wrong Password"),
            ValidationResult.WrongUsername => (ActionResult<string>)BadRequest("Wrong Username"),
            _ => throw new Exception("Could not handle request properly because of occured unhadled exception"),
        };
    }

    /// <summary>
    /// Provides both access and refresh token in response payload
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string> GenerateJwtPair(UserAuthentication request)
    {
        var user = await _userProvider.GetUserByUsernameAsync(request.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{request.Username}'.");

        JwtPair jwtPair = _jwtManager.CreateJwtPair(user);

        await _userProvider.SaveRefreshTokenAsync(request.Username, new RefreshToken()
        {
            Token = jwtPair.RefreshToken.Token,
            Created = jwtPair.RefreshToken.Created,
            Expires = jwtPair.RefreshToken.Expires
        }, JwtIssueReason.Login);
        // await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPair.RefreshToken);

        return JsonSerializer.Serialize(jwtPair);
    }
}
