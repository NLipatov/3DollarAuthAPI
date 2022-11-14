using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;

namespace AuthAPI.Services.ModelBuilder
{
    public static class ModelFactory
    {
        public static User BuildUser
            (ICryptographyHelper _cryptoHelper, 
            UserDTO dto,
            List<UserClaim>? claims)
        {
            _cryptoHelper.CreateHashAndSalt(dto.Password!, out byte[] passwordHash, out byte[] passwordSalt);
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
