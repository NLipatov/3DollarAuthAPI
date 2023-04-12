using AuthAPI.DTOs.Claims;
using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.JWT;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.ResponseSerializer;
using AuthAPI.Services.UserCredentialsValidation;
using AuthAPI.Services.UserProvider;
using LimpShared.Authentification;
using LimpShared.DTOs.User;
using LimpShared.ResultTypeEnum;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly IJwtService _jwtService;
    private readonly IUserCredentialsValidator _credentialsValidator;
    private readonly IResponseSerializer _responseSerializer;

    public AuthController
        (
            IUserProvider userProvider,
            IJwtService jwtService,
            IUserCredentialsValidator credentialsValidator,
            IResponseSerializer responseSerializer
        )
    {
        _userProvider = userProvider;
        _jwtService = jwtService;
        _credentialsValidator = credentialsValidator;
        _responseSerializer = responseSerializer;
    }

    [HttpPost("Register")]
    public async Task<ActionResult<UserOperationResult>> Register(UserDTO request)
    {
        string contextId = HttpContext.Session.Id;
        return await _userProvider.RegisterUser(request, request?.Claims?.ExtractClaims());
    }

    [HttpGet("Get-claim")]
    public ActionResult<TokenClaim> ReadClaim(string token, string claimName)
    {
        TokenClaim? result = _jwtService.GetClaim(token, claimName);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpGet("Get-token-claims")]
    public ActionResult<List<TokenClaim>> ReadClaims(string token)
    {
        return Ok(_jwtService.GetTokenClaims(token));
    }

    [HttpGet("Validate-access-token")]
    public ActionResult<string> ValidateAccessToken(string accesstoken)
    {
        if (_jwtService.ValidateAccessToken(accesstoken))
            return Ok(
            _responseSerializer.Serialize(
                new TokenRelatedOperationResult
            {
                ResultType = OperationResultType.Success,
                Message = "Token is valid",
            }));

        return Ok(
            _responseSerializer.Serialize(
            new TokenRelatedOperationResult
        {
            ResultType = OperationResultType.Fail,
            FailureType = FailureType.InvalidToken,
            Message = "Token is not valid",
        }));
    }

    /// <summary>
    /// Метод для получения валидного токена
    /// По логину и паролю юзера из текущего userProvider можно получить JWT
    /// <paramref name="request"/>
    /// </summary>
    [HttpPost("get-token")]
    public async Task<ActionResult<string>> GetToken(UserDTO request)
    {
        ValidationResult ValidationResult = await _credentialsValidator.ValidateCredentials(request);

        if (ValidationResult == ValidationResult.Success)
        {
            return await ProvideAccessAndRefreshTokensAsync(request);
        }
        else
        {
            return ValidationResult switch
            {
                ValidationResult.WrongPassword => (ActionResult<string>)BadRequest("Wrong Password"),
                ValidationResult.WrongUsername => (ActionResult<string>)BadRequest("Wrong Username"),
                _ => throw new Exception("Could not handle request properly because of occured unhadled exception"),
            };
        }
    }

    /// <summary>
    /// Фронт решает сам когда ему дёрнуть эту ручку и обновить Access Token и Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<string>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        //В хранилище юзеров смотрим, есть ли у нас юзер, которому был выдан этот refreshToken
        User? refreshTokenOwner = (await _userProvider.GetUsersAsync()).FirstOrDefault(x => x.RefreshToken == refreshToken);

        if (refreshTokenOwner == null)
        {
            return BadRequest("Invalid refresh token");
        }
        else if (refreshTokenOwner.RefreshTokenExpires < DateTime.UtcNow)
        {
            return Unauthorized("Token expired");
        }

        JWTPair jwtPair = await _jwtService.CreateJWTPairAsync(_userProvider, refreshTokenOwner.Username);

        await SetRefreshToken(jwtPair.RefreshToken,
            new UserDTO { Username = refreshTokenOwner.Username });

        return Ok(jwtPair.AccessToken);
    }

    [HttpPost("refresh-tokens-explicitly")]
    public async Task<ActionResult<string>> RefreshTokensExplicitly(RefreshToken refreshToken)
    {
        //В хранилище юзеров смотрим, есть ли у нас юзер, которому был выдан этот refreshToken
        var users = await _userProvider.GetUsersAsync();

        User? refreshTokenOwner = users.FirstOrDefault(x => x.RefreshToken == refreshToken.Token);

        if (refreshTokenOwner == null)
        {
            return BadRequest("Invalid refresh token");
        }
        else if (refreshTokenOwner.RefreshTokenExpires < DateTime.UtcNow)
        {
            return Unauthorized("Token expired");
        }

        JWTPair jwtPair = await _jwtService.CreateJWTPairAsync(_userProvider, refreshTokenOwner.Username);

        await _userProvider.SaveRefreshTokenAsync(refreshTokenOwner.Username, jwtPair.RefreshToken);

        return Ok(JsonSerializer.Serialize(jwtPair));
    }

    private async Task SetRefreshToken(RefreshToken newRefreshToken, UserDTO userDTO)
    {
        await _userProvider.SaveRefreshTokenAsync(userDTO.Username, newRefreshToken);

        var cookieOption = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.Expires,
        };

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOption);
    }

    /// <summary>
    /// Provides an access token in response payload and stores refresh token in Cookies
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string> ProvideTokensAsync(UserDTO request)
    {
        JWTPair jwtPait = await _jwtService.CreateJWTPairAsync(_userProvider, request.Username);

        await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPait.RefreshToken);

        await SetRefreshToken(jwtPait.RefreshToken, request);

        return jwtPait.AccessToken;
    }

    /// <summary>
    /// Provides both access and refresh token in response payload
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<string> ProvideAccessAndRefreshTokensAsync(UserDTO request)
    {
        JWTPair jwtPait = await _jwtService.CreateJWTPairAsync(_userProvider, request.Username);

        await _userProvider.SaveRefreshTokenAsync(request.Username, jwtPait.RefreshToken);

        return JsonSerializer.Serialize(jwtPait);
    }
}
