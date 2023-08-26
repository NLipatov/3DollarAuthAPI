using AuthAPI.Mapping;
using AuthAPI.Models;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserProvider;
using LimpShared.Models.Authentication.Models;
using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using LimpShared.Models.AuthenticationModels.ResultTypeEnum;
using LimpShared.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using AuthAPI.Services.JWT.JwtAuthentication;
using AuthAPI.Services.JWT.JwtReading;
using AuthAPI.Services.UserArea.UserPublicKeyManager;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserProvider _userProvider;
        private readonly IJwtReader _jwtReader;
        private readonly IPublicKeyManager _publicKeyManager;
        private readonly IJwtAuthenticationService _jwtManager;

        public UsersController
            (IUserProvider userProvider,
                IJwtReader jwtReader,
                IPublicKeyManager publicKeyManager,
                IJwtAuthenticationService jwtManager)
        {
            _userProvider = userProvider;
            _jwtReader = jwtReader;
            _publicKeyManager = publicKeyManager;
            _jwtManager = jwtManager;
        }

        [HttpGet("user/{username}/exist")]
        public async Task<ActionResult<IsUserExistDTO>> GetUser(string username)
        {
            IsUserExistDTO result = await _userProvider.IsUserExist(username);

            return Ok(result);
        }

        [HttpGet("UsersOnline")]
        public async Task<ActionResult<List<User>>> GetUsersOnline()
        {
            var users = await _userProvider.GetUsersOnline();

            var dtos = users
                .Select(x => x.ToDTO())
                .ToList();

            return Ok(dtos);
        }

        [HttpGet("GetUserName")]
        public ActionResult<string> GetUserName(string accessToken)
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

            if (!_jwtManager.ValidateAccessToken(accessToken))
                return Unauthorized(JsonSerializer.Serialize(new TokenRelatedOperationResult
                {
                    ResultType = OperationResultType.Fail,
                    FailureType = FailureType.InvalidToken
                }));


            JwtSecurityToken? securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            var username = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ?? "Anonymous";

            var result = new TokenRelatedOperationResult
            {
                ResultType = OperationResultType.Success,
                Username = username,
            };

            return Ok(JsonSerializer.Serialize(result));
        }

        [HttpPost("RSAPublic")]
        public async Task SetRsaPublicKey(PublicKeyDTO publicKeyDto) =>
            await _publicKeyManager.SetRsaPublic(publicKeyDto);

        [HttpGet("RSAPublic/{username}")]
        public async Task<string?> GetRsaPublicKey(string username) => await _publicKeyManager.GetRsaPublic(username);
    }
}
