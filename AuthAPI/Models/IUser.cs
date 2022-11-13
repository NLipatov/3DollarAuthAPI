using System.Security.Claims;

namespace AuthAPI.Models
{
    public interface IUser
    {
        List<UserClaim> Claims { get; set; }
        byte[] PasswordHash { get; set; }
        byte[] PasswordSalt { get; set; }
        string Username { get; set; }
        public string RefreshToken { get; set; }
        DateTime RefreshTokenCreated { get; set; }
        DateTime RefreshTokenExpires { get; set; }
    }
}