#nullable disable
using EthachatShared.Models.Authentication.Models;

namespace Ethachat.Auth.Domain.Models
{
    public record UserAccessRefreshEventLog : AccessRefreshEventLog
    {
        public User User { get; set; }
    }
}