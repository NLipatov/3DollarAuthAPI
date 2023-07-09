using LimpShared.Models.WebPushNotification;

namespace AuthAPI.Models.Notifications
{
    public static class SubscriptionExtensions
    {
        public static UserNotificationSubscription FromDTO(this NotificationSubscriptionDTO notificationSubscriptionDTO, User user)
        {
            return new UserNotificationSubscription
            {
                User = user,
                Auth = notificationSubscriptionDTO.Auth,
                Id = notificationSubscriptionDTO.Id,
                P256dh = notificationSubscriptionDTO.P256dh,
                Url = notificationSubscriptionDTO.Url,
            };
        }
    }
}
