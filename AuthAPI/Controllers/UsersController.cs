using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserProvider;
using LimpShared.Authentification;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public async Task<ActionResult<TokenRelatedOperationResult>> GetUserName(string accessToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if(!tokenHandler.CanReadToken(accessToken))
            {
                return Unauthorized(new TokenRelatedOperationResult
                {
                    ResultType = TokenRelatedOperationResultType.Fail,
                    FailureType = FailureType.InvalidToken
                });
            }

            if (tokenHandler.ReadToken(accessToken).ValidTo < DateTime.UtcNow)
            {
                return Unauthorized(new TokenRelatedOperationResult
                {
                    ResultType = TokenRelatedOperationResultType.Fail,
                    FailureType = FailureType.ExpiredToken,
                });
            }

            if (!_jwtService.ValidateAccessToken(accessToken))
                return Unauthorized(new TokenRelatedOperationResult
                {
                    ResultType = TokenRelatedOperationResultType.Fail,
                    FailureType = FailureType.InvalidToken
                });


            JwtSecurityToken securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            return Ok(
                new TokenRelatedOperationResult
                {
                    ResultType = TokenRelatedOperationResultType.Success,
                    Username = securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ?? "Anonymous",
                });
                
        }
    }
}
