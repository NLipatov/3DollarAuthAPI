using System.Text;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.ModelBuilder;
using AuthAPI.Services.UserArea.UserProvider.ServiceExceptions;
using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.Fido;
using Ethachat.Auth.Domain.Models.ModelExtensions;
using Ethachat.Auth.Infrastructure.DB.DBContext;
using EthachatShared.Models.Authentication.Enums;
using EthachatShared.Models.Authentication.Models;
using EthachatShared.Models.Authentication.Models.Credentials.CredentialsDTO;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;
using EthachatShared.Models.Authentication.Models.UserAuthentication;
using EthachatShared.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services.UserArea.UserProvider
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
        public async Task<UserAuthenticationOperationResult> RegisterUser(UserAuthentication request,
            List<UserClaim>? claims)
        {
            #region Checking if user with this username already exist.

            User? existingUser = (await GetUsersAsync()).FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                return new UserAuthenticationOperationResult
                {
                    SystemMessage = "User with this username already exists",
                    UserDto = null,
                    ResultType = OperationResultType.Fail
                };

            #endregion

            User user = ModelFactory.BuildUser(_cryptographyHelper, request, claims);

            await SaveUser(user);

            return new UserAuthenticationOperationResult()
            {
                SystemMessage = "Success",
                UserDto = user.ToDto(),
            };
        }
        
        public async Task SaveRefreshTokenAsync(JwtPair jwtPair, User user)
        {
            using (var context = new AuthContext(_configuration))
            {
                var targetUser = context.Users.First(x => x.Id == user.Id);

                targetUser.RefreshToken = jwtPair.RefreshToken.Token;
                targetUser.RefreshTokenCreated = jwtPair.RefreshToken.Created;
                targetUser.RefreshTokenExpires = jwtPair.RefreshToken.Expires;

                await context.SaveChangesAsync();
            }
        }

        public async Task<AuthResult> GetUsernameByCredentials(CredentialsDTO credentialsDto)
        {
            using (AuthContext context = new(_configuration))
            {
                string username = string.Empty;
                if (credentialsDto.JwtPair is not null)
                {
                    var request = await context.Users
                        .Where(x => x.RefreshToken == credentialsDto.JwtPair.RefreshToken.Token)
                        .Select(x => new
                        {
                            Username = x.Username
                        })
                        .FirstOrDefaultAsync();

                    username = request?.Username ?? string.Empty;
                }

                if (credentialsDto.WebAuthnPair is not null)
                {
                    var credentialIdBytes = Convert.FromBase64String(credentialsDto.WebAuthnPair.CredentialId);
                    var request = await context.StoredCredentials
                        .Where(c => c.DescriptorId.SequenceEqual(credentialIdBytes))
                        .Select(x => new
                        {
                            Username = x.UserId,
                            
                        })
                        .FirstOrDefaultAsync();
                    
                    username = Encoding.UTF8.GetString(request?.Username ?? Array.Empty<byte>());
                }

                return new AuthResult
                {
                    Result = string.IsNullOrWhiteSpace(username) ? AuthResultType.Fail : AuthResultType.Success,
                    Message = username
                };
            }
        }

        public async Task SaveRefreshTokenAsync
        (string username,
            RefreshToken dto,
            JwtIssueReason jwtIssueReason = JwtIssueReason.NotActualised)
        {
            using (AuthContext context = new(_configuration))
            {
                var user = context.Users.First(x => x.Username == username);

                if (dto is null)
                    throw new ArgumentException
                        ($"Given {nameof(dto)} was null. Token pipeline is broken.");

                if (dto.Token == user.RefreshToken && jwtIssueReason == JwtIssueReason.RefreshToken)
                    throw new ArgumentException($"Cannot update refresh token: " +
                                                $"refresh token is the same that's stored in the database.");

                user.RefreshToken = dto.Token;
                user.RefreshTokenExpires = dto.Expires;
                user.RefreshTokenCreated = dto.Created;

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

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.Users
                    .Include(x=>x.Claims)
                    .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
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
                    .FirstOrDefaultAsync(c => c.DescriptorId.SequenceEqual(credentialId));

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

        public async Task<FidoCredential?> GetCredentialById(byte[] credentialsId)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.StoredCredentials
                    .FirstOrDefaultAsync(c => c.DescriptorId.SequenceEqual(credentialsId));
            }
        }

        // Checks that db has a record for given credentialId and counter value.
        public async Task<bool> ValidateCredentials(byte[] credentialId, uint counter)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.StoredCredentials
                    .FirstOrDefaultAsync(c =>
                        c.DescriptorId.SequenceEqual(credentialId) && c.SignatureCounter == counter) is not null;
            }
        }

        public async Task<List<FidoCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle,
            CancellationToken cancellationToken = default)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.StoredCredentials
                    .Where(c => c.UserHandle.SequenceEqual(userHandle)).ToListAsync();
            }
        }

        // Counter is a part of credential validation system.
        // Each credential validation increases counter by one.
        // Relogin should reset the counter.
        public async Task ResetCounter(byte[] credentialId)
        {
            using (AuthContext context = new(_configuration))
            {
                var cred = await context.StoredCredentials
                    .FirstAsync(c => c.DescriptorId.SequenceEqual(credentialId));

                cred.SignatureCounter = 0;
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateCounter(byte[] credentialId, uint counter)
        {
            using (AuthContext context = new(_configuration))
            {
                var cred = await context.StoredCredentials
                    .FirstAsync(c => c.DescriptorId.SequenceEqual(credentialId) && c.SignatureCounter == counter);

                cred.SignatureCounter = counter + 1;
                await context.SaveChangesAsync();
            }
        }

        public async Task<string> GetUsernameByCredentialId(byte[] credentialId)
        {
            using (AuthContext context = new(_configuration))
            {
                var cred = await context.StoredCredentials
                    .FirstAsync(c => c.DescriptorId.SequenceEqual(credentialId));

                return Encoding.UTF8.GetString(cred.UserId);
            }
        }

        public async Task<FidoUser?> GetFidoUserByUsernameAsync(string username)
        {
            using (AuthContext context = new(_configuration))
            {
                return await context.FidoUsers.FirstOrDefaultAsync(x => x.Name == username);
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

        public async Task<IsUserExistDto> IsUserExist(string username)
        {
            using (AuthContext context = new(_configuration))
            {
                User? user = await context.Users.FirstOrDefaultAsync(x => x.Username == username);

                if (user is not null)
                    return user.AsUserExistDTO();
                
                FidoUser? fidoUser = await context.FidoUsers.FirstOrDefaultAsync(x => x.Name == username);
                
                if (fidoUser is not null)
                    return fidoUser.AsUserExistDTO();

                return new()
                {
                    IsExist = false,
                    Username = username
                };
            }
        }
    }
}