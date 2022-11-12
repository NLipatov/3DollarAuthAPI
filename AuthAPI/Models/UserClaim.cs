#nullable disable

namespace AuthAPI.Models
{
    public class UserClaim
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
