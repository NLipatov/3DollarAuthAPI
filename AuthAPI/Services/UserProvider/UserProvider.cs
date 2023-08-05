using AuthAPI.DB.DBContext;
using AuthAPI.Mapping;
using AuthAPI.Models;
using AuthAPI.Models.Fido2;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.ModelBuilder;
using AuthAPI.Services.UserProvider.ServiceExceptions;
using LimpShared.Models.Authentication.Models;
using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using LimpShared.Models.Authentication.Models.UserAuthentication;
using LimpShared.Models.AuthenticationModels.ResultTypeEnum;
using Microsoft.EntityFrameworkCore;
using NSec.Cryptography;
using System.Text;

namespace AuthAPI.Services.UserProvider
{
    public class UserProvider : IUserProvider
    {
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IConfiguration _configuration;

        public UserProvider(ICryptographyHelper cryptographyHelper, IConfiguration configuration)
        {
            _cryptographyHelper = cryptographyHelper;
            _configuration = configuration;
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
        public async Task<UserAuthenticationOperationResult> RegisterUser(UserAuthentication request, List<UserClaim>? claims)
        {
            #region Checking if user with this username already exist.
            User? existingUser = (await GetUsersAsync()).FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                return new UserAuthenticationOperationResult
                {
                    SystemMessage = "User with this username already exists",
                    UserDTO = null,
                    ResultType = OperationResultType.Fail
                };
            #endregion

            User user = ModelFactory.BuildUser(_cryptographyHelper, request, claims);

            await SaveUser(user);

            return new UserAuthenticationOperationResult()
            {
                SystemMessage = "Success",
                UserDTO = user.ToDTO(),
            };
        }

        public async Task SaveRefreshTokenAsync(string username, RefreshToken refreshToken)
        {
            using(AuthContext context = new(_configuration))
            {
                User user = context.Users.First(x => x.Username == username);

                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpires = refreshToken.Expires;
                user.RefreshTokenCreated = refreshToken.Created;

                await context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.Users
                    .Include(x => x.Claims)
                    .FirstOrDefaultAsync(x => x.Username == username);
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.Users.Include(x => x.Claims).ToListAsync();
            }
        }

        public async Task SaveEncryptedRefreshToken(string username, RefreshToken rToken)
        {
            using (AuthContext context = new(_configuration))
            {
                User user = await context.Users.FirstAsync(x => x.Username == username);

                user.RefreshToken = rToken.Token;

                await context.SaveChangesAsync();

            }
        }

        private async Task SaveUser(User user)
        {
            using (AuthContext context = new(_configuration))
            {
                await context.AddAsync(user);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetUsersOnline()
        {
            using (AuthContext context = new(_configuration))
            {
                var usersOnline = await context.Users.ToListAsync();
                return usersOnline;
            }
        }

        public async Task<List<FidoCredential>> GetCredentialsByUserAsync(FidoUser user)
        {
            using (AuthContext context = new(_configuration))
            {
                return (await context.StoredCredentials.ToListAsync())
                    .Where(c => c.UserId.AsSpan().SequenceEqual(user.UserId))
                    .ToList();
            }
        }

        public async Task<List<FidoUser>> GetUsersByCredentialIdAsync
        (byte[] credentialId, 
        CancellationToken cancellationToken = default)
        {
            using (AuthContext context = new(_configuration))
            {
                var cred = await context.StoredCredentials
                    .FirstOrDefaultAsync(c => c.Descriptor.Id.SequenceEqual(credentialId));

                if (cred is null)
                    return new List<FidoUser>();

                return await context.FidoUsers
                    .Where(u => u.UserId.SequenceEqual(cred.UserId)).ToListAsync();
            }
        }

        public async Task AddCredentialToUser(FidoUser user, FidoCredential credential)
        {
            using (AuthContext context = new(_configuration))
            {
                credential.UserId = user.UserId;
                await context.StoredCredentials.AddAsync(credential);
                await context.SaveChangesAsync();
            }
        }

        public async Task<FidoCredential?> GetCredentialById(byte[] id)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.StoredCredentials
                    .FirstOrDefaultAsync(c => c.Descriptor.Id.SequenceEqual(id));
            }
        }

        public async Task<List<FidoCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.StoredCredentials
                    .Where(c => c.UserHandle.SequenceEqual(userHandle)).ToListAsync();
            }
        }

        public async Task UpdateCounter(byte[] credentialId, uint counter)
        {
            using (AuthContext context = new(_configuration))
            {
                var cred = await context.StoredCredentials
                    .FirstAsync(c => c.Descriptor.Id.SequenceEqual(credentialId));
                cred.SignatureCounter = counter;
            }
        }

        public async Task<FidoUser?> GetFidoUserByUsernameAsync(string username)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.FidoUsers.FirstOrDefaultAsync(u => u.Name == username);
            }
        }

        public async Task<FidoUser> RegisterFidoUser(string name, string? displayName = null)
        {
            FidoUser newFidoUser = new FidoUser
            {
                Name = name,
                DisplayName = displayName ?? string.Empty,
                UserId = Encoding.UTF8.GetBytes(name)
            };

            using (AuthContext context = new(_configuration))
            {
                await context.FidoUsers.AddAsync(newFidoUser);
                await context.SaveChangesAsync();
            }

            return newFidoUser;
        }

        public async Task SetRSAPublic(PublicKeyDTO publicKeyDTO)
        {
            using (AuthContext context = new(_configuration))
            {
                User? targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == publicKeyDTO.Username);
                if (targetUser == null)
                    throw new ArgumentException($"There is no user with specified username: '{publicKeyDTO.Username}'");

                UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");
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

                await context.SaveChangesAsync();
            }
        }

        public async Task<string?> GetRSAPublic(string username)
        {
            using (AuthContext context = new(_configuration))
            {
                User? targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == username);
                if (targetUser == null)
                    throw new ArgumentException($"There is no user with specified username: '{username}'");

                UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");

                return publicKeyClaim?.Value;
            }
        }

        public async Task<bool> IsUserExist(string username)
        {
            using(AuthContext context = new(_configuration))
            {
                bool exist = await context.Users
                    .Select(x => x.Username)
                    .Where(x => x.ToLower() == username.ToLower())
                    .AnyAsync();

                return exist;
            }
        }
    }
}
