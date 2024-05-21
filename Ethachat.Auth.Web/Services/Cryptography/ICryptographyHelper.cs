namespace AuthAPI.Services.Cryptography;

public interface ICryptographyHelper
{
    void CreateHashAndSalt(string originalData, out byte[] hashedData, out byte[] salt);
    bool VerifyHash(string originalData, IEnumerable<byte> storedHash, byte[] salt);
}