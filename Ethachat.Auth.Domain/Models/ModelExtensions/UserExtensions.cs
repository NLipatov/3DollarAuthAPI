using EthachatShared.Models.Authentication.Models.UserAuthentication;

namespace Ethachat.Auth.Domain.Models.ModelExtensions
{
    public static class UserExtensions
    {
        public static User ErasePassword(this User user)
        {
            user.PasswordHash = Array.Empty<byte>();
            user.PasswordSalt = Array.Empty<byte>();
            return user;
        }

        public static UserAuthentication ToDto(this User user)
        {
            return new UserAuthentication
            {
                Username = user.Username
            };
        }
    }
}
