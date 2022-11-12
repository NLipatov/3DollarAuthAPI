using AuthAPI.DTOs;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.JWT.Models;
using System.Security.Claims;

namespace AuthAPI.Services.UserProvider;

public class FakeUserProvider : IUserProvider
{
    private readonly ICryptographyHelper _cryptoHelper;
    private List<User>? _users;

    public FakeUserProvider(ICryptographyHelper cryptoHelper)
    {
        _cryptoHelper = cryptoHelper;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return (await GetUsers()).FirstOrDefault(x => x.Username == username);
    }

    public Task<List<User>> GetUsers()
    {
        if(_users != null)
        {
            return Task.FromResult(_users);
        }
        else
        {
            List<User> users = new()
        {
            new User()
            {
                Username = "tstark",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "tstark"),
                    new Claim(ClaimTypes.Email, "stark@bizapps.com"),
                    new Claim(ClaimTypes.GivenName, "Tony"),
                    new Claim(ClaimTypes.Surname, "Stark"),
                    new Claim(ClaimTypes.Role, "Admin")
                },
                PasswordHash = null,
                PasswordSalt = null,
            },
            new User()
            {
                Username = "jdark",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "jdark"),
                    new Claim(ClaimTypes.Email, "dark@bizapps.com"),
                    new Claim(ClaimTypes.GivenName, "Jane"),
                    new Claim(ClaimTypes.Surname, "Dark"),
                    new Claim(ClaimTypes.Role, "User")
                },
                PasswordHash = null,
                PasswordSalt = null,
            }
        };

            foreach (var user in users)
            {
                FakeCreds creds = GenerateFakeCreds(user.Username);
                user.PasswordHash = creds.PasswordHash;
                user.PasswordSalt = creds.PasswordSalt;
            }

            _users = users;

            return Task.FromResult(_users);
        }
    }

    public async Task<User> AddNewUser(UserDTO request, List<Claim> claims)
    {
        User? existingUser = (await GetUsers()).FirstOrDefault(x => x.Username == request.Username);
        if (existingUser != null) return existingUser;

        FakeCreds creds = GenerateFakeCreds(request.Username);
        User newUser = new()
        {
            PasswordHash = creds.PasswordHash,
            PasswordSalt = creds.PasswordSalt,
            Username = request.Username,
            Claims = claims
        };

        _users!.Add(newUser);

        return _users.First(x => x.Username == request.Username);
    }

    private FakeCreds GenerateFakeCreds(string password)
    {
        _cryptoHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        return new FakeCreds()
        {
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        };

    }

    public async Task AssignRefreshToken(string username, IRefreshToken refreshToken)
    {
        User? storedUser = await GetUserByUsername(username);
        if(storedUser == null)
        {
            throw new NullReferenceException("There was no such user stored in user provider storage");
        }
        else
        {
            storedUser.RefreshToken = refreshToken.Token;
            storedUser.TokenCreated = refreshToken.Created;
            storedUser.TokenExpires = refreshToken.Expires;
        }
    }

    private class FakeCreds
    {
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    }
}
