using AuthAPI.DB.DBContext;
using AuthAPI.DTOs.User;
using AuthAPI.Mapping;
using AuthAPI.Models;
using AuthAPI.Models.Fido2;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.ModelBuilder;
using AuthAPI.Services.UserProvider.ServiceExceptions;
using LimpShared.Authentification;
using LimpShared.DTOs.PublicKey;
using LimpShared.DTOs.User;
using Microsoft.EntityFrameworkCore;
using NSec.Cryptography;
using System.Text;

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
        public async Task<UserOperationResult> RegisterUser(UserDTO request, List<UserClaim>? claims)
        {
            #region Checking if user with this username already exist.
            User? existingUser = (await GetUsersAsync()).FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                return new UserOperationResult
                {
                    SystemMessage = "User with this username already exists",
                    UserDTO = null,
                    ResultType = LimpShared.ResultTypeEnum.OperationResultType.Fail
                };
            #endregion

            User user = ModelFactory.BuildUser(_cryptographyHelper, request, claims);

            await SaveUser(user);

            return new UserOperationResult()
            {
                SystemMessage = "Success",
                UserDTO = user.ToDTO(),
            };
        }

        public async Task SaveRefreshTokenAsync(string username, RefreshToken refreshToken)
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

        public async Task SaveEncryptedRefreshToken(string username, RefreshToken rToken)
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

        public async Task<List<User>> GetUsersOnline()
        {
            var usersOnline = await _authContext.Users.ToListAsync();
            return usersOnline;
        }

        public async Task<List<FidoCredential>> GetCredentialsByUserAsync(FidoUser user)
        {
            return (await _authContext.StoredCredentials.ToListAsync()).Where(c => c.UserId.AsSpan().SequenceEqual(user.UserId)).ToList();
        }

        public async Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
        {
            var cred = await _authContext.StoredCredentials.FirstOrDefaultAsync(c => c.Descriptor.Id.SequenceEqual(credentialId));

            if (cred is null)
                return new List<FidoUser>();

            return await _authContext.FidoUsers.Where(u => u.UserId.SequenceEqual(cred.UserId)).ToListAsync();
        }

        public async Task AddCredentialToUser(FidoUser user, FidoCredential credential)
        {
            credential.UserId = user.UserId;
            await _authContext.StoredCredentials.AddAsync(credential);
            await _authContext.SaveChangesAsync();
        }

        public async Task<FidoCredential?> GetCredentialById(byte[] id)
        {
            return await _authContext.StoredCredentials.FirstOrDefaultAsync(c => c.Descriptor.Id.SequenceEqual(id));
        }

        public async Task<List<FidoCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default)
        {
            return await _authContext.StoredCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToListAsync();
        }

        public async Task UpdateCounter(byte[] credentialId, uint counter)
        {
            var cred = await _authContext.StoredCredentials.FirstAsync(c => c.Descriptor.Id.SequenceEqual(credentialId));
            cred.SignatureCounter = counter;
        }

        public async Task<FidoUser?> GetFidoUserByUsernameAsync(string username)
        {
            return await _authContext.FidoUsers.FirstOrDefaultAsync(u => u.Name == username);
        }

        public async Task<FidoUser> RegisterFidoUser(string name, string? displayName = null)
        {
            FidoUser newFidoUser = new FidoUser
            {
                Name = name,
                DisplayName = displayName ?? string.Empty,
                UserId = Encoding.UTF8.GetBytes(name)
            };

            await _authContext.FidoUsers.AddAsync(newFidoUser);
            await _authContext.SaveChangesAsync();

            return newFidoUser;
        }

        public async Task SetRSAPublic(PublicKeyDTO publicKeyDTO)
        {
            User? targetUser = await _authContext.Users.Include(x=>x.Claims).FirstOrDefaultAsync(x => x.Username == publicKeyDTO.Username);
            if (targetUser == null)
                throw new ArgumentException($"There is no user with specified username: '{publicKeyDTO.Username}'");

            UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x=> x.Type == "RSA Public Key");
            if (publicKeyClaim == null)
            {
                targetUser.Claims?.Add(new UserClaim
                {
                    Name = "PublicKey",
                    Type = "RSA Public Key",
                    Value = publicKeyDTO.Key,
                });
            }
            else
            {
                publicKeyClaim.Value = publicKeyDTO.Key;
            }

            await _authContext.SaveChangesAsync();
        }

        public async Task<string?> GetRSAPublic(string username)
        {
            User? targetUser = await _authContext.Users.Include(x=>x.Claims).FirstOrDefaultAsync(x=>x.Username == username);
            if(targetUser == null)
                throw new ArgumentException($"There is no user with specified username: '{username}'");

            UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");

            return publicKeyClaim?.Value;
        }
    }
}
