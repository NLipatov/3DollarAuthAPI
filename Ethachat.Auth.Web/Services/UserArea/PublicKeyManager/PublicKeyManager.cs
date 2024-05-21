using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.Fido;
using Ethachat.Auth.Infrastructure.DB.DBContext;
using EthachatShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using EthachatShared.Models.Authentication.Types;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services.UserArea.PublicKeyManager;

public class PublicKeyManager : IPublicKeyManager
{
    private readonly IConfiguration _configuration;

    public PublicKeyManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SetRsaPublic(PublicKeyDto publicKeyDto)
    {
        using (AuthContext context = new(_configuration))
        {
            if (publicKeyDto.AuthenticationType is AuthenticationType.WebAuthn)
            {
                FidoUser? targetUser =
                    await context.FidoUsers.Include(x=>x.Claims).FirstOrDefaultAsync(x => x.Name == publicKeyDto.Username);
                if (targetUser == null)
                    throw new ArgumentException($"There is no user with specified username: '{publicKeyDto.Username}'.");
                
                UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");
                if (publicKeyClaim == null)
                {
                    targetUser.Claims?.Add(new UserClaim
                    {
                        Name = "PublicKey",
                        Type = "RSA Public Key",
                        Value = publicKeyDto.Key,
                    });
                }
                else
                {
                    publicKeyClaim.Value = publicKeyDto.Key;
                }

                await context.SaveChangesAsync();
            }
            if (publicKeyDto.AuthenticationType is AuthenticationType.JwtToken)
            {
                User? targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == publicKeyDto.Username);
                if (targetUser == null)
                    throw new ArgumentException($"There is no user with specified username: '{publicKeyDto.Username}'.");

                UserClaim? publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");
                if (publicKeyClaim == null)
                {
                    targetUser.Claims?.Add(new UserClaim
                    {
                        Name = "PublicKey",
                        Type = "RSA Public Key",
                        Value = publicKeyDto.Key,
                    });
                }
                else
                {
                    publicKeyClaim.Value = publicKeyDto.Key;
                }

                await context.SaveChangesAsync();
            }
        }
    }

    public async Task<string?> GetRsaPublic(string username)
    {
        #warning ToDo: split logic to 2 handlers - one for jwt users and another one for webAuthn.
        await using (AuthContext context = new(_configuration))
        {
            UserClaim publicKeyClaim;
            var targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == username);
            if (targetUser == null)
            {
                var fidoUser = await context.FidoUsers.Include(x => x.Claims)
                    .FirstOrDefaultAsync(x => x.Name == username);
                if (fidoUser is not null)
                {
                    publicKeyClaim = fidoUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");
                    return publicKeyClaim?.Value;
                }
            }

            publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");

            return publicKeyClaim?.Value;
        }
    }
}