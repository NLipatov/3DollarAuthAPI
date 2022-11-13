using AuthAPI.DB.DBContext;
using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.JWT.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Text.Json;

namespace AuthAPI.Services.UserProvider
{
    public class UserProvider : IUserProvider
    {
        private readonly AuthContext _authContext;
        private readonly ICryptographyHelper _cryptographyHelper;
        public UserProvider(AuthContext authContext, ICryptographyHelper cryptographyHelper)
        {
            _authContext = authContext;
            _cryptographyHelper = cryptographyHelper;
        }
        public async Task<User> RegisterUser(UserDTO request, List<UserClaim> claims)
        {
            User? existingUser = (await GetUsersAsync()).FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null) return existingUser;

            _cryptographyHelper.CreateHashAndSalt(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User newUser = new()
            {
                Username = request.Username,
                Claims = claims,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            await _authContext.AddAsync(newUser);
            await _authContext.SaveChangesAsync();

            newUser.PasswordHash = Array.Empty<byte>();
            newUser.PasswordSalt = Array.Empty<byte>();
            return newUser;
        }

        public async Task AssignRefreshTokenAsync(string username, IRefreshToken refreshToken)
        {
            User? storedUser = await GetUserByUsernameAsync(username);
            if (storedUser == null)
            {
                throw new NullReferenceException("User was not found");
            }
            else
            {
                storedUser.RefreshToken = refreshToken.Token;
                storedUser.RefreshTokenCreated = refreshToken.Created;
                storedUser.RefreshTokenExpires = refreshToken.Expires;

                await _authContext.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _authContext.Users.Include(x=>x.Claims).FirstAsync(x => x.Username == username);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _authContext.Users.AsNoTracking().Include(x=>x.Claims).ToListAsync();
        }

        public async Task SaveEncryptedRefreshToken(string username, IRefreshToken rToken)
        {
            User user = await _authContext.Users.FirstAsync(x=>x.Username == username);

            user.RefreshToken = rToken.Token;

            await _authContext.SaveChangesAsync();
        }

        public async Task SaveRefreshToken(string username, IRefreshToken refreshToken)
        {
            User user = _authContext.Users.First(x=>x.Username == username);

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpires = refreshToken.Expires;
            user.RefreshTokenCreated = refreshToken.Created;

            await _authContext.SaveChangesAsync();
        }
    }
}
