using AuthAPI.DB.DBContext;
using AuthAPI.DB.Models;
using AuthAPI.DB.Models.Fido;
using AuthAPI.DB.Models.WebPushNotifications;
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
        private readonly IJwtAuthenticationManager _jwtManager;

        public WebPushController
            (AuthContext authContext, 
                IJwtReader jwtReader, 
                IUserProvider userProvider, 
                IJwtAuthenticationManager jwtManager)
        {
            _authContext = authContext;
            _jwtReader = jwtReader;
            _userProvider = userProvider;
            _jwtManager = jwtManager;
        }

        [HttpPatch("notifications/remove")]
        public async Task DeleteSubsciptions(NotificationSubscriptionDto[] subscriptionDtOs)
        {
            var credentials = subscriptionDtOs
                .FirstOrDefault(x => x.JwtPair is not null || x.WebAuthnPair is not null);

            if (credentials?.JwtPair is not null)
            {
                var jwtPair = credentials.JwtPair;
                var accessToken = jwtPair?.AccessToken ?? string.Empty;

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException
                    ($"Cannot delete web push subscriptions: " +
                     $"Given access token is not a well formed JWT access token.");

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
            else if (credentials?.WebAuthnPair is not null)
            {
                var webAuthnPair = credentials.WebAuthnPair;
                var credentialId = webAuthnPair?.CredentialId ?? string.Empty;

                if (string.IsNullOrWhiteSpace(credentialId))
                    throw new ArgumentException
                    ($"Cannot delete web push subscriptions: " +
                     $"Given WebAuthn CredentialId was an empty string.");
                
                var credentialIdBytes = Convert.FromBase64String(Uri.UnescapeDataString(credentialId));
                
                string username = await _userProvider.GetUsernameByCredentialId(credentialIdBytes);

                FidoUser? fidoUser = await _authContext.FidoUsers
                    .Include(x => x.UserWebPushNotificationSubscriptions)
                    .FirstOrDefaultAsync(x => x.Name == username);

                if (fidoUser is null)
                    throw new ArgumentException($"There is no {nameof(FidoUser)} with such username — {username}.");

                var targetSubscriptions = fidoUser.UserWebPushNotificationSubscriptions.Where(x => subscriptionDtOs.Any(s => s.Id == x.Id));

                _authContext.RemoveRange(targetSubscriptions);

                await _authContext.SaveChangesAsync();
            }
        }

        [HttpGet("notifications/{username}")]
        public async Task<NotificationSubscriptionDto[]> GetSubscriptions(string username)
        {
            #warning ToDo: single handler for user and fidoUser?
            
            User? user = await _authContext.Users
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            FidoUser? fidoUser = await _authContext.FidoUsers.Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Name == username);

            if (user is not null)
                return user.UserWebPushNotificationSubscriptions.Select(x => x.ToDto()).ToArray();
            if (fidoUser is not null)
                return fidoUser.UserWebPushNotificationSubscriptions.Select(x => x.ToDto()).ToArray();

            throw new ArgumentException($"There is no user with such username — {username}.");
        }

        [HttpPut("notifications/subscribe")]
        public async Task AddSubscription(NotificationSubscriptionDto subscriptionDto)
        {
            if (subscriptionDto.WebAuthnPair is null && subscriptionDto.JwtPair is null)
                throw new ArgumentException
                    ($"Cannot subscribe to web push: " +
                    $"{nameof(subscriptionDto.JwtPair)} and {nameof(subscriptionDto.WebAuthnPair)} are both nulls.");

            #warning ToDo: implement a validator for WebAuthN
            string username = string.Empty;
            if (subscriptionDto.JwtPair is not null)
            {
                var jwtPair = subscriptionDto.JwtPair;
                var accessToken = jwtPair?.AccessToken ?? string.Empty;
             
                username = _jwtReader.GetUsernameFromAccessToken(accessToken);
            }
            else if (subscriptionDto.WebAuthnPair is not null)
            {
                var webAuthnPair = subscriptionDto.WebAuthnPair;
                var credentialId = webAuthnPair?.CredentialId ?? string.Empty;
                var credentialIdBytes = Convert.FromBase64String(Uri.UnescapeDataString(credentialId));
                
                username = await _userProvider.GetUsernameByCredentialId(credentialIdBytes);
            }

            User? user = await _authContext.Users
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Username == username);

            FidoUser? fidoUser = await _authContext.FidoUsers
                .Include(x => x.UserWebPushNotificationSubscriptions)
                .FirstOrDefaultAsync(x => x.Name == username);
            
            if (user is null && fidoUser is null)
            {
                throw new ArgumentException
                    ($"There's no {nameof(FidoUser)} or {nameof(DB.Models.User)} matching given credentials.");
            }
            else if (user is not null)
            {
                _authContext.WebPushNotificationSubscriptions.Add(subscriptionDto.FromDto(user));
                await _authContext.SaveChangesAsync();
            }
            else if (fidoUser is not null)
            {
                _authContext.WebPushNotificationSubscriptions.Add(subscriptionDto.FromDto(fidoUser));
                await _authContext.SaveChangesAsync();
            }
        }
    }
}
