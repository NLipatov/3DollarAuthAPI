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

        public static NotificationSubscriptionDTO ToDTO(this UserNotificationSubscription notificationSubscription)
        {
            return new NotificationSubscriptionDTO
            {
                Auth = notificationSubscription.Auth,
                Id = notificationSubscription.Id,
                Url = notificationSubscription.Url,
                P256dh = notificationSubscription.P256dh
            };
        }
    }
}
