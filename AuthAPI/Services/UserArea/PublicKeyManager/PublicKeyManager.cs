using AuthAPI.DB.DBContext;
using AuthAPI.DB.Models;
using LimpShared.Models.Authentication.Models.AuthenticatedUserRepresentation.PublicKey;
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
            User? targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == publicKeyDto.Username);
            if (targetUser == null)
                throw new ArgumentException($"There is no user with specified username: '{publicKeyDto.Username}'");

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

    public async Task<string?> GetRsaPublic(string username)
    {
        await using (AuthContext context = new(_configuration))
        {
            var targetUser = await context.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Username == username);
            if (targetUser == null)
                throw new ArgumentException($"There is no user with specified username: '{username}'");

            var publicKeyClaim = targetUser.Claims?.FirstOrDefault(x => x.Type == "RSA Public Key");

            return publicKeyClaim?.Value;
        }
    }
}