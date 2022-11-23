using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserProvider;
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
        public async Task<ActionResult<string>> GetUserName(string accessToken)
        {
            bool isTokenValid = _jwtService.ValidateAccessToken(accessToken);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            return securityToken!.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ?? "Anonymous";
        }
    }
}
