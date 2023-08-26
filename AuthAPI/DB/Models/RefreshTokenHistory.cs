#nullable disable
namespace AuthAPI.DB.Models;

public record RefreshTokenHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public User User { get; set; }
    public string UserAgent { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}