#nullable disable
namespace AuthAPI.Services.JWT.Models
{
    public class JWTPair
    {
        public string AccessToken { get; set; }
        public IRefreshToken RefreshToken { get; set; }
    }
}
