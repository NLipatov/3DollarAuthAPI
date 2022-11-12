using System.Security.Claims;

namespace AuthAPI.Models
{
    public interface IUser
    {
        List<Claim> Claims { get; set; }
        byte[] PasswordHash { get; set; }
        byte[] PasswordSalt { get; set; }
        string Username { get; set; }
        string RefreshToken { get; set; }
        DateTime TokenCreated { get; set; }
        DateTime TokenExpires { get; set; }
    }
}