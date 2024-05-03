using AuthAPI.DB.Models;
using AuthAPI.DB.Models.Fido;
using EthachatShared.Models.Authentication.Enums;
using EthachatShared.Models.Authentication.Models;
using EthachatShared.Models.Authentication.Models.Credentials.CredentialsDTO;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;
using EthachatShared.Models.Authentication.Models.UserAuthentication;
using EthachatShared.Models.Users;

namespace AuthAPI.Services.UserArea.UserProvider;

public interface IUserProvider
{
    public Task<List<User>> GetUsersOnline();
    public Task<IsUserExistDto> IsUserExist(string username);
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    public Task<FidoUser?> GetFidoUserByUsernameAsync(string username);
    public Task<FidoUser> RegisterFidoUser(string name, string? displayName = null);
    public Task<List<FidoCredential>> GetCredentialsByUserAsync(FidoUser user);
    public Task AddCredentialToUser(FidoUser user, FidoCredential credential);
    public Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
    public Task<FidoCredential?> GetCredentialById(byte[] credentialsId);
    public Task<bool> ValidateCredentials(byte[] credentialId, uint counter);
    public Task<List<FidoCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default);
    public Task UpdateCounter(byte[] credentialId, uint counter);
    public Task ResetCounter(byte[] credentialId);
    public Task<string> GetUsernameByCredentialId(byte[] credentialId);
    public Task<List<User>> GetUsersAsync();
    public Task<UserAuthenticationOperationResult> RegisterUser(UserAuthentication request, List<UserClaim>? claims);
    public Task SaveRefreshTokenAsync(string username, RefreshToken dto, JwtIssueReason issueReason = JwtIssueReason.NotActualised);
    public Task SaveRefreshTokenAsync(JwtPair jwtPair, User user);
    public Task<AuthResult> GetUsernameByCredentials(CredentialsDTO credentialsDto);
}