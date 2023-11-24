using AuthAPI.DB.Models.Fido;

namespace AuthAPI.DB.Models.WebPushNotifications
{
    public class UserWebPushNotificationSubscription
    {
        public User? User { get; set; }
        public FidoUser? FidoUser { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Url { get; set; }

        public string? P256dh { get; set; }

        public string? Auth { get; set; }
        public Guid? UserAgentId { get; set; }
        public string? FirebaseRegistrationToken { get; set; }
    }
}
