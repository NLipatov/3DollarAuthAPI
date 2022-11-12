using AuthAPI.Models;

namespace AuthAPI.Services.Cryptography
{
    public interface ICryptographyHelper
    {
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(User user, string password, byte[] passwordHash, byte[] passwordSalt);
    }
}