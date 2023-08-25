using AuthAPI.DB.DBContext;
using AuthAPI.Models;
using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services.UserArea.UserPublicKeyManager;

public class PublicKeyManager : IPublicKeyManager
{
    private readonly IConfiguration _configuration;

    public PublicKeyManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SetRsaPublic(PublicKeyDTO publicKeyDTO)
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

    public async Task<string?> GetRsaPublic(string username)
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
}