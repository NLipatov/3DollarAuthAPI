using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.UserProvider;

namespace AuthAPI.Services.UserCredentialsValidation
{
    public class UserCredentialsValidator : IUserCredentialsValidator
    {
        private readonly IUserProvider _userProvider;
        private readonly ICryptographyHelper _cryptographyHelper;

        public UserCredentialsValidator
            (
                IUserProvider userProvider,
                ICryptographyHelper cryptographyHelper
            )
        {
            _userProvider = userProvider;
            _cryptographyHelper = cryptographyHelper;
        }
        public async Task<ValidationResult> ValidateCredentials(UserDTO request)
        {
            User? user = await _userProvider.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                return ValidationResult.WrongUsername;
            }

            if (!_cryptographyHelper.VerifyHash(request.Password!, user.PasswordHash, user.PasswordSalt))
            {
                return ValidationResult.WrongPassword;
            }

            return ValidationResult.Success;
        }
    }
}
