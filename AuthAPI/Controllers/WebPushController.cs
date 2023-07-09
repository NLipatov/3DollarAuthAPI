using AuthAPI.DB.DBContext;
using AuthAPI.Models;
using AuthAPI.Models.Notifications;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserProvider;
using LimpShared.Models.WebPushNotification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebPushController : ControllerBase
    {
        private readonly AuthContext _authContext;
        private readonly IJwtService _jwtService;
        private readonly IUserProvider _userProvider;

        public WebPushController(AuthContext authContext, IJwtService jwtService, IUserProvider userProvider)
        {
            _authContext = authContext;
            _jwtService = jwtService;
            _userProvider = userProvider;
        }

        [HttpPut("notifications/subscribe")]
        public async Task Subscribe(NotificationSubscriptionDTO subscriptionDTO)
        {
            if (string.IsNullOrWhiteSpace(subscriptionDTO.AccessToken))
                throw new ArgumentException
                    ($"Cannot subscribe to web push: " +
                    $"{nameof(subscriptionDTO.AccessToken)} is not a well formed JWT access token.");

            bool accessTokenIsValid = _jwtService.ValidateAccessToken(subscriptionDTO.AccessToken);
            if (!accessTokenIsValid)
                throw new ArgumentException("Cannot subscribe to web push: given access token is not valid.");

            string username = _jwtService.GetUsernameFromAccessToken(subscriptionDTO.AccessToken);

            User? user = await _authContext.Users
                .Include(x=>x.NotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user is null)
                throw new ArgumentException
                    ($"Cannot subscribe to web push: " +
                    $"there is no user with such {nameof(Models.User.Username)} found - '{username}'.");

            _authContext.NotificationSubscriptions.Add(subscriptionDTO.FromDTO(user));

            await _authContext.SaveChangesAsync();
        }
    }
}
