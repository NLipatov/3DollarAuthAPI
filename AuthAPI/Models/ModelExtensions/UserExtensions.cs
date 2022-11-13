namespace AuthAPI.Models.ModelExtensions
{
    public static class UserExtensions
    {
        public static User ErasePassword(this User user)
        {
            user.PasswordHash = null;
            user.PasswordSalt = null;
            return user;
        }
    }
}
