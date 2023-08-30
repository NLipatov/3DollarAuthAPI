using AuthAPI.Extensions.ResponseSerializeExtension;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserCredentialsValidation;
using LimpShared.Models.Authentication.Models;
using LimpShared.Models.Authentication.Models.UserAuthentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AuthAPI.DB.Models;
using AuthAPI.Extensions;
using AuthAPI.Services.JWT.JwtAuthentication;
using AuthAPI.Services.JWT.JwtReading;
using AuthAPI.Services.RefreshHistoryService;
using AuthAPI.Services.UserArea.UserProvider;
using LimpShared.Models.Authentication.Enums;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly IJwtReader _jwtReader;
    private readonly IUserCredentialsValidator _credentialsValidator;
    private readonly IJwtAuthenticationService _jwtManager;
    private readonly IJwtRefreshHistoryService _jwtRefreshHistoryService;

    public AuthController
    (
        IUserProvider userProvider,
        IJwtReader jwtReader,
        IUserCredentialsValidator credentialsValidator,
        IJwtAuthenticationService jwtManager,
        IJwtRefreshHistoryService jwtRefreshHistoryService
    )
    {
        _userProvider = userProvider;
        _jwtReader = jwtReader;
        _credentialsValidator = credentialsValidator;
        _jwtManager = jwtManager;
        _jwtRefreshHistoryService = jwtRefreshHistoryService;
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

    [HttpGet("validate-access-token")]
    public ActionResult<string> ValidateAccessToken(string accesstoken)
    {
        string result;
        if (_jwtManager.ValidateAccessToken(accesstoken))
        {
            result = new TokenRelatedOperationResult
            {
                ResultType = OperationResultType.Success,
                Message = "Token is valid",
            }.AsJson();

        }
        else
        {
            result = new TokenRelatedOperationResult
            {
                ResultType = OperationResultType.Fail,
                FailureType = FailureType.InvalidToken,
                Message = "Token is not valid",
            }.AsJson();
        }

        return Ok(result);
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

    [HttpPost("refresh-tokens-explicitly")]
    public async Task<ActionResult<string>> RefreshTokensExplicitly(RefreshTokenDto dto)
    {
        var users = await _userProvider
            .GetUsersAsync();

        var refreshTokenOwner = users.FirstOrDefault(x => x.RefreshToken == dto.RefreshToken.Token);
        
        if (refreshTokenOwner is null)
            return BadRequest("Invalid refresh token.");
        
        if (DateTime.UtcNow > refreshTokenOwner.RefreshTokenExpires)
            return Unauthorized("Refresh token expired.");

        var user = await _userProvider.GetUserByUsernameAsync(refreshTokenOwner.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{refreshTokenOwner.Username}'.");

        var jwtPair = _jwtManager.CreateJwtPair(user);
        
        //Updating a refresh token to store
        dto.RefreshToken = jwtPair.RefreshToken;
        
        await _userProvider.SaveRefreshTokenAsync(refreshTokenOwner.Username, dto, JwtIssueReason.RefreshToken);

        return Ok(JsonSerializer.Serialize(jwtPair));
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

        await _userProvider.SaveRefreshTokenAsync(request.Username, new RefreshTokenDto
        {
            UserAgent = request.UserAgent,
            UserAgentId = request.UserAgentId,
            RefreshToken = jwtPair.RefreshToken
        }, JwtIssueReason.Login);
        // await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPair.RefreshToken);

        return JsonSerializer.Serialize(jwtPair);
    }
}
