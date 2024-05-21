namespace AuthAPI.Services.AuthenticationManager;

public interface IAuthenticationManager<T>
{
    Task<bool> ValidateCredentials(T credentials);
    Task<T?> RefreshCredentials(T credentials);
}