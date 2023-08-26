using AuthAPI.DB.DBContext;
using AuthAPI.DB.Models;
using AuthAPI.DB.Models.WebPushNotifications;
using AuthAPI.Services.JWT;
using AuthAPI.Services.JWT.JwtAuthentication;
using AuthAPI.Services.JWT.JwtReading;
using AuthAPI.Services.UserArea.UserProvider;
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
        private readonly IJwtReader _jwtReader;
        private readonly IUserProvider _userProvider;
        private readonly IJwtAuthenticationService _jwtManager;

        public WebPushController(AuthContext authContext, IJwtReader jwtReader, IUserProvider userProvider, IJwtAuthenticationService jwtManager)
        {
            _authContext = authContext;
            _jwtReader = jwtReader;
            _userProvider = userProvider;
            _jwtManager = jwtManager;
        }

        [HttpPatch("notifications/remove")]
        public async Task DeleteSubsciptions(NotificationSubscriptionDto[] subscriptionDtOs)
        {
            string? accessToken = subscriptionDtOs
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.AccessToken))
                ?.AccessToken;

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException
                    ($"Cannot delete web push subscriptions: " +
                    $"{nameof(NotificationSubscriptionDto.AccessToken)} is not a well formed JWT access token.");

            bool accessTokenIsValid = _jwtManager.ValidateAccessToken(accessToken);
            if (!accessTokenIsValid)
                throw new ArgumentException("Cannot delete web push subscriptions: given access token is not valid.");

            string username = _jwtReader.GetUsernameFromAccessToken(accessToken);

            User? user = await _authContext.Users
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user is null)
                throw new ArgumentException($"There is no {nameof(User)} with such username — {username}.");

            var targetSubscriptions = user.UserWebPushNotificationSubscriptions.Where(x => subscriptionDtOs.Any(s => s.Id == x.Id));

            _authContext.RemoveRange(targetSubscriptions);

            await _authContext.SaveChangesAsync();
        }

        [HttpGet("notifications/{username}")]
        public async Task<NotificationSubscriptionDto[]> GetSubscriptions(string username)
        {
            User? user = await _authContext.Users
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user is null)
                throw new ArgumentException($"There is no {nameof(User)} with such username — {username}.");

            return user.UserWebPushNotificationSubscriptions.Select(x => x.ToDto()).ToArray();
        }

        [HttpPut("notifications/subscribe")]
        public async Task Subscribe(NotificationSubscriptionDto subscriptionDto)
        {
            if (string.IsNullOrWhiteSpace(subscriptionDto.AccessToken))
                throw new ArgumentException
                    ($"Cannot subscribe to web push: " +
                    $"{nameof(subscriptionDto.AccessToken)} is not a well formed JWT access token.");

            bool accessTokenIsValid = _jwtManager.ValidateAccessToken(subscriptionDto.AccessToken);
            if (!accessTokenIsValid)
                throw new ArgumentException("Cannot subscribe to web push: given access token is not valid.");

            string username = _jwtReader.GetUsernameFromAccessToken(subscriptionDto.AccessToken);

            User? user = await _authContext.Users
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user is null)
                throw new ArgumentException
                    ($"Cannot subscribe to web push: " +
                    $"there is no user with such {nameof(DB.Models.User.Username)} found - '{username}'.");

            _authContext.WebPushNotificationSubscriptions.Add(subscriptionDto.FromDto(user));

            await _authContext.SaveChangesAsync();
        }
    }
}
