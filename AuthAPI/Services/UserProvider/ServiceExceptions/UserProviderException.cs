namespace AuthAPI.Services.UserProvider.ServiceExceptions
{
    public class UserProviderException : Exception
    {
        public UserProviderException() { }
        public UserProviderException(string message) 
            : base(message) { }
        public UserProviderException(string message, Exception inner) 
            : base(message, inner) { }
    }
}
