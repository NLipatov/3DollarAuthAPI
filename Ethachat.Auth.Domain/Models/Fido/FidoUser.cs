using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ethachat.Auth.Domain.Models.WebPushNotifications;
using Fido2NetLib;

namespace Ethachat.Auth.Domain.Models.Fido
{
    public class FidoUser
    {
        [Key]
        public Guid RecordId { get; set; }
        /// <summary>
        /// Required. A human-friendly identifier for a user account. It is intended only for display, i.e., aiding the user in determining the difference between user accounts with similar displayNames. For example, "alexm", "alex.p.mueller@example.com" or "+14255551234". https://w3c.github.io/webauthn/#dictdef-publickeycredentialentity
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The user handle of the user account entity. To ensure secure operation, authentication and authorization decisions MUST be made on the basis of this id member, not the displayName nor name members
        /// </summary>
        [JsonPropertyName("id")]
        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] UserId { get; set; }

        /// <summary>
        /// A human-friendly name for the user account, intended only for display. For example, "Alex P. Müller" or "田中 倫". The Relying Party SHOULD let the user choose this, and SHOULD NOT restrict the choice more than necessary.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Additional related to user information as RSA Pulic keys
        /// </summary>
        public List<UserClaim>? Claims { get; set; } = new();
        public List<UserWebPushNotificationSubscription> UserWebPushNotificationSubscriptions { get; set; } = new();
    }
}
