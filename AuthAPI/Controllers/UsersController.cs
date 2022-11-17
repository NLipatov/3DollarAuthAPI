using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserProvider _userProvider;
        public UsersController(IUserProvider userProvider)
        {
            _userProvider = userProvider;
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
    }
}
