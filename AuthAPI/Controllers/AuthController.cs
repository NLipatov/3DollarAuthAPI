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

    public AuthController
    (
        IUserProvider userProvider,
        IJwtReader jwtReader,
        IUserCredentialsValidator credentialsValidator,
        IJwtAuthenticationService jwtManager
    )
    {
        _userProvider = userProvider;
        _jwtReader = jwtReader;
        _credentialsValidator = credentialsValidator;
        _jwtManager = jwtManager;
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
        ValidationResult validationResult = await _credentialsValidator.ValidateCredentials(request);

        if (validationResult == ValidationResult.Success)
        {
            return await ProvideAccessAndRefreshTokensAsync(request);
        }
        else
        {
            return validationResult switch
            {
                ValidationResult.WrongPassword => (ActionResult<string>)BadRequest("Wrong Password"),
                ValidationResult.WrongUsername => (ActionResult<string>)BadRequest("Wrong Username"),
                _ => throw new Exception("Could not handle request properly because of occured unhadled exception"),
            };
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<string>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        //В хранилище юзеров смотрим, есть ли у нас юзер, которому был выдан этот refreshToken
        User? refreshTokenOwner =
            (await _userProvider.GetUsersAsync()).FirstOrDefault(x => x.RefreshToken == refreshToken);

        if (refreshTokenOwner == null)
        {
            return BadRequest("Invalid refresh token");
        }
        else if (refreshTokenOwner.RefreshTokenExpires < DateTime.UtcNow)
        {
            return Unauthorized("Token expired");
        }

        var user = await _userProvider.GetUserByUsernameAsync(refreshTokenOwner.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{refreshTokenOwner.Username}'.");

        var jwtPair = _jwtManager.CreateJwtPair(user);

        await SetRefreshToken(jwtPair.RefreshToken,
            new UserAuthentication { Username = refreshTokenOwner.Username });

        return Ok(jwtPair.AccessToken);
    }

    [HttpPost("refresh-tokens-explicitly")]
    public async Task<ActionResult<string>> RefreshTokensExplicitly(RefreshTokenDto dto)
    {
        var users = await _userProvider.GetUsersAsync();

        User? refreshTokenOwner = users.FirstOrDefault(x => x.RefreshToken == dto.RefreshToken.Token);

        if (refreshTokenOwner == null)
        {
            return BadRequest("Invalid refresh token");
        }
        else if (refreshTokenOwner.RefreshTokenExpires < DateTime.UtcNow)
        {
            return Unauthorized("Token expired");
        }

        var user = await _userProvider.GetUserByUsernameAsync(refreshTokenOwner.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{refreshTokenOwner.Username}'.");

        JwtPair jwtPair = _jwtManager.CreateJwtPair(user);

        await _userProvider.SaveRefreshTokenAsync(refreshTokenOwner.Username, dto);

        return Ok(JsonSerializer.Serialize(jwtPair));
    }

    private async Task SetRefreshToken(RefreshToken refreshToken, UserAuthentication userDto)
    {
        await _userProvider.SaveRefreshTokenAsync(userDto.Username, refreshToken);

        var cookieOption = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.Expires,
        };

        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOption);
    }

    /// <summary>
    /// Provides an access token in response payload and stores refresh token in Cookies
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string> ProvideTokensAsync(UserAuthentication request)
    {
        var user = await _userProvider.GetUserByUsernameAsync(request.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{request.Username}'.");

        JwtPair jwtPair = _jwtManager.CreateJwtPair(user);

        await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPair.RefreshToken);

        await SetRefreshToken(jwtPair.RefreshToken, request);

        return jwtPair.AccessToken;
    }

    /// <summary>
    /// Provides both access and refresh token in response payload
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string> ProvideAccessAndRefreshTokensAsync(UserAuthentication request)
    {
        var user = await _userProvider.GetUserByUsernameAsync(request.Username)
                   ??
                   throw new ArgumentException($"There's no user with such username: '{request.Username}'.");

        JwtPair jwtPair = _jwtManager.CreateJwtPair(user);

        await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPair.RefreshToken);

        return JsonSerializer.Serialize(jwtPair);
    }
}
