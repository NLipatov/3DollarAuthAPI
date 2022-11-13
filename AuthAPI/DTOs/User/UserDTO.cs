#nullable disable
using AuthAPI;

namespace AuthAPI.DTOs.User;

public class UserDTO
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Phonename { get; set; }
}
