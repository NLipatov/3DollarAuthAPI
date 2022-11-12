using AuthAPI.DTOs;

namespace AuthAPI.Services.UserCredentialsValidation
{
    public interface IUserCredentialsValidator
    {
        Task<ValidationResult> ValidateCredentials(UserDTO request);
    }
}