using Fido2NetLib.Objects;
using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models.Fido2
{
    public class FidoCredential
    {
        [Key]
        public Guid RecordId { get; set; }
        public byte[] UserId { get; set; }
        public PublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }
}
