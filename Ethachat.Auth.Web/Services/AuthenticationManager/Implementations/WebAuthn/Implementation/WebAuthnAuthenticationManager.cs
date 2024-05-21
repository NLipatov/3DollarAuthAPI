using AuthAPI.Services.UserArea.UserProvider;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;

namespace AuthAPI.Services.AuthenticationManager.Implementations.WebAuthn.Implementation;

public class WebAuthnAuthenticationManager : IAuthenticationManager<WebAuthnPair>
{
    private readonly IUserProvider _userProvider;

    public WebAuthnAuthenticationManager(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }
    public async Task<bool> ValidateCredentials(WebAuthnPair credentials)
    {
        var credentialIdBytes = Convert.FromBase64String(Uri.UnescapeDataString(credentials.CredentialId));
        var isTokenValid = await _userProvider.ValidateCredentials(credentialIdBytes, credentials.Counter);
        return isTokenValid;
    }

    public async Task<WebAuthnPair?> RefreshCredentials(WebAuthnPair credentials)
    {
        try
        {
            var credentialIdBytes = Convert.FromBase64String(credentials.CredentialId);
            await _userProvider
                .UpdateCounter(credentialIdBytes, credentials.Counter);

            return credentials;
        }
        catch (Exception e)
        {
            throw new ArgumentException($"Exception:{nameof(WebAuthnAuthenticationManager)}.{nameof(RefreshCredentials)}:" +
                                        $"Could not update a {nameof(WebAuthnPair)}: {e.Message}.");
        }
    }
}