using AuthAPI.DTOs.User;

namespace AuthAPI.Services.UserCredentialsValidation
{
    public interface IUserCredentialsValidator
    {
        Task<ValidationResult> ValidateCredentials(UserDTO request);
    }
}