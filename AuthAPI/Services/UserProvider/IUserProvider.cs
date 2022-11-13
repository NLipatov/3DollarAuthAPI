using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.JWT.Models;

namespace AuthAPI.Services.UserProvider;

public interface IUserProvider
{
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task<List<User>> GetUsersAsync();
    public Task<User> RegisterUser(UserDTO request, List<UserClaim> claims);
    public Task AssignRefreshTokenAsync(string username, IRefreshToken refreshToken);
    public Task SaveRefreshToken(string username, IRefreshToken refreshToken);
}