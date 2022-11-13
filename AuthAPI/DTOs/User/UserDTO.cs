using AuthAPI;
using AuthAPI.DTOs.Claims;
using AuthAPI.Models;

namespace AuthAPI.DTOs.User;

public class UserDTO
{
    public string Username { get; set; }
    public string? Password { get; set; }
    public List<UserClaimsDTO>? Claims { get; set; }
}
