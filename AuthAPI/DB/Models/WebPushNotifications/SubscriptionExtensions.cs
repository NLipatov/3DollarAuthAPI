using AuthAPI.DB.Models.Fido;
using LimpShared.Models.WebPushNotification;

namespace AuthAPI.DB.Models.WebPushNotifications
{
    public static class SubscriptionExtensions
    {
        public static UserWebPushNotificationSubscription FromDto(this NotificationSubscriptionDto notificationSubscriptionDto, User user)
        {
            return new UserWebPushNotificationSubscription
            {
                User = user,
                Auth = notificationSubscriptionDto.Auth,
                Id = notificationSubscriptionDto.Id,
                P256dh = notificationSubscriptionDto.P256dh,
                Url = notificationSubscriptionDto.Url,
                UserAgentId = notificationSubscriptionDto.UserAgentId,
                FirebaseRegistrationToken = notificationSubscriptionDto.FirebaseRegistrationToken
            };
        }
        
        public static UserWebPushNotificationSubscription FromDto(this NotificationSubscriptionDto notificationSubscriptionDto, FidoUser fidoUser)
        {
            return new UserWebPushNotificationSubscription
            {
                FidoUser = fidoUser,
                Auth = notificationSubscriptionDto.Auth,
                Id = notificationSubscriptionDto.Id,
                P256dh = notificationSubscriptionDto.P256dh,
                Url = notificationSubscriptionDto.Url,
                UserAgentId = notificationSubscriptionDto.UserAgentId,
                FirebaseRegistrationToken = notificationSubscriptionDto.FirebaseRegistrationToken
            };
        }

        public static NotificationSubscriptionDto ToDto(this UserWebPushNotificationSubscription notificationSubscription)
        {
            return new NotificationSubscriptionDto
            {
                Auth = notificationSubscription.Auth,
                Id = notificationSubscription.Id,
                Url = notificationSubscription.Url,
                P256dh = notificationSubscription.P256dh,
                UserAgentId = notificationSubscription.UserAgentId.HasValue ? notificationSubscription.UserAgentId.Value : Guid.Empty,
                FirebaseRegistrationToken = notificationSubscription.FirebaseRegistrationToken
            };
        }
    }
}
