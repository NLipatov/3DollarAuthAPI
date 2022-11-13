using AuthAPI.Models;

namespace AuthAPI.Services.JWT.Models
{
    /// <summary>
    /// Нужен для получения нового JWT без перелогина
    /// </summary>
    public class RefreshToken : IRefreshToken
    {
        public User User { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }
    }
}
