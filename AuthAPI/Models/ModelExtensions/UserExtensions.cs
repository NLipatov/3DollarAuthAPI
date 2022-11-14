namespace AuthAPI.Models.ModelExtensions
{
    public static class UserExtensions
    {
        public static User ErasePassword(this User user)
        {
            user.PasswordHash = Array.Empty<byte>();
            user.PasswordSalt = Array.Empty<byte>();
            return user;
        }
    }
}
