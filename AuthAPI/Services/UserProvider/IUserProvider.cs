using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.JWT.Models;

namespace AuthAPI.Services.UserProvider;

public interface IUserProvider
{
    public Task RegisterOnlineUser();
    public Task<List<User>> GetUsersOnline();
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task<List<User>> GetUsersAsync();
    public Task<UserDTO> RegisterUser(UserDTO request, List<UserClaim>? claims);
    public Task SaveRefreshTokenAsync(string username, IRefreshToken refreshToken);
}