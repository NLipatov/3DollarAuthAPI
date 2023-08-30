#nullable disable
using LimpShared.Models.Authentication.Models;

namespace AuthAPI.DB.Models
{
    public record UserAccessRefreshEventLog : AccessRefreshEventLog
    {
        public User User { get; set; }
    }
}