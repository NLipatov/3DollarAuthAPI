using AuthAPI.Models;

namespace AuthAPI.Services.Cryptography
{
    public interface ICryptographyHelper
    {
        void CreateHashAndSalt(string originalData, out byte[] hashedData, out byte[] Salt);
        bool VerifyHash(string originalData, byte[] storedHash, byte[] salt);
    }
}