#nullable disable
using AuthAPI.DB.Enums;

namespace AuthAPI.DB.Models;

public record RefreshTokenEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public User User { get; set; }
    public string UserAgent { get; set; }
    public Guid UserAgentId { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public JwtIssueReason IssueReason { get; set; }
}