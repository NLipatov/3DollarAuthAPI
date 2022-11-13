using AuthAPI.DTOs.User;

namespace AuthAPI.Services.UserCredentialsValidation
{
    public interface IUserCredentialsValidator
    {
        public Task<ValidationResult> ValidateCredentials(UserDTO request);
    }
}