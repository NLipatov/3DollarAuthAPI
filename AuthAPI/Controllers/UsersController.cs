using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserProvider;
using LimpShared.Authentification;
using LimpShared.DTOs.PublicKey;
using LimpShared.ResultTypeEnum;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserProvider _userProvider;
        private readonly IJwtService _jwtService;
        public UsersController(IUserProvider userProvider, IJwtService jwtService)
        {
            _userProvider = userProvider;
            _jwtService = jwtService;
        }
        [HttpGet("UsersOnline")]
        public async Task<ActionResult<List<UserDTO>>> GetUsersOnline()
        {
            var users = await _userProvider.GetUsersOnline();

            var dtos = users
                .Select(x => x.ToDTO())
                .ToList();

            return Ok(dtos);
        }

        [HttpGet("GetUserName")]
        public async Task<ActionResult<string>> GetUserName(string accessToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(accessToken))
            {
                return Unauthorized(JsonSerializer.Serialize(new TokenRelatedOperationResult
                {
                    ResultType = OperationResultType.Fail,
                    FailureType = FailureType.InvalidToken
                }));
            }

            if (tokenHandler.ReadToken(accessToken).ValidTo < DateTime.UtcNow)
            {
                return Unauthorized(JsonSerializer.Serialize(new TokenRelatedOperationResult
                {
                    ResultType = OperationResultType.Fail,
                    FailureType = FailureType.ExpiredToken,
                }));
            }

            if (!_jwtService.ValidateAccessToken(accessToken))
                return Unauthorized(JsonSerializer.Serialize(new TokenRelatedOperationResult
                {
                    ResultType = OperationResultType.Fail,
                    FailureType = FailureType.InvalidToken
                }));


            JwtSecurityToken securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            var username = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ?? "Anonymous";

            var result = new TokenRelatedOperationResult
            {
                ResultType = OperationResultType.Success,
                Username = username,
            };

            return Ok(JsonSerializer.Serialize(result));
        }
        [HttpPost("SetRSAPublicKey")]
        public async Task SetRSAPublicKey(PublicKeyDTO publicKeyDTO)
        {
            await _userProvider.SetUserPublicKeyAsync(publicKeyDTO.Key, publicKeyDTO.Username);
        }
    }
}
