using AuthAPI.DTOs;
using AuthAPI.Models;
using AuthAPI.Services.JWT;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.UserCredentialsValidation;
using AuthAPI.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly IJwtService _jwtService;
    private readonly IUserCredentialsValidator _credentialsValidator;
    private readonly IConfiguration _configuration;

    public AuthController
        (
            IUserProvider userProvider,
            IJwtService jwtService,
            IUserCredentialsValidator credentialsValidator,
            IConfiguration configuration
        )
    {
        _userProvider = userProvider;
        _jwtService = jwtService;
        _credentialsValidator = credentialsValidator;
        _configuration = configuration;
    }

    /// <summary>
    /// Метод для получения валидного токена
    /// По логину и паролю юзера из текущего userProvider можно получить JWT
    /// <paramref name="request"/>
    /// </summary>
    [HttpPost("get-JWT")]
    public async Task<ActionResult<string>> GetJWT(UserDTO request)
    {
        ValidationResult ValidationResult = await _credentialsValidator.ValidateCredentials(request);

        if (ValidationResult == ValidationResult.Success)
        {
            string accessToken = _jwtService.GenerateAccessToken(await _userProvider.GetUserByUsername(request.Username)
            ??
            throw new Exception("Unexpected error occured on attempt to generate JWT"));

            var refreshToken = _jwtService.GenerateRefreshToken();

            await SetRefreshToken(refreshToken, request);

            return accessToken;
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
        User? refreshTokenOwner = (await _userProvider.GetUsers()).FirstOrDefault(x => x.RefreshToken == refreshToken);

        if (refreshTokenOwner == null)
        {
            return BadRequest("Invalid refresh token");
        }
        else if (refreshTokenOwner.TokenExpires < DateTime.Now)
        {
            return Unauthorized("Token expired");
        }

        string accessToken = _jwtService.GenerateAccessToken(refreshTokenOwner);
        await SetRefreshToken(_jwtService.GenerateRefreshToken(),
            new UserDTO { Username = refreshTokenOwner.Username });

        return Ok(accessToken);
    }

    private async Task SetRefreshToken(IRefreshToken newRefreshToken, UserDTO userDTO)
    {
        await _userProvider.AssignRefreshToken(userDTO.Username, newRefreshToken);

        var cookieOption = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.Expires,
        };

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOption);
    }
}
