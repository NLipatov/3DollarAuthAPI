using AuthAPI.Services.Cryptography;
using AuthAPI.Services.JWT;
using AuthAPI.Services.UserCredentialsValidation;
using AuthAPI.Services.UserProvider;

namespace AuthAPI.Extensions
{
    public static class AuthAPIExtensions
    {
        public static void UseCryptographyHelper(this IServiceCollection services)
        {
            services.AddTransient<ICryptographyHelper, CryptographyHelper>();
        }

        public static void UseFakeUserProvider(this IServiceCollection services)
        {
            services.AddSingleton<IUserProvider, FakeUserProvider>();
        }

        public static void UseJWTGenerator(this IServiceCollection services)
        {
            services.AddTransient<IJwtService, JwtService>();
        }

        public static void UseUserCredentialsValidator(this IServiceCollection services)
        {
            services.AddTransient<IUserCredentialsValidator, UserCredentialsValidator>();
        }
    }
}
