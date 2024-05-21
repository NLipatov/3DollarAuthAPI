using AuthAPI.Services.Cryptography;
using AuthAPI.Services.UserArea.UserProvider;
using Ethachat.Auth.Domain.Models;
using EthachatShared.Models.Authentication.Models.UserAuthentication;

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
        public async Task<ValidationResult> ValidateCredentials(UserAuthentication request)
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
