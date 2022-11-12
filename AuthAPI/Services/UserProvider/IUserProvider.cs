using AuthAPI.DTOs;
using AuthAPI.Models;
using AuthAPI.Services.JWT.Models;
using System.Security.Claims;

namespace AuthAPI.Services.UserProvider;

public interface IUserProvider
{
    public Task<User?> GetUserByUsername(string username);
    public Task<List<User>> GetUsers();
    public Task<User> AddNewUser(UserDTO request, List<Claim> claims);
    public Task AssignRefreshToken(string username, IRefreshToken refreshToken);
}