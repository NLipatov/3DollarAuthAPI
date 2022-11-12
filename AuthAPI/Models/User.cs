using System.Security.Claims;

namespace AuthAPI.Models;

public class User : IUser
{
    public string Username { get; set; }
    public List<Claim> Claims { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; }
    public DateTime TokenExpires { get; set; }
}
