using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;

namespace AuthAPI.Services.UserArea.UserPublicKeyManager;

/// <summary>
/// Stores a user public key on a server;
/// Gives out a user public key stored on a server.
/// </summary>
public interface IPublicKeyManager
{
    public Task SetRsaPublic(PublicKeyDTO publicKeyDTO);
    public Task<string?> GetRsaPublic(string username);
}