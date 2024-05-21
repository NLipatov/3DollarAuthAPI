using EthachatShared.Models.Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using AuthAPI.Services.JWT.JwtAuthentication;
using AuthAPI.Services.JWT.JwtReading;
using AuthAPI.Services.UserArea.PublicKeyManager;
using AuthAPI.Services.UserArea.UserProvider;
using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.ModelExtensions;
using EthachatShared.Models.Authentication.Enums;
using EthachatShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using EthachatShared.Models.Users;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserProvider _userProvider;
        private readonly IJwtReader _jwtReader;
        private readonly IPublicKeyManager _publicKeyManager;
        private readonly IJwtAuthenticationManager _jwtManager;

        public UsersController
            (IUserProvider userProvider,
                IJwtReader jwtReader,
                IPublicKeyManager publicKeyManager,
                IJwtAuthenticationManager jwtManager)
        {
            _userProvider = userProvider;
            _jwtReader = jwtReader;
            _publicKeyManager = publicKeyManager;
            _jwtManager = jwtManager;
        }

        [HttpGet("user/{username}/exist")]
        public async Task<ActionResult<IsUserExistDto>> GetUser(string username)
        {
            IsUserExistDto result = await _userProvider.IsUserExist(username);

            return Ok(result);
        }

        [HttpGet("UsersOnline")]
        public async Task<ActionResult<List<User>>> GetUsersOnline()
        {
            var users = await _userProvider.GetUsersOnline();

            var dtos = users
                .Select(x => x.ToDto())
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
        public async Task SetRsaPublicKey(PublicKeyDto publicKeyDto) =>
            await _publicKeyManager.SetRsaPublic(publicKeyDto);

        [HttpGet("RSAPublic/{username}")]
        public async Task<string?> GetRsaPublicKey(string username) => await _publicKeyManager.GetRsaPublic(username);
    }
}
