using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models;

public class User : IUser
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; } = "N/A";
    public List<UserClaim>? Claims { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenCreated { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}
