using AuthAPI.DB.DBContext;
using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.JWT.Models;
using AuthAPI.Services.ModelBuilder;
using AuthAPI.Services.UserProvider.ServiceExceptions;
using Microsoft.EntityFrameworkCore;

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
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">UserDTO with information, 
        /// to be put in new User record in DB.
        /// username must not be yet presented in DB</param>
        /// <param name="claims"></param>
        /// <returns></returns>
        /// <exception cref="UserProviderException"></exception>
        public async Task<UserDTO> RegisterUser(UserDTO request, List<UserClaim>? claims)
        {
            #region Checking if user with this username already exist.
            User? existingUser = (await GetUsersAsync()).FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                throw new UserProviderException("User with this username already exists");
            #endregion

            User user = ModelFactory.BuildUser(_cryptographyHelper, request, claims);

            await SaveUser(user);

            return user.ToDTO();
        }

        public async Task SaveRefreshTokenAsync(string username, IRefreshToken refreshToken)
        {
            User user = _authContext.Users.First(x => x.Username == username);

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpires = refreshToken.Expires;
            user.RefreshTokenCreated = refreshToken.Created;

            await _authContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _authContext.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _authContext.Users.Include(x => x.Claims).ToListAsync();
        }

        public async Task SaveEncryptedRefreshToken(string username, IRefreshToken rToken)
        {
            User user = await _authContext.Users.FirstAsync(x => x.Username == username);

            user.RefreshToken = rToken.Token;

            await _authContext.SaveChangesAsync();
        }

        private async Task SaveUser(User user)
        {
            await _authContext.AddAsync(user);
            await _authContext.SaveChangesAsync();
        }
    }
}
