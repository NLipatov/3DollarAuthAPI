﻿using AuthAPI.Models;
using AuthAPI.Models.Fido2;
using LimpShared.Models.Authentication.Models;
using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using LimpShared.Models.Authentication.Models.UserAuthentication;

namespace AuthAPI.Services.UserProvider;

public interface IUserProvider
{
    public Task<List<User>> GetUsersOnline();
    public Task<bool> IsUserExist(string username);
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task<FidoUser?> GetFidoUserByUsernameAsync(string username);
    public Task<FidoUser> RegisterFidoUser(string name, string? displayName = null);
    public Task<List<FidoCredential>> GetCredentialsByUserAsync(FidoUser user);
    public Task AddCredentialToUser(FidoUser user, FidoCredential credential);
    public Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
    public Task<FidoCredential?> GetCredentialById(byte[] id);
    public Task<List<FidoCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default);
    public Task UpdateCounter(byte[] credentialId, uint counter);
    public Task<List<User>> GetUsersAsync();
    public Task<UserAuthenticationOperationResult> RegisterUser(UserAuthentication request, List<UserClaim>? claims);
    public Task SaveRefreshTokenAsync(string username, RefreshToken refreshToken);
    public Task SetRSAPublic(PublicKeyDTO publicKeyDTO);
    public Task<string?> GetRSAPublic(string username);
}