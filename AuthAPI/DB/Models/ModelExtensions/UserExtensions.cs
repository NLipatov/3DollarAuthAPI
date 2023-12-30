using EthachatShared.Models.Authentication.Models.AuthenticatedUserRepresentation.Claims;
using EthachatShared.Models.Authentication.Models.UserAuthentication;

namespace AuthAPI.DB.Models.ModelExtensions
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
