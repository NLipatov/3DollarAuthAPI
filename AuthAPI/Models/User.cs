using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models;

public class User : IUser
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; }
    public List<UserClaim> Claims { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenCreated { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}
