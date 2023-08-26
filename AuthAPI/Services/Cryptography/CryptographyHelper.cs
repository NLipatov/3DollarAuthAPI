using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Services.Cryptography;

public class CryptographyHelper : ICryptographyHelper
{
    public void CreateHashAndSalt(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public bool VerifyHash(string originalData, IEnumerable<byte> storedHash, byte[] salt)
    {
        using (var hmac = new HMACSHA512(salt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(originalData));
            return computedHash.SequenceEqual(storedHash);
        }
    }
}
