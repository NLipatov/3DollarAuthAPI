using AuthAPI.DTOs.Claims;

namespace AuthAPI.DTOs.User;

public class UserDTO
{
    public string Username { get; set; } = "N/A";
    public string? Password { get; set; }
    public List<UserClaimsDTO>? Claims { get; set; }
}