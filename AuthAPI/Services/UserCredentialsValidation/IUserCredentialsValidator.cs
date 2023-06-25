using LimpShared.Models.Authentication.Models.UserAuthentication;

namespace AuthAPI.Services.UserCredentialsValidation
{
    public interface IUserCredentialsValidator
    {
        public Task<ValidationResult> ValidateCredentials(UserAuthentication request);
    }
}