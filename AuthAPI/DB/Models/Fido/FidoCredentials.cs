using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;

namespace AuthAPI.DB.Models.Fido
{
    public class FidoCredential
    {
        [Key]
        public Guid RecordId { get; set; }
        public byte[] UserId { get; set; }
        public byte[] DescriptorId { get; set; }
        public PublicKeyCredentialType DescriptorType { get; set; }
        public AuthenticatorTransport[]? DescriptorTransports { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }
}
