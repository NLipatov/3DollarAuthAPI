using AuthAPI.DB.Models;
using AuthAPI.Services.Cryptography;
using LimpShared.Models.Authentication.Models.UserAuthentication;

namespace AuthAPI.Services.ModelBuilder
{
    public static class ModelFactory
    {
        public static User BuildUser
            (ICryptographyHelper cryptoHelper, 
            UserAuthentication dto,
            List<UserClaim>? claims)
        {
            cryptoHelper.CreateHashAndSalt(dto.Password!, out byte[] passwordHash, out byte[] passwordSalt);
            return new User
            {
                Username = dto.Username,
                Claims = claims ?? null,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };
        }
    }
}
