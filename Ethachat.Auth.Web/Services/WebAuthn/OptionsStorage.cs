using System.Collections.Concurrent;
using Fido2NetLib;

namespace AuthAPI.Services.WebAuthn;

public static class OptionsStorage
{
    
    public static ConcurrentDictionary<string, AssertionOptions> usernameToOptions = new();
}